
using EconomicDataTransfer_Consumer_Economic;
using EconomicDataTransfer_Consumer_Economic.Contact;
using EconomicDataTransfer_Consumer_Economic.Customer;
using EconomicDataTransfer_Consumer_Economic.CustomerGroup;
using EconomicDataTransfer_Consumer_Economic.Department;
using EconomicDataTransfer_Consumer_Economic.DraftInvoice;
using EconomicDataTransfer_Consumer_Economic.Employee;
using EconomicDataTransfer_Consumer_Economic.Invoice;
using EconomicDataTransfer_Consumer_Economic.Layout;
using EconomicDataTransfer_Consumer_Economic.Layout;
using EconomicDataTransfer_Consumer_Economic.PaymentTerm;
using EconomicDataTransfer_Consumer_Economic.Product;
using EconomicDataTransfer_Consumer_Economic.ProductGroup;
using EconomicDataTransfer_Consumer_Economic.VatZone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AcentioCRM = InvoiceScheduler_Consumer_AcentioCRM;
namespace EconomicDataTransfer_Consumer
{
    public class RetrieverAndParser_Economic
    {
        internal async Task<CombinedDataSet> Get()
        {
            var url = "https://restapi.e-conomic.com";

            var XAppSecretToken = "Qr7MfqkrExs8dqsEHUkcxECR0TqFgfvBEYzm6fVZNAI";
            var XAgreementGrantToken = "2v9MJy8N9Tz9Z3lzpqKTbzM5qcIJt3zZDOT3dYP3JP8";
            var success = false;
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("X-AppSecretToken", XAppSecretToken);
            client.DefaultRequestHeaders.Add("X-AgreementGrantToken", XAgreementGrantToken);

            client.BaseAddress = new Uri(url);

            CombinedDataSet data = null;
            try
            {
                var productgroups = await GetProductGroups(client);
                if (productgroups.Success)
                {
                    var products = await GetProducts(client);
                    if (products.Success)
                    {
                        var vatzones = await GetVatZones(client);
                        if (vatzones.Success)
                        {
                            var customergroups = await GetCustomerGroups(client);
                            if (customergroups.Success)
                            {
                                var customers = await GetCustomers(client);
                                if (customers.Success)
                                {
                                    var departments = await GetDepartments(client);
                                    if (departments.Success)
                                    {
                                        var contacts = await GetContacts(client, customers);
                                        if (contacts.Success)
                                        {
                                            var paymentterms = await GetPaymentTerms(client);
                                            if (paymentterms.Success)
                                            {
                                                var draftinvoices = await GetDraftInvoices(client);
                                                if (draftinvoices.Success)
                                                {
                                                    var boookedinvoices = await GetBookedInvoices(client);
                                                    if (boookedinvoices.Success)
                                                    {
                                                        data = new CombinedDataSet(productgroups, products, vatzones, customergroups, customers, contacts, paymentterms, draftinvoices, boookedinvoices, departments);
                                                        success = true;
                                                    }
                                                }

                                                //data = new CombinedDataSet(productgroups, products, vatzones, customergroups, customers, contacts, paymentterms, new InvoiceResponse(), new InvoiceResponse());
                                                //success = true;
                                            }
                                        }
                                    }
                                }
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
        internal HttpClient GetClient()
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

        internal async Task<CombinedDataSet> GetMasterdata(AcentioCRM.CombinedDataSet data_acentio)
        {
            var client = GetClient();
            var success = false;
            CombinedDataSet data = null;
            try
            {
              //  var accountIds = data_acentio.Accounts.list.Where(a=> a.cInvoicedFrom == "E-conomic (DK)").Select(r => r.cSyncid).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();

                var customerids = data_acentio.Accounts.list.Where(a => (a.cInvoicedFrom ?? "").ToLower() == "e-conomic (dk)").Select(r => r.cERPCustomerNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                var customers = await GetCustomers(client, customerids);// client,draftinvoices.collection.Select(r => r.customer.customerNumber.ToString()).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());


                var contactids = data_acentio.Contacts.list.Where(c => data_acentio.Accounts.list.Any(a=>a.id == c.accountId && (a.cInvoicedFrom ?? "").ToLower() == "e-conomic (dk)")).Select(r =>r.emailAddress).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();

                //var contacts = await GetContacts(client, customers);

                /*

                var draftids = data_acentio.Invoices.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && a.cInvoicedFrom == "E-conomic (DK)")).Select(r => r.cERPDraftInvoiceNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                draftids.AddRange(data_acentio.CreditNotes.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && a.cInvoicedFrom == "E-conomic (DK)")).Select(r => r.cERPDraftCreditNoteNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList());
                var draftinvoices = await GetDraftInvoices(client, draftids);
                if (draftinvoices.Success)
                {

                    var customerids = data_acentio.Accounts.list.Where(a => (a.cInvoicedFrom ?? "").ToLower() == "e-conomic (dk)").Select(r => r.cERPCustomerNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                    var customers = await GetCustomers(client, customerids);// client,draftinvoices.collection.Select(r => r.customer.customerNumber.ToString()).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    if (customers.Success)
                    {
                        var productids = data_acentio.Products.list.Where(r => r.cERPSource == "economic").Select(r => r.cSyncid).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                        var products = await GetProducts(client, productids);// client, draftinvoices.collection.Where(r=>r.lines!=null).SelectMany(r => r.lines).Where(r=>r.product!=null).Select(r=>r.product.productNumber.ToString()).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                        //var products = await GetProducts(client);
                        if (products.Success)
                        {
                            var departments = await GetDepartments(client);
                            if (departments.Success)
                            {

                                var paymentterms = await GetPaymentTerms(client);
                                if (paymentterms.Success)
                                {
                                    var contacts = await GetContacts(client, customers);
                                    if (contacts.Success)
                                    {

                                        var employees = await GetEmployees(client);
                                        if (contacts.Success)
                                        {
                                            var layouts = await GetLayouts(client);
                                            if (layouts.Success)
                                            {
                                                data = new CombinedDataSet();
                                                data.Draftinvoices = draftinvoices;
                                                data.Customers = customers;
                                                data.Products = products;
                                                data.PaymentTerms = paymentterms;
                                                data.Contacts = contacts;
                                                data.Employees = employees;
                                                data.Departments = departments;
                                                success = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }*/
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
        internal async Task<CombinedDataSet> GetData(AcentioCRM.CombinedDataSet data_acentio)
        {
            var client = GetClient();
            var success = false;
            CombinedDataSet data = null;
            try
            {
                var draftids = data_acentio.Invoices.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && a.cInvoicedFrom == "E-conomic (DK)")).Select(r => r.cERPDraftInvoiceNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                draftids.AddRange(data_acentio.CreditNotes.list.Where(r => data_acentio.Accounts.list.Any(a => a.id == r.accountId && a.cInvoicedFrom == "E-conomic (DK)")).Select(r => r.cERPDraftCreditNoteNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList());
                var draftinvoices = await GetDraftInvoices(client, draftids);
                if (draftinvoices.Success)
                {

                    var customerids = data_acentio.Accounts.list.Where(a => (a.cInvoicedFrom ?? "").ToLower() == "e-conomic (dk)").Select(r => r.cERPCustomerNo).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                    var customers = await GetCustomers(client, customerids);// client,draftinvoices.collection.Select(r => r.customer.customerNumber.ToString()).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                    if (customers.Success)
                    {
                        var productids = data_acentio.Products.list.Where(r => r.cERPSource == "economic").Select(r => r.cSyncid).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
                        var products = await GetProducts(client, productids);// client, draftinvoices.collection.Where(r=>r.lines!=null).SelectMany(r => r.lines).Where(r=>r.product!=null).Select(r=>r.product.productNumber.ToString()).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList());
                        //var products = await GetProducts(client);
                        if (products.Success)
                        {
                            var departments = await GetDepartments(client);
                            if (departments.Success)
                            {

                                var paymentterms = await GetPaymentTerms(client);
                                if (paymentterms.Success)
                                {
                                    var contacts = await GetContacts(client, customers);
                                    if (contacts.Success)
                                    {

                                        var employees = await GetEmployees(client);
                                        if (contacts.Success)
                                        {
                                            var layouts = await GetLayouts(client);
                                            if (layouts.Success)
                                            {
                                                data = new CombinedDataSet();
                                                data.Draftinvoices = draftinvoices;
                                                data.Customers = customers;
                                                data.Products = products;
                                                data.PaymentTerms = paymentterms;
                                                data.Contacts = contacts;
                                                data.Employees = employees;
                                                data.Departments = departments;
                                                success = true;
                                            }
                                        }
                                    }
                                }
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

        private async Task<CustomerResponse> GetCustomers(HttpClient client, List<string> list)
        {
            var semaphore = new SemaphoreSlim(5);
            var tasks = list.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await GetData<CustomerResponseData>(
                        client,
                        "customers/" + item);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            var enriched = await Task.WhenAll(tasks);

            var data = enriched
                .Where(r => r != null)
                .ToList();

            return new CustomerResponse() { collection = data };
        }
        private async Task<DraftInvoiceResponse> GetDraftInvoices(HttpClient client)
        {
            var data = await GetData<DraftInvoiceResponse, DraftInvoiceResponseData>(client, "invoices/drafts");
            return data;
        }


        private async Task<DraftInvoiceResponse> GetDraftInvoices(HttpClient client, List<string> list)
        {
            var semaphore = new SemaphoreSlim(5);

            var tasks = list.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await GetData<DraftInvoiceResponseData>(
                        client,
                        "invoices/drafts/" + item);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            var enriched = await Task.WhenAll(tasks);

            var data = enriched.Where(r => r != null).ToList();

            return new DraftInvoiceResponse() { collection = data };

        }
        private async Task<InvoiceResponse> GetBookedInvoices(HttpClient client)
        {
            var data = await GetData<InvoiceResponse, InvoiceResponseData>(client, "invoices/booked");

            var semaphore = new SemaphoreSlim(5);

            var tasks = data.collection.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await GetData<InvoiceResponseData>(
                        client,
                        "invoices/booked/" + item.bookedInvoiceNumber);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            var enriched = await Task.WhenAll(tasks);

            data.collection = enriched
                .Where(r => r != null)
                .ToList();

            //var json ="";
            //foreach (var item in data.collection) {  json = json +","+ item.Json;  }
            //System.IO.File.WriteAllText("C:\\temp\\data.json", json);
            return data;
        }

        private async Task<PaymentTermResponse> GetPaymentTerms(HttpClient client)
        {
            return await GetData<PaymentTermResponse, PaymentTermResponseData>(client, "payment-terms");
        }
        private async Task<LayoutResponse> GetLayouts(HttpClient client)
        {
            return await GetData<LayoutResponse, LayoutResponseData>(client, "layouts");
        }

        private async Task<VatZoneResponse> GetVatZones(HttpClient client)
        {
            return await GetData<VatZoneResponse, VatZoneResponseData>(client, "vat-zones");
        }
        private async Task<CustomerResponse> GetCustomers(HttpClient client)
        {

            return await GetData<CustomerResponse, CustomerResponseData>(client, "customers");
        }
        private async Task<EmployeeResponse> GetEmployees(HttpClient client)
        {

            return await GetData<EmployeeResponse, EmployeeResponseData>(client, "employees");
        }


        private async Task<ContactResponse> GetContacts(HttpClient client, CustomerResponse customers)
        {
            ContactResponse contacts = null;
            var semaphore = new SemaphoreSlim(5);

            var tasks = customers.collection.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await GetData<ContactResponse, ContactResponseData>(
                        client,
                        "customers/" + item.customerNumber + "/contacts");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);

            contacts = new ContactResponse
            {
                collection = results
                    .Where(r => r?.collection != null)
                    .SelectMany(r => r.collection)
                    .ToList()
            };
            return contacts;

        }
        private async Task<CustomerGroupResponse> GetCustomerGroups(HttpClient client)
        {
            return await GetData<CustomerGroupResponse, CustomerGroupResponseData>(client, "customer-groups");
        }


        private async Task<DepartmentResponse> GetDepartments(HttpClient client)
        {
            return await GetData<DepartmentResponse, DepartmentResponseData>(client, "departments");
        }
        private async Task<ProductResponse> GetProducts(HttpClient client, List<string> list)
        {
            var semaphore = new SemaphoreSlim(5);
            var tasks = list.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await GetData<ProductResponseData>(
                        client,
                        "products/" + item);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            var enriched = await Task.WhenAll(tasks);

            var data = enriched
                .Where(r => r != null)
                .ToList();

            return new ProductResponse() { collection = data };
        }
        private async Task<ProductResponse> GetProducts(HttpClient client)
        {
            return await GetData<ProductResponse, ProductResponseData>(client, "products");
        }
        private async Task<ProductGroupResponse> GetProductGroups(HttpClient client)
        {
            return await GetData<ProductGroupResponse, ProductGroupResponseData>(client, "product-groups");
        }

        private async Task<T> GetData<T, T2>(HttpClient client, string documents) where T : GenericResponse<T2>, new() where T2 : GenericDataResponse
        {
            T dataResponse = null;
            var blocksize = 1000;
            var i = 0;
            // var combinedjson = "";
            while (true)
            {
                var url = "/" + documents + "?skippages=" + i + "&pagesize=" + blocksize;
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    //       combinedjson = combinedjson + "|||||" +jsonResponse;
                    var responseIteration = JsonSerializer.Deserialize<T>(jsonResponse);
                    if (responseIteration.Success)
                    {
                        if (dataResponse == null)
                        {
                            dataResponse = responseIteration;

                        }
                        else
                        {
                            dataResponse.collection.AddRange(responseIteration.collection);

                        }
                    }
                    else if (dataResponse == null)
                    {
                        dataResponse = new T() { };
                        break;
                    }
                    else break;
                    if (responseIteration.collection.Count < blocksize) break;
                }
                else
                {
                    dataResponse = new T() { };
                    break;
                }
                i++;
            }
            if (dataResponse.Success)

            {
                foreach (var s in dataResponse.collection)
                {
                    s.Hash = CreateMD5(s.ToString());
                }
            }
            return dataResponse;

        }
        private async Task<T> GetData<T>(HttpClient client, string documents) where T : GenericDataResponse, new()
        {
            T dataResponse = null;

            var url = "/" + documents;
            var jsonResponse = "";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                jsonResponse = await response.Content.ReadAsStringAsync();
                dataResponse = JsonSerializer.Deserialize<T>(jsonResponse);

                if (dataResponse == null)
                {
                    dataResponse = new T() { };
                }
            }
            else
            {
                dataResponse = new T() { };
            }

            dataResponse.Hash = CreateMD5(dataResponse.ToString());
            dataResponse.Json = jsonResponse;
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

        internal async Task DeleteDrafts(InvoiceScheduler.Acentio.DB.CombinedDataSet data_acentio_db)
        {
            var client = GetClient();
            foreach (var draftentry in data_acentio_db.DraftNumbers)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(draftentry.ErpDraftNumber)) continue;
                    var url = "/invoices/drafts/" + draftentry.ErpDraftNumber; ;
                    var response = await client.DeleteAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                    }
                    else
                    { 
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

     
    }
}
