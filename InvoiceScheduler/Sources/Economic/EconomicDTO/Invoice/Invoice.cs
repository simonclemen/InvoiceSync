using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.Invoice
{
    

    public class InvoiceResponse : GenericResponse<InvoiceResponseData>
    {    
    }


    public class InvoiceResponseData : GenericDataResponse
    {
        public int bookedInvoiceNumber { get; set; }
        public int? orderNumber { get; set; }
        public DateTime date { get; set; }

        public string currency { get; set; }
        public decimal exchangeRate { get; set; }

        public decimal netAmount { get; set; }
        public decimal netAmountInBaseCurrency { get; set; }
        public decimal grossAmount { get; set; }
        public decimal grossAmountInBaseCurrency { get; set; }
        public decimal vatAmount { get; set; }

        public decimal roundingAmount { get; set; }
        public decimal remainder { get; set; }
        public decimal remainderInBaseCurrency { get; set; }

        public DateTime dueDate { get; set; }
        
        public PaymentTermsDto paymentTerms { get; set; }
        public CustomerReferenceDto customer { get; set; }
        public RecipientDto recipient { get; set; }
        public NotesDto notes { get; set; }
        public ReferencesDto references { get; set; }
        public LayoutDto layout { get; set; }

        public PdfDto pdf { get; set; }

        public string sent { get; set; }
        public string self { get; set; }

                
        public List<Line> lines { get; set; }
        
    }
    public class Line: GenericDataResponse
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
    public class PaymentTermsDto
    {
        public int paymentTermsNumber { get; set; }
        public int daysOfCredit { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string paymentTermsType { get; set; }
        public string self { get; set; }
    }

    public class CustomerReferenceDto
    {
        public int customerNumber { get; set; }
        public string self { get; set; }
    }

    public class RecipientDto
    {
        public string name { get; set; }
        public string address { get; set; }
        public string zip { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string ean { get; set; }

        public VatZoneDto vatZone { get; set; }
    }

    public class VatZoneDto
    {
        public string name { get; set; }
        public int vatZoneNumber { get; set; }
        public bool enabledForCustomer { get; set; }
        public bool enabledForSupplier { get; set; }
        public string self { get; set; }
    }

    public class NotesDto
    {
        public string heading { get; set; }
        public string textLine1 { get; set; }
        public string textLine2 { get; set; }
    }

    public class ReferencesDto
    {
        public CustomerContactDto customerContact { get; set; }
        public string other { get; set; }
    }

    public class CustomerContactDto
    {
        public int customerContactNumber { get; set; }
        public string self { get; set; }
    }

    public class LayoutDto
    {
        public int layoutNumber { get; set; }
        public string self { get; set; }
    }

    public class PdfDto
    {
        public string download { get; set; }
    }

    public class EconomicInvoiceWrapper
    {
        public EconomicInvoiceWrapper() { this.Info = new List<string>(); }
        public IList<string> Info { get; set; }
        public bool Success { get; set; }
        public InvoiceResponseData Invoice { get; set; }
    }
}
