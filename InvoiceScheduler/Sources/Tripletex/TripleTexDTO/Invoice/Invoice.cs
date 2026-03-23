using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripleTexDataTransfer_Consumer_TripleTex.Order;
using TripletexOrder = TripleTexDataTransfer_Consumer_TripleTex.Order;
namespace TripleTexDataTransfer_Consumer_TripleTex.Invoice
{
    public class InvoiceResponse : GenericResponse<InvoiceResponseData>
    {
    }
    public class InvoiceBooked
    {
        public IDRef customer { get; set; }
        public IList<IDRef> orders { get; set; }

        public string invoiceDate { get; set; }
        public string invoiceDueDate { get; set; }

        public InvoiceBooked(long customerid, long orderid, string invoiceDueDate, string invoiceDate) {
            this.customer = new IDRef() { id = customerid };
            this.orders = new List<IDRef>();
            this.invoiceDueDate = invoiceDueDate;
            this.invoiceDate = invoiceDate;

            this.orders.Add(new IDRef() { id = orderid });  
        }  
    }
    public class IDRef
    {
        public long id { get; set; }
    }
    public class InvoiceResponseWrapper
    {
        public InvoiceResponseData value { get; set; }
    }
    public class InvoiceResponseData : GenericDataResponse
    {
       
       // public int version { get; set; }
       // public string url { get; set; }
        public int invoiceNumber { get; set; }
        public string invoiceDate { get; set; }
        public Customer customer { get; set; }
        //public int creditedInvoice { get; set; }
        //public bool isCredited { get; set; }
        public string invoiceDueDate { get; set; }
        //public string kid { get; set; }
        public string invoiceComment { get; set; }
        public string comment { get; set; }
        public List<Order> orders { get; set; }
        public List<OrderLine> orderLines { get; set; }
       // public List<object> travelReports { get; set; }
       // public List<ProjectInvoiceDetail> projectInvoiceDetails { get; set; }
       // public Voucher voucher { get; set; }
        public string deliveryDate { get; set; }
        public decimal amount { get; set; }
        public decimal amountCurrency { get; set; }
        public decimal amountExcludingVat { get; set; }
        public decimal amountExcludingVatCurrency { get; set; }
        public decimal amountRoundoff { get; set; }
        public decimal amountRoundoffCurrency { get; set; }
        public decimal amountOutstanding { get; set; }
        public decimal amountCurrencyOutstanding { get; set; }
        public decimal amountOutstandingTotal { get; set; }
        public decimal amountCurrencyOutstandingTotal { get; set; }
        //public int sumRemits { get; set; }
        public Currency currency { get; set; }
        public bool isCreditNote { get; set; }
       // public bool isCharged { get; set; }
        //public bool isApproved { get; set; }
        //public List<Posting> postings { get; set; }
        //public List<Reminder> reminders { get; set; }
        public string invoiceRemarks { get; set; }
        public object invoiceRemark { get; set; }
        //public bool isPeriodizationPossible { get; set; }
        //public int documentId { get; set; }

    }

    public class Currency
    {
        public int id { get; set; }
       // public string url { get; set; }
        //public string isoCode { get { return "NOK"; } }
    }

    public class Customer
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Order
    {
        public int id { get; set; }
        public string url { get; set; }
    }



    public class OrderLine
    {
        public int id { get; set; }
        public int version { get; set; }
        public string url { get; set; }
        public Product product { get; set; }
        public object inventory { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public decimal count { get; set; }
        public decimal unitCostCurrency { get; set; }
        public decimal unitPriceExcludingVatCurrency { get; set; }
        public Currency currency { get; set; }
        public decimal markup { get; set; }
        public decimal discount { get; set; }
        public VatType vatType { get; set; }
        public decimal amountExcludingVatCurrency { get; set; }
        public decimal amountIncludingVatCurrency { get; set; }
        public object vendor { get; set; }
        public Order order { get; set; }
        public decimal unitPriceIncludingVatCurrency { get; set; }
        public bool isSubscription { get; set; }
        public object subscriptionPeriodStart { get; set; }
        public object subscriptionPeriodEnd { get; set; }
        public object orderGroup { get; set; }
        public int sortIndex { get; set; }
        public bool isPicked { get; set; }
        public object pickedDate { get; set; }
        public decimal orderedQuantity { get; set; }
        public bool isCharged { get; set; }
    }

    public class Posting
    {
        public object id { get; set; }
        public string url { get; set; }
    }

    public class Product
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class ProjectInvoiceDetail
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Reminder
    {
        public int id { get; set; }
        public string url { get; set; }
    }

 

    public class VatType
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Voucher
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class TripletexInvoiceWrapper
    {
        public TripletexInvoiceWrapper() { this.Info = new List<string>(); }
        public IList<string> Info { get; set; }
        public bool Success { get; set; }
        public InvoiceResponseData Invoice { get; set; }
        public TripletexOrder.OrderResponseData Order { get; set; }
    }


}
