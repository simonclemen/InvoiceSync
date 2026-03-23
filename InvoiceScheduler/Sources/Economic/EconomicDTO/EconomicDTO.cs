using EconomicDataTransfer_Consumer_Economic.Contact;
using EconomicDataTransfer_Consumer_Economic.Customer;
using EconomicDataTransfer_Consumer_Economic.CustomerGroup;
using EconomicDataTransfer_Consumer_Economic.DraftInvoice;
using EconomicDataTransfer_Consumer_Economic.Employee;
using EconomicDataTransfer_Consumer_Economic.Invoice;
using EconomicDataTransfer_Consumer_Economic.PaymentTerm;
using EconomicDataTransfer_Consumer_Economic.Product;
using EconomicDataTransfer_Consumer_Economic.ProductGroup;
using EconomicDataTransfer_Consumer_Economic.VatZone;
using EconomicDataTransfer_Consumer_Economic.Department;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace EconomicDataTransfer_Consumer_Economic
{
    public class CombinedDataSet
    {
        public CombinedDataSet()
        {
        }

        public CombinedDataSet(ProductGroupResponse productgroups, ProductResponse products, VatZoneResponse vatzones, CustomerGroupResponse customergroups, CustomerResponse customers, Contact.ContactResponse contacts, PaymentTermResponse paymentterms, DraftInvoiceResponse draftinvoices, InvoiceResponse boookedinvoices, DepartmentResponse departments)
        {
            Productgroups = productgroups;
            Products = products;
            Customers = customers;
            Draftinvoices = draftinvoices;
            Boookedinvoices = boookedinvoices;
            VatZones = vatzones;
            CustomerGroups = customergroups;
            PaymentTerms = paymentterms;
            Contacts  = contacts;
            Departments = departments;
        }

        public VatZoneResponse VatZones { get; }
        public CustomerGroupResponse CustomerGroups { get; }
        public PaymentTermResponse PaymentTerms { get; set; }
        public ProductGroupResponse Productgroups { get; }
        public ProductResponse Products { get; set; }
        public CustomerResponse Customers { get; set; }
        public ContactResponse Contacts { get; set; }
        public DraftInvoiceResponse Draftinvoices { get; set; }
        public InvoiceResponse Boookedinvoices { get; }
        public EmployeeResponse Employees { get; internal set; }

        public  DepartmentResponse Departments { get; internal set; }
    }


    public class Pagination
    {
        public int skipPages { get; set; }
        public int pageSize { get; set; }
        public int maxPageSizeAllowed { get; set; }
        public int results { get; set; }
        public int resultsWithoutFilter { get; set; }
        public string firstPage { get; set; }
        public string nextPage { get; set; }
        public string lastPage { get; set; }
    }

    public abstract class GenericResponse<T> where T : GenericDataResponse
    {
        public bool Success { get { return collection != null; } }

        public List<T> collection { get; set; }
        public Pagination pagination { get; set; }
        public string self { get; set; }
    }
    public abstract class GenericDataResponse
    {
        public string Hash { get; set; }
        public string Json { get; internal set; }

        public override string ToString()
        {
            var s = "";
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(this);
                s = s + (property.Name ?? string.Empty) + ":" + (propertyValue ?? string.Empty) + "_";
            }

            return s;
        }
        public string C_SQL()
        {
            var s = "";
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(this);
                s = s + "parms.Add(name: \"@" + property.Name + "\", value: data." + property.Name + ", direction: System.Data.ParameterDirection.Input);" + System.Environment.NewLine;//(property.Name ?? string.Empty) + ":" + (propertyValue ?? string.Empty) + "_";
            }

            return s;
        }
        public string ToAddColumns()
        {
            var s = "";
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                s = s + "ALTER TABLE dbo.[" + type.ToString() + "] ADD [" + property.Name + "] nvarchar(max);";
                //object propertyValue = property.GetValue(this);
                //s = s + (property.Name ?? string.Empty) + ":" + (propertyValue ?? string.Empty) + "_";
            }

            return s;
        }
    }


}
