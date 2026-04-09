using EconomicDataTransfer_Consumer_Economic.BookedInvoice;
using EconomicDataTransfer_Consumer_Economic.DraftInvoice;
using EconomicDataTransfer_Consumer_Economic.Invoice;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.InvoiceRecord;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.Schedule;
using InvoiceScheduler_Consumer_AcentioCRM;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Acentio   = InvoiceScheduler_Consumer_AcentioCRM;
using AcentioDB = InvoiceScheduler.Acentio.DB.AcentioDBDTO;
using Economic = EconomicDataTransfer_Consumer_Economic;

namespace InvoiceScheduler_Consumer
{
    internal class Economic_SynchPersister
    {
        public Settings Settings { get; }
        
        public Economic_SynchPersister(Settings settings)
        {
            Settings = settings;
        }
     
        public async Task SaveEconomicData(Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            HttpClient economic_client = GetEconomicClient();
            HttpClient acentiocrm_client = GetAcentioClient();

            var invoices = data_acentio.Invoices.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && a.cInvoicedFrom == "E-conomic (DK)")).ToList();
            var creditnotes = data_acentio.CreditNotes.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && a.cInvoicedFrom == "E-conomic (DK)")).ToList();
            await SaveEconomicDraftInvoices(economic_client, acentiocrm_client, invoices,creditnotes, data_acentio, data_economic);
            await SaveEconomicBookedInvoices(economic_client, acentiocrm_client,invoices, creditnotes, data_acentio, data_economic);
        }

        private async Task SaveEconomicBookedInvoices(HttpClient economic_client, HttpClient acentiocrm_client, List<Acentio.InvoiceResponseData> economic_invoices, List<CreditNoteResponseData> creditnotes, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            await SaveEconomicBookedInvoice(economic_client, acentiocrm_client, economic_invoices, data_acentio, data_economic);
            await SaveEconomicBookedCreditNote(economic_client, acentiocrm_client, creditnotes, data_acentio, data_economic);
        }
        private async Task SaveEconomicBookedInvoice(HttpClient economic_client, HttpClient acentiocrm_client, List<Acentio.InvoiceResponseData> economic_invoices, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            Settings.PersistEconomicInvoices.Log.Add(GetLogEntry("Start - Booked"));
            var economic_booked_invoices = economic_invoices.Where(r => (r.status ?? "").ToLower() == "ready for transfer").ToList();
            foreach (var invoice in economic_booked_invoices)
            {
                try {
                    var economic_response = await SaveEconomicBookedInvoice(invoice.id, invoice.cERPDraftInvoiceNo, invoice.accountId, economic_client, data_acentio, data_economic);
                    if (economic_response.Success) await SaveCRMBookedInvoice(economic_response, invoice, acentiocrm_client);
                    else await ResetCRMInvoice(invoice, economic_response.Info, acentiocrm_client);
                    
                    await SaveNotes("Invoice", invoice.id, economic_response.Info, acentiocrm_client, Settings.Invoices.Warning, Settings.Invoices.Error);

                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMInvoice(invoice, info, acentiocrm_client);
                }
            }
            Settings.PersistEconomicInvoices.Log.Add(GetLogEntry("End - Booked"));
        }
        private async Task SaveCRMBookedInvoice(Economic.Invoice.EconomicInvoiceWrapper economic_invoice_wrapper, Acentio.InvoiceResponseData invoice, HttpClient acentiocrm_client)
        {
            var document = "Invoice";
            var url = "api/v1/" + document + "/" + invoice.id;
            var economic_invoice = economic_invoice_wrapper.Invoice;

            if (economic_invoice != null)
            {
                invoice.cERPInvoiceNo = economic_invoice.bookedInvoiceNumber.ToString();
                invoice.cPDFInvoice = economic_invoice.pdf != null ? economic_invoice.pdf.download : string.Empty;
                invoice.status = "Sent";
            }


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
                    economic_invoice_wrapper.Info.Add("Save CRM Booked Invoice: " + error);
                }
            }
            catch (Exception ex)
            {
                Settings.Invoices.Error.Add(GetLogEntity(invoice.id, ex.Message + " " + ex.StackTrace));
                economic_invoice_wrapper.Info.Add("Save CRM Booked Invoice: " + ex.Message);
            }
        }
        private async Task SaveEconomicBookedCreditNote(HttpClient economic_client, HttpClient acentiocrm_client, List<CreditNoteResponseData> creditnotes, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            Settings.PersistEconomicInvoices.Log.Add(GetLogEntry("Start - Booked"));
            var economic_booked_creditnotes = creditnotes.Where(r => (r.status ?? "").ToLower() == "ready for transfer").ToList();
            foreach (var creditnote in economic_booked_creditnotes)
            {
                try
                {
                    var economic_response = await SaveEconomicBookedInvoice(creditnote.id, creditnote.cERPDraftCreditNoteNo, creditnote.accountId, economic_client, data_acentio, data_economic);
                    if (economic_response.Success) await SaveCRMBookedCreditNote(economic_response, creditnote, acentiocrm_client);
                    else await ResetCRMCreditNote(creditnote, economic_response.Info, acentiocrm_client);

                    await SaveNotes("CreditNote", creditnote.id, economic_response.Info, acentiocrm_client, Settings.Invoices.Warning, Settings.Invoices.Error);

                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMCreditNote(creditnote, info, acentiocrm_client);
                }
            }
            Settings.PersistEconomicInvoices.Log.Add(GetLogEntry("End - Booked"));
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

        private HttpClient GetEconomicClient()
        {
            var url = "https://restapi.e-conomic.com";
            var XAppSecretToken = "Qr7MfqkrExs8dqsEHUkcxECR0TqFgfvBEYzm6fVZNAI";
            var XAgreementGrantToken = "2v9MJy8N9Tz9Z3lzpqKTbzM5qcIJt3zZDOT3dYP3JP8";            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-AppSecretToken", XAppSecretToken);
            client.DefaultRequestHeaders.Add("X-AgreementGrantToken", XAgreementGrantToken);

            client.BaseAddress = new Uri(url);
            return client;
        }

        private async Task SaveEconomicDraftInvoices(HttpClient economic_client, HttpClient acentiocrm_client, IList<Acentio.InvoiceResponseData> invoices, List<CreditNoteResponseData> creditnotes, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            if (Settings.PersistEconomicInvoices.Skip) return;
            Settings.PersistEconomicInvoices.Log.Add(GetLogEntry("Start"));

            await HandleNewInvoices(economic_client, acentiocrm_client, invoices, data_acentio, data_economic);
            await HandleUpdatedInvoices(economic_client, acentiocrm_client, invoices, data_acentio, data_economic);
            await HandleDeletedInvoices(economic_client, acentiocrm_client, invoices, data_acentio, data_economic);

            await HandleNewCreditNotes(economic_client, acentiocrm_client, creditnotes, data_acentio, data_economic);
            await HandleUpdatedCreditNotes(economic_client, acentiocrm_client, creditnotes, data_acentio, data_economic);
            Settings.PersistEconomicInvoices.Log.Add(GetLogEntry("End"));
        }


        private async Task HandleNewInvoices(HttpClient economic_client, HttpClient acentiocrm_client, IList<Acentio.InvoiceResponseData> invoices, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
           //Get All invoices not already synched
            var newinvoices = invoices.Where(r => string.IsNullOrEmpty(r.cERPDraftInvoiceNo));

            foreach (var invoice in newinvoices)
            {
                try
                {
                    

                    var newinvoice = GetEconomicInvoice(invoice, null, data_acentio, data_economic);

                    var economic_response = await SaveEconomicDraftInvoice(invoice.id, newinvoice, economic_client);
                    if (economic_response.Success) await SaveCRMDraftInvoice(economic_response, invoice, acentiocrm_client);
                    else await ResetCRMInvoice(invoice, economic_response.Info, acentiocrm_client);

                    await SaveNotes("Invoice", invoice.id, economic_response.Info, acentiocrm_client, Settings.PersistEconomicInvoices.Warning, Settings.PersistEconomicInvoices.Error);

                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMInvoice(invoice,info , acentiocrm_client);
                    await SaveNotes("Invoice", invoice.id, info, acentiocrm_client, Settings.Invoices.Warning, Settings.Invoices.Error);
                }
            }

        }
        private async Task HandleNewCreditNotes(HttpClient economic_client, HttpClient acentiocrm_client, IList<Acentio.CreditNoteResponseData> creditnotes, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            //Get All invoices not already synched
            var newcreditnotes = creditnotes.Where(r => string.IsNullOrEmpty(r.cERPDraftCreditNoteNo));

            foreach (var creditnote in newcreditnotes)
            {
                try
                {
                    var sourceinvoice = data_acentio.Invoices.list.First(r => r.id == creditnote.invoiceId);
                    var newcreditnote = GetEconomicCreditNote(creditnote, sourceinvoice,null, data_acentio, data_economic);

                    var economic_response = await SaveEconomicDraftInvoice(creditnote.id, newcreditnote, economic_client);
                    if (economic_response.Success) await SaveCRMDraftCreditNote(economic_response, creditnote, acentiocrm_client);
                    else await ResetCRMCreditNote(creditnote, economic_response.Info, acentiocrm_client);

                    await SaveNotes("CreditNote", creditnote.id, economic_response.Info, acentiocrm_client, Settings.CreditNotes.Warning, Settings.CreditNotes.Error);

                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMCreditNote(creditnote, info, acentiocrm_client);
                    await SaveNotes("CreditNote", creditnote.id, info, acentiocrm_client, Settings.CreditNotes.Warning, Settings.CreditNotes.Error);
                }
            }

        }

        private async Task HandleUpdatedInvoices(HttpClient economic_client, HttpClient acentiocrm_client, IList<Acentio.InvoiceResponseData> invoices, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            //Get All invoices not already synched
            var updatedinvoices = invoices.Where(r => !string.IsNullOrEmpty(r.cERPDraftInvoiceNo) && data_economic.Draftinvoices.collection.Any(e => e.draftInvoiceNumber.ToString() == r.cERPDraftInvoiceNo)).ToList();

            foreach (var invoice in updatedinvoices)
            {
                try
                {                    
                    var existinginvoice = data_economic.Draftinvoices.collection.FirstOrDefault(r => r.draftInvoiceNumber.ToString() == invoice.cERPDraftInvoiceNo);
                    var updatedinvoice = GetEconomicInvoice(invoice, existinginvoice, data_acentio, data_economic);

                    var economic_response = await SaveEconomicDraftInvoice(invoice.id, updatedinvoice, economic_client);
                    if (economic_response.Success) await SaveCRMDraftInvoice(economic_response, invoice, acentiocrm_client);
                    else await ResetCRMInvoice(invoice, economic_response.Info, acentiocrm_client);

                    await SaveNotes("Invoice", invoice.id, economic_response.Info, acentiocrm_client, Settings.PersistEconomicInvoices.Warning, Settings.PersistEconomicInvoices.Error);
                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMInvoice(invoice, info, acentiocrm_client);
                    await SaveNotes("Invoice", invoice.id, info, acentiocrm_client, Settings.PersistEconomicInvoices.Warning, Settings.PersistEconomicInvoices.Error);
                }

            }
        }
        private async Task HandleUpdatedCreditNotes(HttpClient economic_client, HttpClient acentiocrm_client, IList<Acentio.CreditNoteResponseData> creditnotes, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            //Get All invoices not already synched
            var updatedcreditnotes = creditnotes.Where(r => !string.IsNullOrEmpty(r.cERPDraftCreditNoteNo) && data_economic.Draftinvoices.collection.Any(e => e.draftInvoiceNumber.ToString() == r.cERPDraftCreditNoteNo)).ToList();

            foreach (var creditnote in updatedcreditnotes)
            {
                try
                {
                    var sourceinvoice = data_acentio.Invoices.list.First(r => r.id == creditnote.invoiceId);

                    var existingcreditnote = data_economic.Draftinvoices.collection.FirstOrDefault(r => r.draftInvoiceNumber.ToString() == creditnote.cERPDraftCreditNoteNo);
                    var updatedcreditnote = GetEconomicCreditNote(creditnote, sourceinvoice, existingcreditnote, data_acentio, data_economic);

                    var economic_response = await SaveEconomicDraftInvoice(creditnote.id, updatedcreditnote, economic_client);
                    if (economic_response.Success) await SaveCRMDraftCreditNote(economic_response, creditnote, acentiocrm_client);
                    else await ResetCRMCreditNote(creditnote, economic_response.Info, acentiocrm_client);

                    await SaveNotes("CreditNote", creditnote.id, economic_response.Info, acentiocrm_client, Settings.Invoices.Warning, Settings.Invoices.Error);
                }
                catch (Exception ex)
                {
                    var info = new List<string>();
                    info.Add(ex.Message);
                    await ResetCRMCreditNote(creditnote, info, acentiocrm_client);
                    await SaveNotes("CreditNote", creditnote.id, info, acentiocrm_client, Settings.Invoices.Warning, Settings.Invoices.Error);
                }

            }
        }
        private async Task HandleDeletedInvoices(HttpClient economic_client, HttpClient acentiocrm_client, IList<Acentio.InvoiceResponseData> invoices, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
           // throw new NotImplementedException();
        }

        private Economic.DraftInvoice.DraftInvoiceResponseData GetEconomicInvoice(Acentio.InvoiceResponseData invoice, Economic.DraftInvoice.DraftInvoiceResponseData existing, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {  
            var target = (existing == null ? new Economic.DraftInvoice.DraftInvoiceResponseData() : existing);

            var crm_account = data_acentio.Accounts.list.FirstOrDefault(r => r.id == invoice.accountId);
            var economic_account = data_economic.Customers.collection.FirstOrDefault(r => r.customerNumber.ToString() == crm_account.cERPCustomerNo);
            if (economic_account == null) throw new ArgumentException("Unknown ERP Customer");

            if (target.customer == null) target.customer = new Economic.DraftInvoice.Customer();
            target.customer.customerNumber = economic_account.customerNumber;
            
            if (target.notes == null) target.notes = new Economic.DraftInvoice.Notes();
            target.notes.heading = invoice.name;

            

            DateTime dt;
            target.notes.textLine1 = "";
            if (invoice.cAccruedStart != invoice.cAccruedEnd)
            {
                if (DateTime.TryParse(invoice.cAccruedStart, out dt))
                {
                    target.notes.textLine1 += dt.ToString("dd-MM-yyyy");
                }
                if (DateTime.TryParse(invoice.cAccruedEnd, out dt))
                {
                    target.notes.textLine1 += " - " + dt.ToString("dd-MM-yyyy");
                }
            }

          

            target.currency = invoice.amountCurrency;
            target.date = invoice.dateInvoiced;
            target.dueDate = invoice.dateDue;
            target.grossAmount = RoundDown(invoice.grandTotalAmount,2);
            
            if (target.layout==null) target.layout = new Economic.DraftInvoice.Layout();
            target.layout.layoutNumber = 21; ///?

            target.netAmount = RoundDown(invoice.amount,2);
            target.vatAmount = RoundDown(invoice.taxAmount,2);
            
            if (target.recipient==null) target.recipient = new Economic.DraftInvoice.Recipient();
            target.recipient.name = economic_account.name;
            target.recipient.address = invoice.billingAddressStreet;
            target.recipient.city = invoice.billingAddressCity;
            target.recipient.zip = invoice.billingAddressPostalCode;
            target.recipient.country = invoice.billingAddressCountry;
            target.recipient.ean = economic_account.ean;
            target.recipient.cvr = economic_account.corporateIdentificationNumber;
            
            if (target.recipient.vatZone == null) target.recipient.vatZone = new VatZone();
            target.recipient.vatZone.vatZoneNumber = economic_account.vatZone.vatZoneNumber;            
            if (target.references==null) target.references = new Economic.DraftInvoice.References();
            
            target.references.other = string.IsNullOrWhiteSpace(invoice.purchaseOrderReference) ? invoice.number : invoice.purchaseOrderReference;
            
            var crm_customercontact = data_acentio.Contacts.list.FirstOrDefault(r => r.id == invoice.billingContactId);
            if (crm_customercontact != null)
            {
                var economic_customercontact = data_economic.Contacts.collection.FirstOrDefault(r => r.customer.customerNumber == economic_account.customerNumber && (r.email ?? "").ToLower() == (crm_customercontact.emailAddress ?? "".ToLower()));
                if (economic_customercontact != null)
                {
                    target.references.customerContact = new CustomerContact() { customerContactNumber = economic_customercontact.customerContactNumber };
                }
            }


            var economic_salesperson = data_economic.Employees.collection.FirstOrDefault(r=>(r.name??"").ToLower() == (invoice.cAcentioInvoiceReference??"").ToLower());
            if (economic_salesperson != null)
            {
                if (target.references.salesPerson == null) target.references.salesPerson = new SalesPerson();
                target.references.salesPerson.employeeNumber = economic_salesperson.employeeNumber;
            }
            

            target.externalId = invoice.number;
            if (target.paymentTerms == null) target.paymentTerms = new Economic.DraftInvoice.PaymentTerms();
            var economic_paymentterm = data_economic.PaymentTerms.collection.FirstOrDefault(r => r.paymentTermsNumber == economic_account.paymentTerms.paymentTermsNumber);
            if (economic_paymentterm != null) {
                target.paymentTerms.paymentTermsNumber = economic_paymentterm.paymentTermsNumber;
                target.paymentTerms.paymentTermsType = economic_paymentterm.paymentTermsType;
                target.paymentTerms.daysOfCredit = economic_paymentterm.daysOfCredit;
                target.paymentTerms.description = economic_paymentterm.description;
                target.paymentTerms.name = economic_paymentterm.name;
            }
                        
            GetEconomicInvoiceLines(invoice, target, data_acentio, data_economic);

            return target;

        }
    

        private void GetEconomicInvoiceLines(Acentio.InvoiceResponseData invoice,DraftInvoiceResponseData target, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            target.lines = new List<Economic.DraftInvoice.Line>();            
            var invoiceitems = data_acentio.InvoiceItems.list.Where(r=>r.invoiceId == invoice.id).ToList();
            foreach (var line in invoiceitems)
            {
                var newline = new Economic.DraftInvoice.Line();

                Economic.Product.ProductResponseData economic_product = null;
                if (!string.IsNullOrEmpty(line.productId))
                {
                    var crm_product = data_acentio.Products.list.FirstOrDefault(r => r.id == line.productId);
                    if (crm_product != null)
                    {
                        economic_product = data_economic.Products.collection.FirstOrDefault(r => r.productNumber == crm_product.cSyncid);
                        if (economic_product != null)
                        {
                            if (newline.product == null) newline.product = new Economic.DraftInvoice.Product();
                            newline.product.productNumber = economic_product.productNumber;
                        }
                    }
                }
                              
                newline.description = string.IsNullOrWhiteSpace(line.description) ? (economic_product == null ? (string.IsNullOrWhiteSpace(line.name) ?  "N/A" : line.name): economic_product.name) : line.description;
                newline.quantity = line.quantity;
                newline.lineNumber = line.order;
                newline.totalNetAmount = RoundDown(line.amount,2);
                newline.unitNetPrice = RoundDown(line.unitPrice,2);
                newline.vatRate = line.taxRate;
                newline.sortKey = line.order;

                if (invoice.cAccruedStart != invoice.cAccruedEnd)
                {
                    DateTime dt;
                    if (DateTime.TryParse(invoice.cAccruedStart, out dt))
                    {
                        if (newline.accrual == null) newline.accrual = new Economic.DraftInvoice.Accrual();
                        newline.accrual.startDate = dt;
                    }
                    if (DateTime.TryParse(invoice.cAccruedEnd, out dt))
                    {
                        if (newline.accrual == null) newline.accrual = new Economic.DraftInvoice.Accrual();
                        newline.accrual.endDate = dt;
                    }
                }
                

             
                if (newline.departmentalDistribution == null) newline.departmentalDistribution = new Economic.DraftInvoice.DepartmentalDistribution();
           
                switch ((invoice.cPlatform??"").ToLower())
                {
                    case "a360":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "acentio 360").departmentNumber;
                        break;
                    case "truetrade":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "acentio truetrade").departmentNumber;
                        break;
                    case "biz":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "acentio biz").departmentNumber;
                        break;
                    case "ibistic":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "ibistic by acentio").departmentNumber;
                        break;
                    case "supplier":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "other").departmentNumber;
                        break;
                    default:
                        break;

                }


                target.lines.Add(newline);
                
            }

        }



        private Economic.DraftInvoice.DraftInvoiceResponseData GetEconomicCreditNote(Acentio.CreditNoteResponseData source,Acentio.InvoiceResponseData sourceinvoice, Economic.DraftInvoice.DraftInvoiceResponseData existing, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            var target = (existing == null ? new Economic.DraftInvoice.DraftInvoiceResponseData() : existing);

            var crm_account = data_acentio.Accounts.list.FirstOrDefault(r => r.id == source.accountId);
            var economic_account = data_economic.Customers.collection.FirstOrDefault(r => r.customerNumber.ToString() == crm_account.cERPCustomerNo);

            if (target.customer == null) target.customer = new Economic.DraftInvoice.Customer();
            target.customer.customerNumber = economic_account.customerNumber;

            if (target.notes == null) target.notes = new Economic.DraftInvoice.Notes();
            target.notes.heading = source.name;


            DateTime dt;
            target.notes.textLine1 = "";
            if (sourceinvoice.cAccruedStart != sourceinvoice.cAccruedEnd)
            {
                if (DateTime.TryParse(sourceinvoice.cAccruedStart, out dt))
                {
                    target.notes.textLine1 += dt.ToString("dd-MM-yyyy");
                }
                if (DateTime.TryParse(sourceinvoice.cAccruedEnd, out dt))
                {
                    target.notes.textLine1 += " - " + dt.ToString("dd-MM-yyyy");
                }
            }

            target.currency = source.amountCurrency;
            target.date = source.dateIssued;
            target.dueDate = source.dateDue;
            target.grossAmount = -1 * RoundDown(source.grandTotalAmount, 2);

            if (target.layout == null) target.layout = new Economic.DraftInvoice.Layout();
            target.layout.layoutNumber = 21; ///?

            target.netAmount = -1*RoundDown(source.amount, 2);
            target.vatAmount = -1 * RoundDown(source.taxAmount, 2);

            if (target.recipient == null) target.recipient = new Economic.DraftInvoice.Recipient();
            target.recipient.name = economic_account.name;
            target.recipient.ean = economic_account.ean;
            target.recipient.cvr = economic_account.corporateIdentificationNumber;

            if (target.recipient.vatZone == null) target.recipient.vatZone = new VatZone();
            target.recipient.vatZone.vatZoneNumber = economic_account.vatZone.vatZoneNumber;
            if (target.references == null) target.references = new Economic.DraftInvoice.References();

            target.references.other = !string.IsNullOrWhiteSpace(source.invoiceName) ? source.invoiceName : "";// invoice.purchaseOrderReference;

            var crm_customercontact = data_acentio.Contacts.list.FirstOrDefault(r => r.id == source.billingContactId);
            if (crm_customercontact != null)
            {
                var economic_customercontact = data_economic.Contacts.collection.FirstOrDefault(r => r.customer.customerNumber == economic_account.customerNumber && (r.email ?? "").ToLower() == (crm_customercontact.emailAddress ?? "".ToLower()));
                if (economic_customercontact != null)
                {
                    target.references.customerContact = new CustomerContact() { customerContactNumber = economic_customercontact.customerContactNumber };
                }
            }


            var economic_salesperson = data_economic.Employees.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == (sourceinvoice.cAcentioInvoiceReference ?? "").ToLower());
            if (economic_salesperson != null)
            {
                if (target.references.salesPerson == null) target.references.salesPerson = new SalesPerson();
                target.references.salesPerson.employeeNumber = economic_salesperson.employeeNumber;
            }


            target.externalId = source.number;
            if (target.paymentTerms == null) target.paymentTerms = new Economic.DraftInvoice.PaymentTerms();
            var economic_paymentterm = data_economic.PaymentTerms.collection.FirstOrDefault(r => r.paymentTermsNumber == economic_account.paymentTerms.paymentTermsNumber);
            if (economic_paymentterm != null)
            {
                target.paymentTerms.paymentTermsNumber = economic_paymentterm.paymentTermsNumber;
                target.paymentTerms.paymentTermsType = economic_paymentterm.paymentTermsType;
                target.paymentTerms.daysOfCredit = economic_paymentterm.daysOfCredit;
                target.paymentTerms.description = economic_paymentterm.description;
                target.paymentTerms.name = economic_paymentterm.name;
            }

            GetEconomicCreditNoteLines(source, sourceinvoice, target, data_acentio, data_economic);

            return target;

        }


        private void GetEconomicCreditNoteLines(Acentio.CreditNoteResponseData source, Acentio.InvoiceResponseData sourceinvoice, DraftInvoiceResponseData target, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            target.lines = new List<Economic.DraftInvoice.Line>();
            var creditnoteitems = data_acentio.CreditNoteItems.list.Where(r => r.creditNoteId == source.id).ToList();
            foreach (var line in creditnoteitems)
            {
                var newline = new Economic.DraftInvoice.Line();

                Economic.Product.ProductResponseData economic_product = null;
                if (!string.IsNullOrEmpty(line.productId))
                {
                    var crm_product = data_acentio.Products.list.FirstOrDefault(r => r.id == line.productId);
                    if (crm_product != null)
                    {
                        economic_product = data_economic.Products.collection.FirstOrDefault(r => r.productNumber == crm_product.cSyncid);
                        if (economic_product != null)
                        {
                            if (newline.product == null) newline.product = new Economic.DraftInvoice.Product();
                            newline.product.productNumber = economic_product.productNumber;
                        }
                    }
                }

                newline.description = string.IsNullOrWhiteSpace(line.description) ? (economic_product == null ? (string.IsNullOrWhiteSpace(line.name) ? "N/A" : line.name) : economic_product.name) : line.description;
                newline.quantity = line.quantity;
                newline.lineNumber = line.order;
                newline.totalNetAmount = -1 * RoundDown(line.amount, 2);
                newline.unitNetPrice = -1 * RoundDown(line.unitPrice, 2);
                newline.vatRate = line.taxRate;
                newline.sortKey = line.order;

                if (sourceinvoice.cAccruedStart != sourceinvoice.cAccruedEnd)
                {
                    DateTime dt;
                    if (DateTime.TryParse(sourceinvoice.cAccruedStart, out dt))
                    {
                        if (newline.accrual == null) newline.accrual = new Economic.DraftInvoice.Accrual();
                        newline.accrual.startDate = dt;
                    }
                    if (DateTime.TryParse(sourceinvoice.cAccruedEnd, out dt))
                    {
                        if (newline.accrual == null) newline.accrual = new Economic.DraftInvoice.Accrual();
                        newline.accrual.endDate = dt;
                    }
                }



                if (newline.departmentalDistribution == null) newline.departmentalDistribution = new Economic.DraftInvoice.DepartmentalDistribution();

                switch ((sourceinvoice.cPlatform ?? "").ToLower())
                {
                    case "a360":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "acentio 360").departmentNumber;
                        break;
                    case "truetrade":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "acentio truetrade").departmentNumber;
                        break;
                    case "biz":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "acentio biz").departmentNumber;
                        break;
                    case "ibistic":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "ibistic by acentio").departmentNumber;
                        break;
                    case "supplier":
                        newline.departmentalDistribution.departmentalDistributionNumber = data_economic.Departments.collection.FirstOrDefault(r => (r.name ?? "").ToLower() == "other").departmentNumber;
                        break;
                    default:
                        break;

                }


                target.lines.Add(newline);

            }

        }


        public decimal RoundDown(decimal i, double decimalPlaces)
        {
            var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(i * power) / power;
        }
        private async Task<Economic.Invoice.EconomicInvoiceWrapper> SaveEconomicBookedInvoice(string documentid, string draftid, string accountid, HttpClient economic_client, Acentio.CombinedDataSet data_acentio, Economic.CombinedDataSet data_economic)
        {
            var document = "invoices/booked";
            var url = "/" + document;
            var syncresponse = new EconomicInvoiceWrapper();
            try
            {
                var crm_account = data_acentio.Accounts.list.FirstOrDefault(r => r.id == accountid/*invoice.accountId*/);
                var economic_account = data_economic.Customers.collection.FirstOrDefault(r => r.customerNumber.ToString() == crm_account.cERPCustomerNo);

                var bookedInvoice = new Economic.BookedInvoice.BookedInvoice();
                bookedInvoice.draftInvoice.draftInvoiceNumber = Convert.ToInt32(draftid/*invoice.cERPDraftInvoiceNo*/);
                if (economic_account != null)
                {
                    if (!economic_account.eInvoicingDisabledByDefault && !string.IsNullOrEmpty(economic_account.ean)) bookedInvoice.sendBy = "ean";
                    else if (!string.IsNullOrWhiteSpace(crm_account.cEmailForPDFInvoicing)) bookedInvoice.sendBy = "email";
                    else throw new ArgumentException("no means of sending invoice? Active e-Invoicing:: " + !economic_account.eInvoicingDisabledByDefault +", ean: " + (economic_account.ean??"") + ",e-mail: " + (crm_account.cEmailForPDFInvoicing ?? "")); 
                }
                
                
                var options = new JsonSerializerOptions{DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull};
                StringContent content = new StringContent(JsonSerializer.Serialize(bookedInvoice, options), Encoding.UTF8, "application/json");             
                var response = await economic_client.PostAsync(url, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responsedocument = JsonSerializer.Deserialize<Economic.Invoice.InvoiceResponseData>(jsonResponse);
                var id = (responsedocument == null ? "0" : responsedocument.bookedInvoiceNumber.ToString());
                if (response.IsSuccessStatusCode)
                {
                    Settings.Invoices.NewIds.Add(new Settings.EntityData(id));
                    syncresponse.Invoice = responsedocument;
                    syncresponse.Success = true;
                    syncresponse.Info.Add("Save Economic Booked Invoice: " + "Invoice " + id + " successfully booked with E-conomic.");                    
                    return syncresponse;
                }
                else
                {                    
                    Settings.Invoices.Warning.Add(GetLogEntity(id, jsonResponse));
                    syncresponse.Success = false;
                    syncresponse.Info.Add("Save Economic Booked Invoice: Error: " + "Invoice " + id + " " + jsonResponse);

                }
            }
            catch (Exception ex)
            {
                Settings.Invoices.Error.Add(GetLogEntity(documentid, ex.Message + " " + ex.StackTrace));
                syncresponse.Success = false;
                syncresponse.Info.Add("Save Economic Booked Invoice: Error: " + "Invoice " + /*invoice.id*/documentid + " " + ex.Message);
            }

            return syncresponse;

        }
        private async Task<EconomicDraftInvoiceWrapper> SaveEconomicDraftInvoice(string crm_id, Economic.DraftInvoice.DraftInvoiceResponseData invoice, HttpClient economic_client)
        {
            var document = "invoices/drafts" + (invoice.draftInvoiceNumber ==0?"":"/" + invoice.draftInvoiceNumber.ToString());
            var url = "/" + document;
            var syncresponse = new EconomicDraftInvoiceWrapper();

            try
            {
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull                    
                };

                StringContent content = new StringContent(JsonSerializer.Serialize(invoice,options), Encoding.UTF8, "application/json");
                var response = (invoice.draftInvoiceNumber == 0 ? await economic_client.PostAsync(url, content): await economic_client.PutAsync(url, content)) ;
                var jsonResponse = await response.Content.ReadAsStringAsync();              
                var responsedocument = JsonSerializer.Deserialize<Economic.DraftInvoice.DraftInvoiceResponseData>(jsonResponse);
                var id = (responsedocument == null ? "0" : responsedocument.draftInvoiceNumber.ToString());
                if (response.IsSuccessStatusCode)
                {
                    Settings.Invoices.NewIds.Add(new Settings.EntityData(id));
                    syncresponse.Invoice = responsedocument;
                    syncresponse.Success = true;
                    syncresponse.Info.Add("Save Economic Draft: " + "Invoice " +id+" successfully synchronized with E-conomic.");
                    return syncresponse;
                }
                else
                {
                    syncresponse.Invoice = null;
                    syncresponse.Success = false;
                    syncresponse.Info.Add("Save Economic Draft: " + "Invoice " + id + " failed in synchronizaation with E-conomic: " + jsonResponse);
                    Settings.Invoices.Warning.Add(GetLogEntity(id, jsonResponse));
                }
            }
            catch (Exception ex)
            {

                syncresponse.Invoice = null;
                syncresponse.Success = false;
                syncresponse.Info.Add("Save Economic Draft: " + "Invoice failed in synchronizaation with E-conomic: " + ex.Message);

                Settings.Invoices.Error.Add(GetLogEntity(crm_id, ex.Message + " " + ex.StackTrace));
            }
            
            return syncresponse;
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
        private async Task ResetCRMCreditNote(Acentio.CreditNoteResponseData creditnote, IList<string> info, HttpClient acentiocrm_client)
        {
            var document = "CreditNote";
            var url = "api/v1/" + document + "/" + creditnote.id;

            creditnote.status = "Error";
            creditnote.isLocked = false;
            creditnote.isNotActual = true;

            StringContent content = new StringContent(JsonSerializer.Serialize(creditnote), Encoding.UTF8, "application/json");
            try
            {

                var response = await acentiocrm_client.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                    var error = response.Headers.FirstOrDefault(r => r.Key == "X-Status-Reason").Value.FirstOrDefault();
                    Settings.Invoices.Warning.Add(GetLogEntity(creditnote.id, error));
                    info.Add("Reset CRM CreditNote: " + "Error resetting: " + error);
                }
            }
            catch (Exception ex)
            {
                Settings.Invoices.Error.Add(GetLogEntity(creditnote.id, ex.Message + " " + ex.StackTrace));
                info.Add("Reset CRM CreditNote: " + "error resetting: " + ex.Message);
            }

        }

        private async Task SaveCRMDraftInvoice(Economic.DraftInvoice.EconomicDraftInvoiceWrapper economic_invoice_wrapper, Acentio.InvoiceResponseData invoice, HttpClient acentiocrm_client)
        {
            var document = "Invoice";
            var url = "api/v1/" + document + "/" + invoice.id;
            var economic_invoice = economic_invoice_wrapper.Invoice;

            if (economic_invoice != null)
            {
                invoice.cERPDraftInvoiceNo = economic_invoice.draftInvoiceNumber.ToString();
                invoice.cPDFInvoice = economic_invoice.pdf != null ? economic_invoice.pdf.download : string.Empty;
                invoice.cInvoiceNumber = economic_invoice.draftInvoiceNumber.ToString();

            }
            
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
                    economic_invoice_wrapper.Info.Add("Save CRM Draft Invoice: " + error);

                }
            }
            catch (Exception ex)
            {
                Settings.Invoices.Error.Add(GetLogEntity(invoice.id, ex.Message + " " + ex.StackTrace));
                economic_invoice_wrapper.Info.Add("Save CRM Draft Invoice: " + ex.Message);
            }          
        }
        private async Task SaveCRMDraftCreditNote(Economic.DraftInvoice.EconomicDraftInvoiceWrapper economic_invoice_wrapper, Acentio.CreditNoteResponseData creditnote, HttpClient acentiocrm_client)
        {
            var document = "CreditNote";
            var url = "api/v1/" + document + "/" + creditnote.id;
            var economic_invoice = economic_invoice_wrapper.Invoice;

            if (economic_invoice != null)
            {
                creditnote.cERPDraftCreditNoteNo = economic_invoice.draftInvoiceNumber.ToString();
                creditnote.cPDFInvoice = economic_invoice.pdf != null ? economic_invoice.pdf.download : string.Empty;
                creditnote.cCreditNoteNumber = economic_invoice.draftInvoiceNumber.ToString();

            }

            creditnote.isLocked = false;
            creditnote.isNotActual = true;


            StringContent content = new StringContent(JsonSerializer.Serialize(creditnote), Encoding.UTF8, "application/json");
            try
            {

                var response = await acentiocrm_client.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {

                }
                else
                {
                    var error = response.Headers.FirstOrDefault(r => r.Key == "X-Status-Reason").Value.FirstOrDefault();
                    Settings.CreditNotes.Warning.Add(GetLogEntity(creditnote.id, error));
                    economic_invoice_wrapper.Info.Add("Save CRM Draft CreditNote: " + error);

                }
            }
            catch (Exception ex)
            {
                Settings.CreditNotes.Error.Add(GetLogEntity(creditnote.id, ex.Message + " " + ex.StackTrace));
                economic_invoice_wrapper.Info.Add("Save CRM Draft CreditNote: " + ex.Message);
            }
        }

        

        private async Task SaveCRMBookedCreditNote(Economic.Invoice.EconomicInvoiceWrapper economic_invoice_wrapper, Acentio.CreditNoteResponseData creditnote, HttpClient acentiocrm_client)
        {
            var document = "CreditNote";
            var url = "api/v1/" + document + "/" + creditnote.id;
            var economic_invoice = economic_invoice_wrapper.Invoice;

            if (economic_invoice != null)
            {
                creditnote.cERPCreditNoteNo = economic_invoice.bookedInvoiceNumber.ToString();
                creditnote.cPDFInvoice = economic_invoice.pdf != null ? economic_invoice.pdf.download : string.Empty;
                creditnote.status = "Sent";
            }


            StringContent content = new StringContent(JsonSerializer.Serialize(creditnote), Encoding.UTF8, "application/json");
            try
            {

                var response = await acentiocrm_client.PutAsync(url, content);
                if (response.IsSuccessStatusCode)
                {

                }
                else
                {
                    var error = response.Headers.FirstOrDefault(r => r.Key == "X-Status-Reason").Value.FirstOrDefault();
                    Settings.Invoices.Warning.Add(GetLogEntity(creditnote.id, error));
                    economic_invoice_wrapper.Info.Add("Save CRM Booked Credit Note: " + error);
                }
            }
            catch (Exception ex)
            {
                Settings.Invoices.Error.Add(GetLogEntity(creditnote.id, ex.Message + " " + ex.StackTrace));
                economic_invoice_wrapper.Info.Add("Save CRM Booked Credit Note: " + ex.Message);
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

        private Settings.EntityData GetLogEntity(string id, string msg)
        {
            return new Settings.EntityData(id, DateTime.Now.ToShortTimeString() + ": " + msg);
        }
        private string GetLogEntry(string msg)
        {
            return DateTime.Now.ToLongTimeString() + ": " + msg;


        }

        
    }


}
            
