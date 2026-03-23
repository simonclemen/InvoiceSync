using InvoiceScheduler_Consumer_AcentioCRM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Acentio   = InvoiceScheduler_Consumer_AcentioCRM;
using AcentioDB = InvoiceScheduler.Acentio.DB.AcentioDBDTO;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.InvoiceRecord;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.Schedule;

namespace InvoiceScheduler_Consumer
{
    internal class SchedulerPersister
    {
        public Settings Settings { get; }
        
        public SchedulerPersister(Settings settings)
        {
            Settings = settings;
        }


        internal async Task<IList<AcentioDB.InvoiceRecord.InvoiceRecord>> Save(Acentio.CombinedDataSet data_acentio, InvoiceScheduler.Acentio.DB.CombinedDataSet data_acentio_db)
        {
            var url = "https://crm.acentio.com";
            var apikey = "68dcf5cdc467b220a52ecc4c537469c9";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", apikey);
            client.BaseAddress = new Uri(url);

            var result = await SaveInvoices(client, data_acentio, data_acentio_db);
            return result;
        }

        private async Task<IList<InvoiceRecord>> SaveInvoices(HttpClient client, Acentio.CombinedDataSet data_acentio, InvoiceScheduler.Acentio.DB.CombinedDataSet data_acentio_db)
        {
            var document = "Invoice";
            var url = "api/v1/" + document;
            var result = new List<InvoiceRecord>();
            
            foreach (var item in data_acentio_db.Schedules)
            
            {
                var salesorder = data_acentio.SalesOrders.list.FirstOrDefault(r => r.id == item.Sales_Order_Id);
                var salesorderitems = data_acentio.SalesOrderItems.list.Where(r => r.salesOrderId == item.Sales_Order_Id);
                
                try
                {
                   
                    var invoice = GetInvoice(item, salesorder, salesorderitems, data_acentio.Accounts.list);
                    StringContent content = new StringContent(JsonSerializer.Serialize(invoice), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var responsedocument = JsonSerializer.Deserialize<Acentio.InvoiceResponseData>(jsonResponse);
                    var id = (responsedocument == null ? "0" : responsedocument.id);
                    if (response.IsSuccessStatusCode)
                    {
                        Settings.Invoices.NewIds.Add(new Settings.EntityData(id));
                        result.Add(new InvoiceRecord(item.Id, id, item.Sales_Order_Version) );
                        string info = "New invoice based on scheduling information: SalesOrderId: " + item.Sales_Order_Id + ", Version: " + item.Sales_Order_Version;
                        await SaveNote("Invoice", id, info, client, Settings.Invoices.Warning, Settings.Invoices.Error);
                    }
                    else
                    {
                        var error = response.Headers.FirstOrDefault(r => r.Key == "X-Status-Reason").Value.FirstOrDefault();
                        Settings.Invoices.Warning.Add(GetLogEntity(id, error));
                    }
                }
                catch (Exception ex)
                {
                    Settings.Invoices.Error.Add(GetLogEntity(item.Id, ex.Message + " " + ex.StackTrace));
                }                
            }
            return result;
        }

        private Acentio.InvoiceResponseData GetInvoice(Schedule item, SalesOrderResponseData salesorder, IEnumerable<SalesOrderItemResponseData> salesorderitems, IEnumerable<AccountResponseData> accounts)
        {
            var account = accounts.FirstOrDefault(r => r.id == salesorder.accountId);
            var target = new Acentio.InvoiceResponseData();
            target.cERPSource = Settings.SystemId;
            target.cSyncid = item.Id;
            target.name = salesorder.name;
            target.description = salesorder.description;
            target.accountId = salesorder.accountId;
            target.status = string.IsNullOrWhiteSpace(salesorder.cInvoiceCreationState) ? "Draft": salesorder.cInvoiceCreationState;
            target.amountCurrency = salesorder.amountCurrency;
            target.amount = salesorder.amount;
            target.amountDue = target.amount;
            target.billingAddressStreet = salesorder.billingAddressStreet;
            target.billingAddressCity = salesorder.billingAddressCity;
            target.billingAddressPostalCode = salesorder.billingAddressPostalCode;
            target.billingAddressCountry = salesorder.billingAddressCity;
            target.billingAddressCountry = salesorder.billingAddressCountry;
            target.billingContactId = salesorder.billingContactId;
            target.billingContactName = salesorder.billingContactName;
            target.purchaseOrderReference = salesorder.cPOnumberReference;
            target.cAcentioInvoiceReference = salesorder.cAcentioInvoiceReference;
            target.buyerReference = salesorder.billingContactName;
            target.cAccruedStart = item.Periode_Start_Date.ToString("yyyy-MM-dd");
            target.cAccruedEnd = item.Periode_End_Date.ToString("yyyy-MM-dd");
            target.dateInvoiced = item.Invoice_Date.ToString("yyyy-MM-dd");
            target.dateDue = item.Payment_Date.ToString("yyyy-MM-dd");
            target.discountAmount = salesorder.discountAmount;
            target.discountAmountCurrency = salesorder.discountAmountCurrency;
            target.grandTotalAmount = salesorder.grandTotalAmount;
            target.grandTotalAmountCurrency = salesorder.grandTotalAmountCurrency;
            target.isLocked = false;
            target.salesOrderId = salesorder.id;
            target.salesOrderName = salesorder.name;
            target.taxAmountCurrency = salesorder.taxAmountCurrency;
            target.taxRate = salesorder.taxRate;
            target.taxAmount = salesorder.taxAmount;
            target.assignedUserId = salesorder.assignedUserId;
            target.assignedUserName = salesorder.assignedUserName;
            target.cPlatform = salesorder.cPlatform;
            target.itemList = new List<InvoiceItemResponseData>();
            target.cUsageType = salesorder.cUsageType;
            target.cType = salesorder.cType;
            target.cInvoicedFrom = account.cInvoicedFrom;
            foreach (var line in salesorderitems)
            {
                if (line.quantity > 0)
                {
                    var iline = new InvoiceItemResponseData();
                    iline.accountId = line.accountId;
                    iline.accountName = line.accountName;
                    iline.allowFractionalQuantity = line.allowFractionalQuantity; ;
                    iline.amount = line.amount;
                    iline.amountCurrency = line.amountCurrency;
                    iline.amountConverted = line.amountConverted;
                    iline.cERPSource = Settings.SystemId;
                    iline.description = line.description;
                    iline.discount = line.discount;
                    iline.listPrice = line.listPrice;
                    iline.listPriceConverted = line.listPriceConverted;
                    iline.listPriceCurrency = line.listPriceCurrency;
                    iline.name = line.name;
                    iline.order = line.order;
                    iline.productId = line.productId;
                    iline.productName = line.productName;
                    iline.quantity = line.quantity;
                    iline.quantityInt = line.quantityInt;
                    iline.taxRate = line.taxRate;
                    iline.unitPrice = line.unitPrice;
                    iline.unitPriceConverted = line.unitPriceConverted;
                    iline.unitPriceCurrency = line.unitPriceCurrency;
                    iline.unitWeight = line.unitWeight;
                    iline.weight = line.weight;
                    target.itemList.Add(iline);
                }
            }
            if (target.itemList.Count == 0) target.status = "Cancelled";
            return target;
        }
       
        private Settings.EntityData GetLogEntity(string id, string msg)
        {
            return new Settings.EntityData(id, DateTime.Now.ToShortTimeString() + ": " + msg);
        }
        private string GetLogEntry(string msg)
        {
            return DateTime.Now.ToLongTimeString() + ": " + msg;


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

    }


}
            
