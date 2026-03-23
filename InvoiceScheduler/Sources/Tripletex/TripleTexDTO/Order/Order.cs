using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleTexDataTransfer_Consumer_TripleTex.Order
{
    public class OrderResponse : GenericResponse<OrderResponseData>
    {
    }
    public class OrderResponseWrapper
    {
        public OrderResponseData value { get; set; }
    }
    public class OrderLineResponseWrapper
    {
        public OrderLine value { get; set; }
    }

    
    public class OrderResponseData : GenericDataResponse
    {
        public int version { get; set; }
        public Customer customer { get; set; }
        //public Contact contact { get; set; }
        //public object attn { get; set; }
        public string displayName { get; set; }
        //public string receiverEmail { get; set; }
        //public string overdueNoticeEmail { get; set; }
        //public string number { get; set; }
        //public string reference { get; set; }
        //public OurContact ourContact { get; set; }
        //public object department { get; set; }
        public string orderDate { get; set; }
        //public object project { get; set; }
        public string invoiceComment { get; set; }
        //public Currency currency { get; set; }
        //public int invoicesDueIn { get; set; }
        //public string invoicesDueInType { get; set; }
        //public bool isShowOpenPostsOnInvoices { get; set; }
        //public bool isClosed { get; set; }
        public string deliveryDate { get; set; }
        //public object deliveryAddress { get; set; }
        //public string deliveryComment { get; set; }
        //public bool isPrioritizeAmountsIncludingVat { get; set; }
        //public string orderLineSorting { get; set; }
        //public List<object> orderGroups { get; set; }
        public List<OrderLine> orderLines { get; set; }
        //public bool isSubscription { get; set; }
        //public int subscriptionDuration { get; set; }
        //public string subscriptionDurationType { get; set; }
        //public int subscriptionPeriodsOnInvoice { get; set; }
        //blic string subscriptionPeriodsOnInvoiceType { get; set; }
        //public string subscriptionInvoicingTimeInAdvanceOrArrears { get; set; }
        //public int subscriptionInvoicingTime { get; set; }
        //public string subscriptionInvoicingTimeType { get; set; }
        //public bool isSubscriptionAutoInvoicing { get; set; }
        public PreliminaryInvoice preliminaryInvoice { get; set; }
        //public List<object> attachment { get; set; }
        //public string sendMethodDescription { get; set; }
        //public bool invoiceOnAccountVatHigh { get; set; }
        //public decimal totalInvoicedOnAccountAmountAbsoluteCurrency { get; set; }
        //public bool invoiceSendSMSNotification { get; set; }
        //public string invoiceSMSNotificationNumber { get; set; }
        //public decimal markUpOrderLines { get; set; }
        //public decimal discountPercentage { get; set; }
        //public string customerName { get; set; }
        //public string projectManagerNameAndNumber { get; set; }
        //public List<object> travelReports { get; set; }
        //public List<object> accountingDimensionValues { get; set; }
    }
    //2127893392
    public class PreliminaryInvoice
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Contact
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Currency
    {
        public int id { get; set; }
        public string url { get; set; }
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
       // public decimal amountIncludingVatCurrency { get; set; }
        public object vendor { get; set; }
        public Order order { get; set; }
        //ublic decimal unitPriceIncludingVatCurrency { get; set; }
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

    public class OurContact
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Product
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    
    public class VatType
    {
        public int id { get; set; }
        public string url { get; set; }
    }


}
