using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleTexDataTransfer_Consumer_TripleTex.Customer
{
    public class CustomerResponse : GenericResponse<CustomerResponseData>
    {
    }

    public class CustomerResponseData : GenericDataResponse
    {
        
        public int version { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string organizationNumber { get; set; }
        public int globalLocationNumber { get; set; }
        public int supplierNumber { get; set; }
        public int customerNumber { get; set; }
        public bool isSupplier { get; set; }
        public bool isCustomer { get; set; }
        public bool isInactive { get; set; }
        public object accountManager { get; set; }
        public object department { get; set; }
        public string email { get; set; }
        public string invoiceEmail { get; set; }
        public string overdueNoticeEmail { get; set; }
        public string phoneNumber { get; set; }
        public string phoneNumberMobile { get; set; }
        public string description { get; set; }
        public string language { get; set; }
        public string displayName { get; set; }
        public bool isPrivateIndividual { get; set; }
        public bool singleCustomerInvoice { get; set; }
        public string invoiceSendMethod { get; set; }
        public string emailAttachmentType { get; set; }
        public PostalAddress postalAddress { get; set; }
        public PhysicalAddress physicalAddress { get; set; }
        public object deliveryAddress { get; set; }
        public Category category1 { get; set; }
        public Category category2 { get; set; }
        public Category category3 { get; set; }
        public int invoicesDueIn { get; set; }
        public string invoicesDueInType { get; set; }
        public Currency currency { get; set; }
        public List<object> bankAccountPresentation { get; set; }
        public LedgerAccount ledgerAccount { get; set; }
        public bool isFactoring { get; set; }
        public bool invoiceSendSMSNotification { get; set; }
        public string invoiceSMSNotificationNumber { get; set; }
        public bool isAutomaticSoftReminderEnabled { get; set; }
        public bool isAutomaticReminderEnabled { get; set; }
        public bool isAutomaticNoticeOfDebtCollectionEnabled { get; set; }
        public double discountPercentage { get; set; }
        public string website { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class Currency
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class LedgerAccount
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class PhysicalAddress
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class PostalAddress
    {
        public int id { get; set; }
        public int version { get; set; }
        public string url { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string postalCode { get; set; }
        public string city { get; set; }
        public Country country { get; set; }
        public string displayName { get; set; }
        public string addressAsString { get; set; }
        public string displayNameInklMatrikkel { get; set; }
        public int knr { get; set; }
        public int gnr { get; set; }
        public int bnr { get; set; }
        public int fnr { get; set; }
        public int snr { get; set; }
        public string unitNumber { get; set; }
    }

    public class Country
    {
        public int id { get; set; }
        public string url { get; set; }
    }


}
