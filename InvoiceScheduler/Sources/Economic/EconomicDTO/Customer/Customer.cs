using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.Customer
{

    public class CustomerResponse : GenericResponse<CustomerResponseData>
    {
    }

    public class CustomerResponseData : GenericDataResponse
    {
        public int customerNumber { get; set; }
        public string currency { get; set; }
        public PaymentTerms paymentTerms { get; set; }
        public CustomerGroup customerGroup { get; set; }
        public string address { get; set; }
        public decimal balance { get; set; }
        public double dueAmount { get; set; }
        public string corporateIdentificationNumber { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public double creditLimit { get; set; }
        public string ean { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string zip { get; set; }
        public string telephoneAndFaxNumber { get; set; }
        public VatZone vatZone { get; set; }
        public SalesPerson salesPerson { get; set; }
        public DateTime lastUpdated { get; set; }
        public string contacts { get; set; }
        public Templates templates { get; set; }
        public Totals totals { get; set; }
        public string deliveryLocations { get; set; }
        public Invoices invoices { get; set; }
        public bool eInvoicingDisabledByDefault { get; set; }
        public MetaData metaData { get; set; }
        public string self { get; set; }
    }

    public class Create
    {
        public string description { get; set; }
        public string href { get; set; }
        public string httpMethod { get; set; }
    }

    public class CustomerGroup
    {
        public int customerNumber { get; set; }
        public string self { get; set; }
    }

    public class CustomerContact
    {
        public int customerContactNumber { get; set; }
        public CustomerGroup customer { get; set; }
        public string self { get; set; }
    }


    public class Invoices
    {
        public string drafts { get; set; }
        public string booked { get; set; }
        public string self { get; set; }
    }

    public class MetaData
    {
        public Create create { get; set; }
    }



    public class PaymentTerms
    {
        public int paymentTermsNumber { get; set; }
        public string self { get; set; }
    }



    public class SalesPerson
    {
        public int employeeNumber { get; set; }
        public string self { get; set; }
    }

    public class Templates
    {
        public string invoice { get; set; }
        public string invoiceLine { get; set; }
        public string self { get; set; }
    }

    public class Totals
    {
        public string drafts { get; set; }
        public string booked { get; set; }
        public string self { get; set; }
    }

    public class VatZone
    {
        public int vatZoneNumber { get; set; }
        public string self { get; set; }
    }

}




