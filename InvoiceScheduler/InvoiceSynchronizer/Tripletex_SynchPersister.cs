using EconomicDataTransfer_Consumer_Economic.DraftInvoice;
using InvoiceScheduler_Consumer_AcentioCRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TripleTexDataTransfer_Consumer_TripleTex.Customer;
using TripleTexDataTransfer_Consumer_TripleTex.Invoice;
using TripleTexDataTransfer_Consumer_TripleTex.Order;
using TripleTexDataTransfer_Consumer_TripleTex.Session;
using Acentio = InvoiceScheduler_Consumer_AcentioCRM;
using Tripletex = TripleTexDataTransfer_Consumer_TripleTex;

namespace InvoiceScheduler_Consumer
{
    internal class Tripletex_SynchPersister
    {
        private Settings Settings;
        private readonly string key = "tripletex (no)";
        public Tripletex_SynchPersister(Settings settings)
        {
            this.Settings = settings;
        }
        public async Task SaveTripleTexData(Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {
            var tripletex_client = await GetTripletexClient();
            var acentiocrm_client = GetAcentioClient();
                        
            var invoices = data_acentio.Invoices.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && (a.cInvoicedFrom??"").ToLower() == key)).ToList();
            var creditnotes = data_acentio.CreditNotes.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && a.cInvoicedFrom == key)).ToList();

            if (Settings.PersistTripleTexInvoices.Skip) return;
            Settings.PersistTripleTexInvoices.Log.Add(GetLogEntry("Start"));
                        
            await SaveTripletexDraftInvoices(tripletex_client, acentiocrm_client, invoices, creditnotes, data_acentio, data_tripletex);
            await SaveTripletexBookedInvoices(tripletex_client, acentiocrm_client, invoices, creditnotes, data_acentio, data_tripletex);
            
            Settings.PersistTripleTexInvoices.Log.Add(GetLogEntry("End"));
        }
        private async Task SaveTripletexBookedInvoices(HttpClient tripletex_client, HttpClient acentiocrm_client, List<Acentio.InvoiceResponseData> invoices, List<CreditNoteResponseData> creditnotes, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)        
        {
            await SaveEconomicBookedInvoice(tripletex_client, acentiocrm_client, invoices, data_acentio, data_tripletex);
            await SaveTripletexBookedCreditNote(tripletex_client, acentiocrm_client, creditnotes, data_acentio, data_tripletex);

        }
        private async Task SaveEconomicBookedInvoice(HttpClient tripletex_client, HttpClient acentiocrm_client, List<Acentio.InvoiceResponseData> invoices, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {
            Settings.PersistTripleTexInvoices.Log.Add(GetLogEntry("Start - Booked"));
            var tripletex_booked_invoices = invoices.Where(r => (r.status ?? "").ToLower() == "ready for transfer").ToList();
            foreach (var invoice in tripletex_booked_invoices)
            {
                try
                {
                    var crm_account = data_acentio.Accounts.list.FirstOrDefault(r => r.id == invoice.accountId);
                    var tripletex_account = data_tripletex.Customers.values.FirstOrDefault(r => r.customerNumber.ToString() == crm_account.cERPCustomerNo);

                    var tripletex_response = await TripletexBookedInvoice(invoice, Convert.ToInt64(invoice.cERPDraftInvoiceNo), tripletex_account.id, invoice.dateDue,invoice.dateInvoiced, tripletex_client, data_acentio, data_tripletex);
                    if (tripletex_response.Success) await SaveCRMBookedInvoice(tripletex_response, invoice, acentiocrm_client);
                    else await ResetCRMInvoice(invoice, tripletex_response.Info, acentiocrm_client);

                    await SaveNotes("Invoice", invoice.id, tripletex_response.Info, acentiocrm_client, Settings.PersistTripleTexInvoices.Warning, Settings.PersistTripleTexInvoices.Error);

                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMInvoice(invoice, info, acentiocrm_client);
                }
            }
            Settings.PersistTripleTexInvoices.Log.Add(GetLogEntry("End - Booked"));
        }

        private async Task<TripletexInvoiceWrapper> TripletexBookedInvoice(Acentio.InvoiceResponseData crm_invoice, long erpInvoiceId, long  customerid,string invoiceduedate, string invoicedate, HttpClient tripletex_client, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {

            var url = "invoice?sendToCustomer=true";

            var syncresponse = new Tripletex.Invoice.TripletexInvoiceWrapper();
            var invoice = new Tripletex.Invoice.InvoiceBooked(customerid, erpInvoiceId, invoiceduedate,invoicedate);
            
            try
            {
                var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                var json = JsonSerializer.Serialize(invoice, options);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await tripletex_client.PostAsync(url, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responsedocument = JsonSerializer.Deserialize<Tripletex.Invoice.InvoiceResponseWrapper>(jsonResponse);
                var id = (responsedocument == null ? "0" : responsedocument.value.id.ToString());

                if (response.IsSuccessStatusCode)
                {                    
                    Settings.PersistTripleTexInvoices.NewIds.Add(new Settings.EntityData(id));
                    syncresponse.Invoice = responsedocument.value;
                    syncresponse.Success = true;
                    syncresponse.Info.Add("Tripletex Booked Invoice: " + "Invoice " + id + " successfully synchronized with Tripletex.");
                    crm_invoice.cPDFInvoice = responsedocument.value != null ? "https://tripletex.no/v2/invoice/" + responsedocument.value.id + "/pdf" : null;
                    crm_invoice.cERPInvoiceNo = id;
                    crm_invoice.status = "Sent";
                    
                    return syncresponse;
                }
                else
                {
                    syncresponse.Invoice = null;
                    syncresponse.Success = false;
                    syncresponse.Info.Add("Tripletex Booked Invoice: " + "Invoice " + id + " failed in synchronizaation with Tripletex: " + jsonResponse);
                    Settings.PersistTripleTexInvoices.Warning.Add(GetLogEntity(id, jsonResponse));
                }
            }
            catch (Exception ex)
            {

                syncresponse.Order = null;
                syncresponse.Success = false;
                syncresponse.Info.Add("Tripletex Booked Invoice: " + "Invoice failed in synchronizaation with Tripletex: " + ex.Message);

                Settings.PersistTripleTexInvoices.Error.Add(GetLogEntity(crm_invoice.id, ex.Message + " " + ex.StackTrace));
            }

            return syncresponse;
        }
        private async Task SaveCRMBookedInvoice(Tripletex.Invoice.TripletexInvoiceWrapper tripletex_invoice_wrapper, Acentio.InvoiceResponseData invoice, HttpClient acentiocrm_client)
        {
            var document = "Invoice";
            var url = "api/v1/" + document + "/" + invoice.id;
            
            StringContent content = new StringContent(JsonSerializer.Serialize(invoice), Encoding.UTF8, "application/json");
            try
            {

                var response = await acentiocrm_client.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                    var error = response.Headers.FirstOrDefault(r => r.Key == "X-Status-Reason").Value.FirstOrDefault();
                    Settings.PersistTripleTexInvoices.Warning.Add(GetLogEntity(invoice.id, error));
                    tripletex_invoice_wrapper.Info.Add("Save CRM Booked Invoice: " + error);
                }
            }
            catch (Exception ex)
            {
                Settings.PersistTripleTexInvoices.Error.Add(GetLogEntity(invoice.id, ex.Message + " " + ex.StackTrace));
                tripletex_invoice_wrapper.Info.Add("Save CRM Booked Invoice: " + ex.Message);
            }
        }

        private async Task SaveTripletexBookedCreditNote(HttpClient tripletex_client, HttpClient acentiocrm_client, List<CreditNoteResponseData> creditnotes, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {
            //throw new NotImplementedException();
        }

   

        private async Task SaveTripletexDraftInvoices(HttpClient tripletex_client, HttpClient acentiocrm_client, List<Acentio.InvoiceResponseData> invoices, List<CreditNoteResponseData> creditnotes, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {            
            await HandleNewInvoices(tripletex_client, acentiocrm_client, invoices, data_acentio, data_tripletex);
            await HandleUpdatedInvoices(tripletex_client, acentiocrm_client, invoices, data_acentio, data_tripletex);
      
            //await HandleNewCreditNotes(tripletex_client, acentiocrm_client, creditnotes, data_acentio, data_tripletex);
            //await HandleUpdatedCreditNotes(tripletex_client, acentiocrm_client, creditnotes, data_acentio, data_tripletex);
                        

        }
        private async Task HandleUpdatedInvoices(HttpClient tripletex_client, HttpClient acentiocrm_client, List<Acentio.InvoiceResponseData> invoices, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {
            //Get All invoices not already synched
            var updatedinvoices = invoices.Where(r => !string.IsNullOrEmpty(r.cERPDraftInvoiceNo) && data_tripletex.Orders.values.Any(e => e.id.ToString() == r.cERPDraftInvoiceNo)).ToList();

            foreach (var invoice in updatedinvoices)
            {
                try
                {   
                    var existingorder = data_tripletex.Orders.values.FirstOrDefault(r => r.id.ToString() == invoice.cERPDraftInvoiceNo);

                    var updatedorder = GetTripletexOrder(invoice, existingorder, data_acentio, data_tripletex);


                    //    var updatedinvoice = GetTripletexInvoice(invoice, existinginvoice, data_acentio, data_tripletex, tripletex_response.Order);

                    //  await SaveTripletexDraftInvoice(tripletex_response, invoice, updatedinvoice, tripletex_client);
                    var tripletex_response = await SaveTripletexOrder(invoice, updatedorder, tripletex_client);
                    if (tripletex_response.Success) await SaveCRMDraftInvoice(tripletex_response, invoice, acentiocrm_client);
                    else await ResetCRMInvoice(invoice, tripletex_response.Info, acentiocrm_client);
                    
                    await SaveNotes("Invoice", invoice.id, tripletex_response.Info, acentiocrm_client, Settings.PersistTripleTexInvoices.Warning, Settings.PersistTripleTexInvoices.Error);
                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMInvoice(invoice, info, acentiocrm_client);
                    await SaveNotes("Invoice", invoice.id, info, acentiocrm_client, Settings.PersistTripleTexInvoices.Warning, Settings.PersistTripleTexInvoices.Error);
                }

            }
        }

        private async Task<Tripletex.Order.OrderLineResponseWrapper> GetOrderLine(int id, HttpClient tripletex_client)
        {
            var url = "order/orderline/" + id + "?fields=*";
            var response = await tripletex_client.GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<Tripletex.Order.OrderLineResponseWrapper>(jsonResponse);

                return data;

            }
            return null;
        }
        private async Task<Tripletex.Order.OrderResponseWrapper> GetOrder(string id, HttpClient client)
        {
            var url = "order/" + id + "?fields=*" + ",orderLines(*)";
            var response = await client.GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<Tripletex.Order.OrderResponseWrapper>(jsonResponse);

                return data;

            }
            return null;

        }
        private async Task HandleNewInvoices(HttpClient tripletex_client, HttpClient acentiocrm_client, List<Acentio.InvoiceResponseData> invoices, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {
            //Get All invoices not already synched
            var newinvoices = invoices.Where(r => string.IsNullOrEmpty(r.cERPDraftInvoiceNo));

            foreach (var invoice in newinvoices)
            {
                try
                {               
                    // var newinvoice = GetTripletexInvoice(invoice, null, data_acentio, data_tripletex, tripletex_response.Order);
                    //await SaveTripletexDraftInvoice(tripletex_response, invoice, newinvoice, tripletex_client);
                    var neworder = GetTripletexOrder(invoice, null, data_acentio, data_tripletex);
                    var tripletex_response = await SaveTripletexOrder(invoice, neworder, tripletex_client);                        
                    if (tripletex_response.Success) await SaveCRMDraftInvoice(tripletex_response, invoice, acentiocrm_client);
                    else await ResetCRMInvoice(invoice, tripletex_response.Info, acentiocrm_client);
             
                    await SaveNotes("Invoice", invoice.id, tripletex_response.Info, acentiocrm_client, Settings.PersistTripleTexInvoices.Warning, Settings.PersistTripleTexInvoices.Error);

                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMInvoice(invoice, info, acentiocrm_client);
                    await SaveNotes("Invoice", invoice.id, info, acentiocrm_client, Settings.PersistTripleTexInvoices.Warning, Settings.PersistTripleTexInvoices.Error);
                }
            }
        }

        private async Task<Tripletex.Invoice.TripletexInvoiceWrapper> SaveTripletexOrder(Acentio.InvoiceResponseData crm_invoice,  Tripletex.Order.OrderResponseData order, HttpClient tripletex_client)
        {
            var url = "order" + (order.id == 0 ? "" : "/" + order.id.ToString());

            var syncresponse = new Tripletex.Invoice.TripletexInvoiceWrapper();

            try
            {
                var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                var json = JsonSerializer.Serialize(order, options);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = (order.id == 0 ? await tripletex_client.PostAsync(url, content) : await tripletex_client.PutAsync(url, content));
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responsedocument = JsonSerializer.Deserialize<Tripletex.Order.OrderResponseWrapper>(jsonResponse);
                var id = (responsedocument == null ? "0" : responsedocument.value.id.ToString());
                
                if (response.IsSuccessStatusCode)
                {
                    responsedocument = await GetOrder(id, tripletex_client);

                    Settings.PersistTripleTexInvoices.NewIds.Add(new Settings.EntityData(id));
                    syncresponse.Order = responsedocument.value;
                    syncresponse.Success = true;
                    syncresponse.Info.Add("Save Tripletex Order: " + "Order " + id + " successfully synchronized with Tripletex.");
                    crm_invoice.cPDFInvoice = responsedocument.value.preliminaryInvoice!=null ? "https://tripletex.no/v2/invoice/" + responsedocument.value.preliminaryInvoice.id + "/pdf" : null; 
                    crm_invoice.cERPDraftInvoiceNo = id;

                    foreach (var ol in order.orderLines)
                    {
                          await SaveTripletexOrderLine(syncresponse, ol, tripletex_client);
                    }

                    return syncresponse;
                }
                else
                {
                    syncresponse.Order = null;
                    syncresponse.Success = false;
                    syncresponse.Info.Add("Save Tripletex Order: " + "Order " + id + " failed in synchronizaation with Tripletex: " + jsonResponse);
                    Settings.PersistTripleTexInvoices.Warning.Add(GetLogEntity(id, jsonResponse));
                }
            }
            catch (Exception ex)
            {

                syncresponse.Order = null;
                syncresponse.Success = false;
                syncresponse.Info.Add("Save Tripletex Order: " + "Order failed in synchronizaation with Tripletex: " + ex.Message);

                Settings.PersistTripleTexInvoices.Error.Add(GetLogEntity(crm_invoice.id, ex.Message + " " + ex.StackTrace));
            }

            return syncresponse;
        }
        private async Task SaveTripletexOrderLine(Tripletex.Invoice.TripletexInvoiceWrapper syncresponse, Tripletex.Order.OrderLine orderline, HttpClient tripletex_client)
        {
            var url = "/v2/order/orderline" + (orderline.id == 0 ? "" : "/" + orderline.id.ToString());

            var existingorderline = await GetOrderLine(orderline.id, tripletex_client);
            if (existingorderline != null) orderline.version = existingorderline.value.version;
            try
            {
                var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                var json = JsonSerializer.Serialize(orderline, options);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = (orderline.id == 0 ? await tripletex_client.PostAsync(url, content) : await tripletex_client.PutAsync(url, content));
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responsedocument = JsonSerializer.Deserialize<Tripletex.Order.OrderResponseWrapper>(jsonResponse);
                var id = (responsedocument == null ? "0" : responsedocument.value.id.ToString());

                if (response.IsSuccessStatusCode)
                {                   
                    Settings.PersistTripleTexInvoices.NewIds.Add(new Settings.EntityData(id));
                    syncresponse.Success = true;
                    syncresponse.Info.Add("Save Tripletex Order Line: " + "Order Line:" + id + " successfully synchronized with Tripletex.");
                    return;
                }
                else
                {
                    syncresponse.Success = false;
                    syncresponse.Info.Add("Save Tripletex Order Line: " + "Order Line:" + id + " failed in synchronization with Tripletex: " + jsonResponse);
                    Settings.PersistTripleTexInvoices.Warning.Add(GetLogEntity(id, jsonResponse));
                }
            }
            catch (Exception ex)
            {                                
                syncresponse.Success = false;
                syncresponse.Info.Add("Save Tripletex Order Line: " + "Order Line failed in synchronization with Tripletex: " + ex.Message);

                Settings.PersistTripleTexInvoices.Error.Add(GetLogEntity(orderline.id.ToString(), ex.Message + " " + ex.StackTrace));
            }
            
        }

      

        private async Task SaveTripletexDraftInvoice(Tripletex.Invoice.TripletexInvoiceWrapper syncresponse, Acentio.InvoiceResponseData crminvoice, Tripletex.Invoice.InvoiceResponseData invoice, HttpClient tripletex_client)
        {
            var url = "invoice" + (invoice.id == 0 ? "" : "/" + invoice.id.ToString());
           

            try
            {
                var options = new JsonSerializerOptions{DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull                };
                var json = JsonSerializer.Serialize(invoice, options);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await tripletex_client.PostAsync(url, content); //: await tripletex_client.PutAsync(url, content));
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responsedocument = JsonSerializer.Deserialize<Tripletex.Invoice.InvoiceResponseWrapper>(jsonResponse);
                var id = (responsedocument == null ? "0" : responsedocument.value.id.ToString());
                if (response.IsSuccessStatusCode)
                {                   
                    syncresponse.Invoice = responsedocument.value;
                    syncresponse.Success = true;
                    syncresponse.Info.Add("Save Tripletex Draft: " + "Invoice " + id + " successfully synchronized with Tripletex.");
                    crminvoice.cERPDraftInvoiceNo = id;
                    crminvoice.cPDFInvoice = "https://tripletex.no/v2/invoice/" + id + "/pdf";
                    Settings.PersistTripleTexInvoices.NewIds.Add(new Settings.EntityData(id));
                }
                else
                {
                    syncresponse.Invoice = null;
                    syncresponse.Success = false;
                    syncresponse.Info.Add("Save Tripletex Draft: " + "Invoice " + id + " failed in synchronizaation with Tripletex: " + jsonResponse);
                    Settings.PersistTripleTexInvoices.Warning.Add(GetLogEntity(id, jsonResponse));
                }
            }
            catch (Exception ex)
            {

                syncresponse.Invoice = null;
                syncresponse.Success = false;
                syncresponse.Info.Add("Save Tripletex Draft: " + "Invoice failed in synchronizaation with Tripletex: " + ex.Message);
                Settings.PersistTripleTexInvoices.Error.Add(GetLogEntity(crminvoice.id, ex.Message + " " + ex.StackTrace));
            }      
        }

        private async Task SaveCRMDraftInvoice(Tripletex.Invoice.TripletexInvoiceWrapper tripletex_invoice_wrapper, Acentio.InvoiceResponseData invoice, HttpClient acentiocrm_client)
        {
            var document = "Invoice";
            var url = "api/v1/" + document + "/" + invoice.id;

            invoice.isLocked = false;
            invoice.isNotActual = true;

            StringContent content = new StringContent(JsonSerializer.Serialize(invoice), Encoding.UTF8, "application/json");
            try
            {

                var response = await acentiocrm_client.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {

                }
                else
                {
                    var error = response.Headers.FirstOrDefault(r => r.Key == "X-Status-Reason").Value.FirstOrDefault();
                    Settings.Invoices.Warning.Add(GetLogEntity(invoice.id, error));
                    tripletex_invoice_wrapper.Info.Add("Save CRM Draft Invoice: " + error);

                }
            }
            catch (Exception ex)
            {
                Settings.Invoices.Error.Add(GetLogEntity(invoice.id, ex.Message + " " + ex.StackTrace));
                tripletex_invoice_wrapper.Info.Add("Save CRM Draft Invoice: " + ex.Message);
            }
        }
        private async Task ResetCRMInvoice(Acentio.InvoiceResponseData invoice, IList<string> info, HttpClient acentiocrm_client)
        {
            var document = "Invoice";
            var url = "api/v1/" + document + "/" + invoice.id;

            invoice.status = "Error";
            invoice.isLocked = false;
            invoice.isNotActual = true;

            StringContent content = new StringContent(JsonSerializer.Serialize(invoice), Encoding.UTF8, "application/json");
            try
            {

                var response = await acentiocrm_client.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                    var error = response.Headers.FirstOrDefault(r => r.Key == "X-Status-Reason").Value.FirstOrDefault();
                    Settings.Invoices.Warning.Add(GetLogEntity(invoice.id, error));
                    info.Add("Reset CRM Invoice: " + "Error resetting: " + error);
                }
            }
            catch (Exception ex)
            {
                Settings.Invoices.Error.Add(GetLogEntity(invoice.id, ex.Message + " " + ex.StackTrace));
                info.Add("Reset CRM Invoice: " + "error resetting: " + ex.Message);
            }

        }

        private Tripletex.Order.OrderResponseData GetTripletexOrder(Acentio.InvoiceResponseData invoice, Tripletex.Order.OrderResponseData existing, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {
            var target = (existing == null ? new Tripletex.Order.OrderResponseData(): existing);

            var crm_account = data_acentio.Accounts.list.FirstOrDefault(r => r.id == invoice.accountId);
            var tripletex_account = data_tripletex.Customers.values.FirstOrDefault(r => r.customerNumber.ToString() == crm_account.cERPCustomerNo);

            if (target.customer == null) target.customer = new Tripletex.Order.Customer();
            target.customer.id = tripletex_account.id;

            target.orderDate = invoice.dateInvoiced;
            //target.deliveryDate = invoice.dateInvoiced;

            DateTime dt;
            target.invoiceComment = "";
            if (invoice.cAccruedStart != invoice.cAccruedEnd)
            {
                if (DateTime.TryParse(invoice.cAccruedStart, out dt))
                {
                    target.invoiceComment += dt.ToString("dd-MM-yyyy");
                }
                if (DateTime.TryParse(invoice.cAccruedEnd, out dt))
                {
                    target.invoiceComment += " - " + dt.ToString("dd-MM-yyyy");
                }
            }
            
            GetTripletexOrderLines(invoice, target, data_acentio, data_tripletex);

            return target;

        }

        private void GetTripletexOrderLines(Acentio.InvoiceResponseData invoice, OrderResponseData target, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex)
        {
            if (target.orderLines==null) target.orderLines = new List<Tripletex.Order.OrderLine>();
            var invoiceitems = data_acentio.InvoiceItems.list.Where(r => r.invoiceId == invoice.id).ToList();
            foreach (var line in invoiceitems)
            {
                var existingline = target.orderLines.FirstOrDefault(r=>r.sortIndex == line.order-1);
                var newline =  (existingline!=null ? existingline :new Tripletex.Order.OrderLine());
                Tripletex.Product.ProductResponseData tripletex_product = null;
                if (!string.IsNullOrEmpty(line.productId))
                {
                    var crm_product = data_acentio.Products.list.FirstOrDefault(r => r.id == line.productId);
                    if (crm_product != null)
                    {
                        tripletex_product = data_tripletex.Products.values.FirstOrDefault(r => r.id.ToString() == crm_product.cSyncid);
                        if (tripletex_product != null)
                        {
                            if (newline.product == null) newline.product = new Tripletex.Order.Product();
                            newline.product.id = tripletex_product.id;
                        }
                    }
                }

                newline.description = (string.IsNullOrWhiteSpace(line.description) ? (tripletex_product == null ? (string.IsNullOrWhiteSpace(line.name) ? "N/A" : line.name) : tripletex_product.name) : line.description);
                newline.count = line.quantity;
                
                newline.sortIndex = line.order-1;
                //newline.amountExcludingVatCurrency = RoundDown(line.unitPrice, 2);
                newline.unitPriceExcludingVatCurrency = RoundDown(line.unitPrice, 2);
                newline.discount = 0;

                target.orderLines.Add(newline);

            }
        }

        private Tripletex.Invoice.InvoiceResponseData GetTripletexInvoice(Acentio.InvoiceResponseData invoice,Tripletex.Invoice.InvoiceResponseData existing, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex, Tripletex.Order.OrderResponseData order)
        {
            var target = (existing == null ? new Tripletex.Invoice.InvoiceResponseData() : existing);

            var crm_account = data_acentio.Accounts.list.FirstOrDefault(r => r.id == invoice.accountId);
            var tripletex_account = data_tripletex.Customers.values.FirstOrDefault(r => r.customerNumber.ToString() == crm_account.cERPCustomerNo);

            if (target.customer == null) target.customer = new Tripletex.Invoice.Customer();
            target.customer.id = tripletex_account.id;

            target.comment = "";
         
            DateTime dt;
            if (invoice.cAccruedStart != invoice.cAccruedEnd)
            {
                if (DateTime.TryParse(invoice.cAccruedStart, out dt))
                {
                    target.comment += dt.ToString("dd-MM-yyyy");
                }
                if (DateTime.TryParse(invoice.cAccruedEnd, out dt))
                {
                    target.comment += " - " + dt.ToString("dd-MM-yyyy");
                }
            }

            target.invoiceDate = invoice.dateInvoiced;
            target.invoiceDueDate = invoice.dateDue;
            target.orders = new List<Tripletex.Invoice.Order>();
            target.orders.Add(new Tripletex.Invoice.Order() { id = order.id });
            
            //target.invoiceRemark = string.IsNullOrWhiteSpace(invoice.purchaseOrderReference) ? invoice.number : invoice.purchaseOrderReference;
            target.isCreditNote = false;
            
            GetTripletexInvoiceLines(invoice, target, data_acentio, data_tripletex, order);

            return target;
        }

        private void GetTripletexInvoiceLines(Acentio.InvoiceResponseData invoice, Tripletex.Invoice.InvoiceResponseData target, Acentio.CombinedDataSet data_acentio, Tripletex.CombinedDataSet data_tripletex, Tripletex.Order.OrderResponseData order)
        {
            if (target.orderLines== null) target.orderLines = new List<Tripletex.Invoice.OrderLine>();
            var invoiceitems = data_acentio.InvoiceItems.list.Where(r => r.invoiceId == invoice.id).ToList();
            foreach (var line in order.orderLines)
            {   
                var newline = new Tripletex.Invoice.OrderLine();

                newline.product = new Tripletex.Invoice.Product();
                newline.product.id = line.product.id;
                
                newline.description = line.description;
                
                newline.orderedQuantity = line.orderedQuantity;
                newline.sortIndex = line.sortIndex;
                newline.amountExcludingVatCurrency = line.amountExcludingVatCurrency;
                newline.unitPriceExcludingVatCurrency = line.unitPriceExcludingVatCurrency;
                newline.discount = line.discount;

                newline.order = new Tripletex.Invoice.Order();
                newline.order.id = order.id;
                    
                target.orderLines.Add(newline);

            }
        }

        private async Task SaveNotes(string type, string id, IList<string> info, HttpClient acentiocrm_client, IList<Settings.EntityData> warnings, IList<Settings.EntityData> errors)
        {
            if (info == null) return;
            foreach (var note in info)
            {
                await SaveNote(type, id, note, acentiocrm_client, warnings, errors);
            }
        }
        private async Task SaveNote(string parentType, string id, string info, HttpClient acentiocrm_client, IList<Settings.EntityData> warnings, IList<Settings.EntityData> errors)
        {
            if (string.IsNullOrEmpty(info)) return;

            var document = "Note";
            var url = "api/v1/" + document;

            var note = new StreamNote();
            note.type = "Post";
            note.parentType = parentType;
            note.parentId = id;
            note.post = info;
            StringContent content = new StringContent(JsonSerializer.Serialize(note), Encoding.UTF8, "application/json");
            try
            {

                var response = await acentiocrm_client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                    var error = response.Headers.FirstOrDefault(r => r.Key == "X-Status-Reason").Value.FirstOrDefault();
                    warnings.Add(GetLogEntity(id, "Note: " + error));

                }
            }
            catch (Exception ex)
            {
                errors.Add(GetLogEntity(id, "Note: " + ex.Message + " " + ex.StackTrace));
            }
        }
        public decimal RoundDown(decimal i, double decimalPlaces)
        {
            var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(i * power) / power;
        }
        private Settings.EntityData GetLogEntity(string id, string msg)
        {
            return new Settings.EntityData(id, DateTime.Now.ToShortTimeString() + ": " + msg);
        }
        private string GetLogEntry(string msg)
        {
            return DateTime.Now.ToLongTimeString() + ": " + msg;


        }
        private HttpClient GetAcentioClient()
        {
            var url = "https://crm.acentio.com";
            var apikey = "68dcf5cdc467b220a52ecc4c537469c9";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", apikey);
            client.BaseAddress = new Uri(url);
            return client;
        }
        private async Task<HttpClient> GetTripletexClient()
        {

            var url = "https://tripletex.no/v2/";            
            var client = new HttpClient();

            client.BaseAddress = new Uri(url);

            Session session = await GetSession(client);
            if (session.Success)
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"0:{session.value.token}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                return client;
            }

            return null;
        }
        private async Task<Session> GetSession(HttpClient client)
        {

            var consumerToken = "eyJ0b2tlbklkIjo1OTkzLCJ0b2tlbiI6IjY1NjAzM2RjLTc2ODQtNDY1OC1hMmVkLTVlOTk0ZDJlNTliYyJ9";
            var employeeToken = "eyJ0b2tlbklkIjoyNjc0MDgzLCJ0b2tlbiI6ImRiNWRiYWRjLTlkYTgtNGY0NS1hMTFkLTkwODRmOTZiNTQyYiJ9";
            var expirationDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

            client.DefaultRequestHeaders.ExpectContinue = false;

            var url = $"token/session/:create" +
                      $"?consumerToken={Uri.EscapeDataString(consumerToken)}" +
                      $"&employeeToken={Uri.EscapeDataString(employeeToken)}" +
                      $"&expirationDate={Uri.EscapeDataString(expirationDate)}";

            var response = await client.PutAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responsedata = JsonSerializer.Deserialize<Session>(jsonResponse);

                return responsedata;
            }

            return new Session();

        }
    }
}