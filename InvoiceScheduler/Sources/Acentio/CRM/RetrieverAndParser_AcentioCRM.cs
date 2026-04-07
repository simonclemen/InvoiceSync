using InvoiceScheduler.Acentio.DB.AcentioDBDTO.Schedule;
using InvoiceScheduler_Consumer_AcentioCRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using crm = InvoiceScheduler_Consumer_AcentioCRM;
namespace InvoiceScheduler_Consumer
{
    public class RetrieverAndParser_AcentioCRM
    {
        public string LastrunDT { get; }
        public Settings Settings { get; private set; }

        public RetrieverAndParser_AcentioCRM(string lastrunDT, Settings settings)
        {
            LastrunDT = lastrunDT;
            Settings = settings;
        }

        internal async Task<CombinedDataSet> GetMasterdata()
        {
            var client = GetClient();
          
            var filter = string.IsNullOrWhiteSpace(LastrunDT)? "": "where[0][type]=after&where[0][attribute]=modifiedAt&where[0][value]=" + LastrunDT + "&where[1][type]=notEquals&where[1][attribute]=modifiedById&where[1][value]=693c611227539034c";
            var products = await GetData<ProductResponse, ProductResponseData>(client, "Product?" + filter);
            var contacts = await GetData<ContactResponse, ContactResponseData>(client, "Contact?" + filter);
            filter = string.IsNullOrWhiteSpace(LastrunDT) ? "" : GetFilter("id", contacts.list.Select(r => r.accountId).ToList());
            var accounts = await GetData<AccountResponse, AccountResponseData>(client, "Account?" + filter);
            
            var data = new CombinedDataSet();
            data.Products = products; 
            data.Contacts = contacts; 
            data.Accounts = accounts;

            return data;
        }

        internal async Task<CombinedDataSet> GetSalesOrders(InvoiceScheduler.Acentio.DB.CombinedDataSet data)
        {
            var client = GetClient();
            var success = false;
            CombinedDataSet response = null;
                        
            try
            {
                var filter = GetFilter("cSyncid", data.Schedules.Select(r => r.Id).ToList());
                var invoices = await GetData<InvoiceResponse, InvoiceResponseData>(client, "Invoice? " + filter);
                if (invoices.list.Count > 0) Settings.Invoices.Warning.Add(new Settings.EntityData("", "Schedules and actual not aligned" ));
                data.Schedules = data.Schedules.Where(r => !invoices.list.Any(i => i.cSyncid == r.Id)).ToList();
             
                filter = GetFilter("id", data.Schedules.Select(r => r.Sales_Order_Id).ToList());
                var salesOrders = await GetData<SalesOrderResponse, SalesOrderResponseData>(client, "SalesOrder? " + filter);
                
                filter = GetFilter("salesOrderId", data.Schedules.Select(r => r.Sales_Order_Id).ToList());
                var salesOrderItems = await GetData<SalesOrderItemResponse, SalesOrderItemResponseData>(client, "SalesOrderItem? " + filter);

                filter = GetFilter("id", salesOrders.list.Select(r => r.accountId).ToList());
                var accounts = await GetData<AccountResponse, AccountResponseData>(client, "Account? " + filter);

                response = new CombinedDataSet(salesOrders, salesOrderItems, accounts);
                success = true;
             
            }
            catch (Exception)
            {
                throw;
            }
            if (!success)
            {
                /*Do something....*/
                throw new ArgumentException("Failed");
            }

            return response;
        }
        internal async Task<CombinedDataSet> GetData()
        {
            var client = GetClient();
            var success = false;
            CombinedDataSet data = new CombinedDataSet();
            try
            {
                success = await GetInvoicesMatchingState(client, data, "Verified", LastrunDT);
                CombinedDataSet data2 = new CombinedDataSet();
                success &= await GetInvoicesMatchingState(client, data2, "Ready For Transfer", null);
                data.Combine(data2);
                data2 = new CombinedDataSet();
                success &= await GetCreditNoteMatchingState(client, data2, "Verified", LastrunDT);
                data.Combine(data2);
                data2 = new CombinedDataSet();
                success &= await GetCreditNoteMatchingState(client, data2, "Ready For Transfer", null);
                data.Combine(data2);

                data2 = new CombinedDataSet();
                var ids = data.CreditNotes.list.Select(r => r.invoiceId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
                success &= await GetInvoicesMatchingIds(client, data2,ids);
                data.Combine(data2);

                data.SalesOrders = new SalesOrderResponse() { list = new List<SalesOrderResponseData>() };
                if (success)
                {
                    var listids = new List<string>();
                    if (data.Invoices.total > 0) listids.AddRange(data.Invoices.list.Select(inv => inv.salesOrderId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());                    
                    if (listids.Count > 0)
                    {
                        string filter = GetFilter("id", listids.Distinct().ToList());
                        data.SalesOrders = await GetData<SalesOrderResponse, SalesOrderResponseData>(client, "SalesOrder? " + filter);
                    }
                }

                data.Products = new ProductResponse() { list = new List<ProductResponseData>() };
                if (success)
                {
                    var listids = new List<string>();
                    if (data.Invoices.total > 0) listids.AddRange(data.Invoices.list.SelectMany(inv => inv.itemList).Select(item => item.productId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    if (data.CreditNotes.total > 0) listids.AddRange(data.CreditNotes.list.SelectMany(inv => inv.itemList).Select(item => item.productId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    if (listids.Count > 0)
                    {
                        string filter = GetFilter("id", listids.Distinct().ToList());
                        data.Products = await GetData<ProductResponse, ProductResponseData>(client, "Product? " + filter);
                    }
                }

                data.Contacts = new ContactResponse() { list = new List<ContactResponseData>() };
                if (success)
                {
                    var listids = new List<string>();
                    if (data.Invoices.total > 0) listids.AddRange(data.Invoices.list.Select(inv => inv.billingContactId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    if (data.CreditNotes.total > 0) listids.AddRange(data.CreditNotes.list.Select(inv => inv.billingContactId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    if (listids.Count > 0)
                    {
                        string filter = GetFilter("id", listids.Distinct().ToList());
                        data.Contacts = await GetData<ContactResponse, ContactResponseData>(client, "Contact? " + filter);
                    }
                }
            }

            catch (Exception)
            {
                throw;
            }

            if (!success)
            {
                /*Do something....*/
                throw new ArgumentException("Failed");
            }

            return data;
        }

        private async Task<bool> GetInvoicesMatchingState(HttpClient client, CombinedDataSet data, string state, string lastrun)
         {
            
            var invoices = await GetData<InvoiceResponse, InvoiceResponseData>(client, "Invoice?where[0][type]=equals&where[0][attribute]=status&where[0][value]=" + state + (string.IsNullOrEmpty(lastrun) ? "":"&where[1][type]=after&where[1][attribute]=modifiedAt&where[1][value]="+ lastrun + "&where[2][type]=notEquals&where[2][attribute]=modifiedById&where[2][value]=693c611227539034c"));
            
            if (invoices.Success)
            {
                if (data.Invoices == null) data.Invoices = invoices;
                else
                {
                    data.Invoices.total += invoices.total;
                    data.Invoices.list.AddRange(invoices.list);
                }
                if (data.Invoices.list.Count > 0)
                {
                    string filter = GetFilter("id", data.Invoices.list.Select(r => r.accountId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.Accounts = await GetData<AccountResponse, AccountResponseData>(client, "Account?" + filter);
                    filter = GetFilter("invoiceId", data.Invoices.list.Select(r => r.id).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.InvoiceItems = await GetData<InvoiceItemResponse, InvoiceItemResponseData>(client, "InvoiceItem?" + filter);
                    filter = GetFilter("id", data.Invoices.list.Select(r => r.billingContactId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.Contacts = await GetData<ContactResponse, ContactResponseData>(client, "Contact?" + filter);


                    foreach (var invoice in invoices.list)
                    {
                        invoice.itemList = data.InvoiceItems.list.Where(r => r.invoiceId == invoice.id).ToList();

                    }
                }
                else
                {
                    data.Accounts = new AccountResponse() { list = new List<AccountResponseData>() };
                    data.InvoiceItems = new InvoiceItemResponse() { list = new List<InvoiceItemResponseData>() };
                    data.Contacts = new ContactResponse() { list = new List<ContactResponseData>() };

                }
                    return true;
            }
            return false;
        }
        private async Task<bool> GetInvoicesMatchingIds(HttpClient client, CombinedDataSet data, IList<string> ids)
        {
            if (ids.Count == 0) return true;

            string invoicefilter = GetFilter("id", ids);
            var invoices = await GetData<InvoiceResponse, InvoiceResponseData>(client, "Invoice?" + invoicefilter);            

            if (invoices.Success)
            {
                if (data.Invoices == null) data.Invoices = invoices;
                else
                {
                    data.Invoices.total += invoices.total;
                    data.Invoices.list.AddRange(invoices.list);
                }
                if (data.Invoices.list.Count > 0)
                {
                    string filter = GetFilter("id", data.Invoices.list.Select(r => r.accountId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.Accounts = await GetData<AccountResponse, AccountResponseData>(client, "Account?" + filter);
                    filter = GetFilter("invoiceId", data.Invoices.list.Select(r => r.id).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.InvoiceItems = await GetData<InvoiceItemResponse, InvoiceItemResponseData>(client, "InvoiceItem?" + filter);
                    filter = GetFilter("id", data.Invoices.list.Select(r => r.billingContactId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.Contacts = await GetData<ContactResponse, ContactResponseData>(client, "Contact?" + filter);


                    foreach (var invoice in invoices.list)
                    {
                        invoice.itemList = data.InvoiceItems.list.Where(r => r.invoiceId == invoice.id).ToList();

                    }
                }
                else
                {
                    data.Accounts = new AccountResponse() { list = new List<AccountResponseData>() };
                    data.InvoiceItems = new InvoiceItemResponse() { list = new List<InvoiceItemResponseData>() };
                    data.Contacts = new ContactResponse() { list = new List<ContactResponseData>() };

                }
                return true;
            }
            return false;
        }
        private async Task<bool> GetCreditNoteMatchingState(HttpClient client, CombinedDataSet data, string state, string lastrun)
        {
            var creditnotes = await GetData<CreditNoteResponse, CreditNoteResponseData>(client, "CreditNote?where[0][type]=equals&where[0][attribute]=status&where[0][value]=" + state+ (string.IsNullOrWhiteSpace(lastrun) ? "" : "&where[1][type]=after&where[1][attribute]=modifiedAt&where[1][value]=" + lastrun + "&where[2][type]=notEquals&where[2][attribute]=modifiedById&where[2][value]=693c611227539034c"));

            if (creditnotes.Success)
            {
                if (data.CreditNotes == null) data.CreditNotes = creditnotes;
                else
                {
                    data.CreditNotes.total += creditnotes.total;
                    data.CreditNotes.list.AddRange(creditnotes.list);
                }
                if (data.CreditNotes.list.Count > 0)
                {
                    string filter = GetFilter("id", data.CreditNotes.list.Select(r => r.accountId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.Accounts = await GetData<AccountResponse, AccountResponseData>(client, "Account?" + filter);
                    filter = GetFilter("creditNoteId", data.CreditNotes.list.Select(r => r.id).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.CreditNoteItems = await GetData<CreditNoteItemResponse, CreditNoteItemResponseData>(client, "CreditNoteItem?" + filter);
                    filter = GetFilter("id", data.CreditNotes.list.Select(r => r.billingContactId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    data.Contacts = await GetData<ContactResponse, ContactResponseData>(client, "Contact?" + filter);


                    foreach (var creditnote in creditnotes.list)
                    {
                       creditnote.itemList = data.CreditNoteItems.list.Where(r => r.creditNoteId == creditnote.id).ToList();

                    }
                }
                else
                {
                    data.Accounts = new AccountResponse() { list = new List<AccountResponseData>() };
                    data.InvoiceItems = new InvoiceItemResponse() { list = new List<InvoiceItemResponseData>() };
                    data.Contacts = new ContactResponse() { list = new List<ContactResponseData>() };

                }
                return true;
            }
            return false;
        }

        private string GetFilter(string field, IList<string> list)
        {
            var sb = new StringBuilder();

            // Required filters for Espo IN query
            sb.Append("where[0][type]=in");
            sb.Append("&where[0][attribute]=" + field);

            foreach (var id in list)
            {
                sb.Append("&where[0][value][]=");
                sb.Append(Uri.EscapeDataString(id));
            }

            string queryString = sb.ToString();
            return queryString;
        }

        private HttpClient GetClient()
        {
            var url = "https://crm.acentio.com";
            var apikey = "68dcf5cdc467b220a52ecc4c537469c9";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", apikey);
            client.BaseAddress = new Uri(url);
            return client;
        }

        private async Task<T> GetDocumentData<T>(HttpClient client, string document) where T : new()
        {
            var url = "api/v1/" + document;
            var response = await client.GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<T>(jsonResponse);
                return responseData;
            }
            else return default(T);
        }

        private async Task<T> GetData<T, T2>(HttpClient client, string documents) where T : GenericResponse<T2>, new() where T2 : GenericDataResponse
        {
            T dataResponse = null;
            var blocksize = 200;
            var i = 0;

            while (true)
            {
                var url = "api/v1/" + documents + (documents.Contains("?") ? "&" : "?") + "offset=" + Math.Max(i * blocksize - 1, 0) + "&maxSize=" + blocksize;
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var responseIteration = JsonSerializer.Deserialize<T>(jsonResponse);
                    if (responseIteration.Success)
                    {
                        if (dataResponse == null) dataResponse = responseIteration;
                        else
                        {
                            dataResponse.list.AddRange(responseIteration.list);
                            if (responseIteration.list.Count < blocksize) break;
                        }
                    }
                    else if (dataResponse == null)
                    {
                        dataResponse = new T() { list = new List<T2>() };
                        break;
                    }
                    else break;
                }
                else
                {
                    dataResponse = new T() { list = new List<T2>() };
                    break;
                }
                i++;
            }
            if (dataResponse.Success)
            {
                if (dataResponse.list != null)
                {
                    foreach (var s in dataResponse.list)
                    {
                        s.Hash = CreateMD5(s.ToString());

                        int line_id = 0;
                        var t = dataResponse.list.Where(r => r.id == s.id && string.IsNullOrEmpty(r.line_id)).ToList();
                        foreach (var elm in t)
                        {
                            elm.line_id = line_id.ToString();
                            line_id++;
                        }

                    }
                }
            }
            return dataResponse;

        }


        public static string CreateMD5(string input)
        {
            // byte array representation of that string
            byte[] encodedinput = new UTF8Encoding().GetBytes(input);

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedinput);

            // string representation (similar to UNIX format)
            string encoded = BitConverter.ToString(hash)
               // without dashes
               .Replace("-", string.Empty)
               // make lowercase
               .ToLower();
            return encoded;
        }

       
    }
}
