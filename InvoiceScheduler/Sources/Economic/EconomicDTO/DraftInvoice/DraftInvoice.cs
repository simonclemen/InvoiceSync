using EconomicDataTransfer_Consumer_Economic.Customer;
using EconomicDataTransfer_Consumer_Economic.CustomerGroup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.DraftInvoice
{
    public class DraftInvoiceResponse : GenericResponse<DraftInvoiceResponseData>
    {
    }


    public class DraftInvoiceResponseData : GenericDataResponse
    {
        public int draftInvoiceNumber { get; set; }
        public Soap soap { get; set; }
        public Templates templates { get; set; }
        public int orderNumberDb { get; set; }
        public string attachment { get; set; }
        public string date { get; set; }
        public string currency { get; set; }
        public decimal? exchangeRate { get; set; }
        public decimal netAmount { get; set; }
        public decimal netAmountInBaseCurrency { get; set; }
        public decimal grossAmount { get; set; }
        public decimal grossAmountInBaseCurrency { get; set; }
        public decimal marginInBaseCurrency { get; set; }
        public decimal marginPercentage { get; set; }
        public decimal vatAmount { get; set; }
        public decimal roundingAmount { get; set; }
        public decimal costPriceInBaseCurrency { get; set; }
        public string dueDate { get; set; }
        public PaymentTerms paymentTerms { get; set; }
        public Customer customer { get; set; }
        public Recipient recipient { get; set; }
        public Notes notes { get; set; }
        public References references { get; set; }
        public Layout layout { get; set; }
        public Pdf pdf { get; set; }
        public DateTime lastUpdated { get; set; }
        public string self { get; set; }

        public List<Line> lines { get; set; }
        public string externalId { get; internal set; }
        public string ean { get; internal set; }
    }


    public class CurrentInvoiceHandle
    {
        public int id { get; set; }
    }

    public class Customer
    {
        public int customerNumber { get; set; }
        public string self { get; set; }
    }

    public class CustomerContact
    {
        public int customerContactNumber { get; set; }
        public Customer customer { get; set; }
        public string self { get; set; }
    }

    public class Layout
    {
        public int layoutNumber { get; set; }
        public string self { get; set; }
    }

    public class Notes
    {
        public string heading { get; set; }
        public string textLine1 { get; set; }
        public string textLine2 { get; set; }
    }

    public class Pagination
    {
        public int skipPages { get; set; }
        public int pageSize { get; set; }
        public int maxPageSizeAllowed { get; set; }
        public int results { get; set; }
        public int resultsWithoutFilter { get; set; }
        public string firstPage { get; set; }
        public string lastPage { get; set; }
    }

    public class PaymentTerms
    {
        public int paymentTermsNumber { get; set; }
        public int daysOfCredit { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string paymentTermsType { get; set; }
        public string self { get; set; }
    }

    public class Pdf
    {
        public string download { get; set; }
    }

    public class Recipient
    {
        public string name { get; set; }
        public string address { get; set; }
        public string zip { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string ean { get; set; }
        public VatZone vatZone { get; set; }
        public string cvr { get; set; }        
    }

    public class References
    {
        public CustomerContact customerContact { get; set; }
        public SalesPerson salesPerson { get; set; }
        public string other { get; set; }
    }

   
    public class SalesPerson
    {
        public int employeeNumber { get; set; }
        public string self { get; set; }
    }

    public class Soap
    {
        public CurrentInvoiceHandle currentInvoiceHandle { get; set; }
    }
              
     
    public class Line : GenericDataResponse
{
    public int lineNumber { get; set; }
    public int sortKey { get; set; }
    public string description { get; set; }
    public decimal quantity { get; set; }
    public decimal unitNetPrice { get; set; }
    public decimal discountPercentage { get; set; }
    public decimal unitCostPrice { get; set; }
    public decimal vatRate { get; set; }
    public decimal vatAmount { get; set; }
    public decimal totalNetAmount { get; set; }
    public Product product { get; set; }
    public DepartmentalDistribution departmentalDistribution { get; set; }
    public Accrual accrual { get; set; }
}
public class Product
{
    public string productNumber { get; set; }
    public string self { get; set; }
}
public class Department
{
    public int departmentNumber { get; set; }
    public string self { get; set; }
}

public class DepartmentalDistribution
{
    public int departmentalDistributionNumber { get; set; }
    public string name { get; set; }
    public bool barred { get; set; }
    public string distributionType { get; set; }
    public List<Distribution> distributions { get; set; }
    public string self { get; set; }
}

public class Distribution
{
    public int percentage { get; set; }
    public Department department { get; set; }
}
public class Accrual
{
    public DateTime? startDate { get; set; }
    public DateTime? endDate { get; set; }
}
public class Templates
    {
        public string bookingInstructions { get; set; }
        public string self { get; set; }
    }

    public class VatZone
    {
        public string name { get; set; }
        public int vatZoneNumber { get; set; }
        public bool enabledForCustomer { get; set; }
        public bool enabledForSupplier { get; set; }
        public string self { get; set; }
    }
    public class EconomicDraftInvoiceWrapper
    {
        public EconomicDraftInvoiceWrapper() { this.Info = new List<string>(); }
        public IList<string> Info { get; set; }
        public bool Success { get; set; }
        public DraftInvoiceResponseData Invoice { get; set; }
    }
}