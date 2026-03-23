
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TripleTexDataTransfer_Consumer_TripleTex;
using TripleTexDataTransfer_Consumer_TripleTex.Customer;
using TripleTexDataTransfer_Consumer_TripleTex.Invoice;
using TripleTexDataTransfer_Consumer_TripleTex.Order;
using TripleTexDataTransfer_Consumer_TripleTex.Product;
using TripleTexDataTransfer_Consumer_TripleTex.Session;
using AcentioCRM = InvoiceScheduler_Consumer_AcentioCRM;
namespace TripleTexDataTransfer_Consumer
{
    public class RetrieverAndParser_TripleTex
    {
        internal async Task<CombinedDataSet> GetData(AcentioCRM.CombinedDataSet data_acentio)
        {
            var url = "https://tripletex.no/v2/";
            var success = false;
            var client = new HttpClient();

            client.BaseAddress = new Uri(url);

            CombinedDataSet data = new CombinedDataSet();
            try
            {
                Session session = await GetSession(client);
                if (session.Success)
                {
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"0:{session.value.token}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                    var draftids = data_acentio.Invoices.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && (a.cInvoicedFrom??"").ToLower() == "tripletex (no)")).Select(r => r.cERPDraftInvoiceNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                    draftids.AddRange(data_acentio.CreditNotes.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && (a.cInvoicedFrom ?? "").ToLower() == "tripletex (no)")).Select(r => r.cERPDraftCreditNoteNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList());

                    var draftorders = await GetOrders(client, draftids);
                    if (draftorders.Success)
                    {
                        var customerids = data_acentio.Accounts.list.Where(a => (a.cInvoicedFrom ?? "").ToLower() == "tripletex (no)").Select(r => r.cSyncid).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                        var customers = await GetCustomers(client, customerids);
                        if (customers.Success)
                        {
                            var productids = data_acentio.Products.list.Where(r => (r.cERPSource??"").ToLower() == "tripletex").Select(r => r.cSyncid).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                            var products = await GetProducts(client, productids);
                                                                                 
                            if (products.Success)
                            {
                                data.Orders = draftorders;
                                data.Customers = customers;
                                data.Products = products;
                                success = true;
                            }
                        }


                    }

                }
            }


            catch (Exception ex)
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
        
   
        internal async Task<CombinedDataSet> Get()
        {
            var url = "https://tripletex.no/v2/";
            var success = false;
            var client = new HttpClient();

            client.BaseAddress = new Uri(url);

            CombinedDataSet data = null;
            try
            {
                Session session = await GetSession(client);
                if (session.Success)
                {
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"0:{session.value.token}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                    var products = await GetProducts(client, null);
                    if (products.Success)
                    {
                        var customers = await GetCustomers(client, null);
                        if (customers.Success)
                        {
                            var invoices = await GetInvoices(client, null);
                            if (invoices.Success)
                            {
                                //var orders = await GetOrders(client);
                                //if (orders.Success)
                                //{
                                    data = new CombinedDataSet(products, customers, invoices);
                                    success = true;
                                //}
                            }
                        }
                    }
                }

            }

            catch (Exception ex)
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
        private async Task<ProductResponse> GetProducts(HttpClient client, IList<string> ids)
        {
            if (ids == null || ids.Count == 0) return new ProductResponse() { values = new List<ProductResponseData>() };
            var optionalParam = "&ids=" + string.Join(",", ids); 
            return await GetData<ProductResponse, ProductResponseData>(client, "product", optionalParam);
        }
        private async Task<CustomerResponse> GetCustomers(HttpClient client,  IList<string> ids)
        {
            if (ids == null || ids.Count == 0) return new CustomerResponse() { values = new List<CustomerResponseData>() };
            var optionalParam = ",postalAddress(*)" + "&id=" + string.Join(",", ids);            
            var data=  await GetData<CustomerResponse, CustomerResponseData>(client, "customer", optionalParam);           
            return data;
        }

        private async Task<OrderResponse> GetOrders(HttpClient client, IList<string> ids)
        {
            if (ids == null || ids.Count == 0) return new OrderResponse() { values = new List<OrderResponseData>() };
            var optionalParam = ",orderLines(*),currency(*)&orderDateFrom=2024-01-01&orderDateTo=" + DateTime.Now.AddYears(3).ToString("yyyy-MM-dd") + "&id=" + string.Join(",", ids); //2125656632,2125656239";
            return await GetData<OrderResponse, OrderResponseData>(client, "order", optionalParam);
        }
        
        private async Task<InvoiceResponse> GetInvoices(HttpClient client, IList<string> ids)
        {
            if (ids == null || ids.Count == 0) return new InvoiceResponse() { values = new List<InvoiceResponseData>() };
            var optionalParam = ",orderLines(*),currency(*)&invoiceDateFrom=2024-01-01&invoiceDateTo=" + DateTime.Now.AddYears(3).ToString("yyyy-MM-dd") + "&id=" + string.Join(",", ids); //2125656632,2125656239";
            return await GetData<InvoiceResponse, InvoiceResponseData>(client, "invoice", optionalParam);
        }
   
        private async Task<T> GetData<T, T2>(HttpClient client, string documents, string optionalparam) where T : GenericResponse<T2>, new() where T2 : GenericDataResponse
        {
            T dataResponse = null;
            var blocksize = 1000;
            var i = 0;
            while (true)
            {
                 var url = documents + "?from=" + i*blocksize + "&count=" + blocksize +"&fields=*" + optionalparam;
                // var url = documents + "?fields=*" + optionalparam;
                var response = await client.GetAsync(url);
                var res = await response.Content.ReadAsStringAsync(); 
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = res;
                    var responseIteration = JsonSerializer.Deserialize<T>(jsonResponse);                    
                    if (responseIteration.Success)
                    {
                        if (dataResponse == null)
                        {
                            dataResponse = responseIteration;
                        }
                        else
                        {
                            dataResponse.values.AddRange(responseIteration.values);
                        }
                    }
                    else if (dataResponse == null)
                    {
                        dataResponse = new T() { };
                        break;
                    }
                    else break;
                    if (responseIteration.values.Count < blocksize) break;
                }
                else
                {
                    dataResponse = new T() { };
                    break;
                }
                i++;
            }
           /* if (dataResponse.Success)

            {
                foreach (var s in dataResponse.values)
                {
                    s.Hash = CreateMD5(s.ToString());
                }
            }*/
            return dataResponse;

        }
        public string CreateMD5(string input)
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
