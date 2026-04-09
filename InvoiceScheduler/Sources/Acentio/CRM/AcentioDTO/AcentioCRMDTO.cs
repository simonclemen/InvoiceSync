using EconomicDataTransfer_Consumer_Economic;
using EconomicDataTransfer_Consumer_Economic.CustomerGroup;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace InvoiceScheduler_Consumer_AcentioCRM
{
    public class CombinedDataSet
    {
        public CombinedDataSet()
        {
        }

        public CombinedDataSet(SalesOrderResponse salesorders, SalesOrderItemResponse salesorderitems, AccountResponse accounts)
        {
            SalesOrders = salesorders;
            SalesOrderItems = salesorderitems;
            Accounts = accounts;
        }

        public CombinedDataSet(UserResponse users, AccountResponse accounts, ContactResponse contacts, ProductResponse products, ProductCategoryResponse productCategories, LeadResponse leads, OpportunityResponse opportunities, SalesOrderResponse salesorders, SalesOrderItemResponse salesorderitems, InvoiceResponse invoices, InvoiceItemResponse invoiceitems, CreditNoteResponse creditnotes, CreditNoteItemResponse creditnoteitems)
        {
            Users = users;
            Accounts = accounts;
            Contacts = contacts;
            Products = products;
            ProductCategories = productCategories;
            SalesOrders = salesorders;
            SalesOrderItems = salesorderitems;
            Invoices = invoices;
            InvoiceItems = invoiceitems;
            CreditNotes = creditnotes;
            CreditNoteItems = creditnoteitems;
            Opportunities = opportunities;
            Leads = leads;
        }
        public UserResponse Users { get; set; }

        public AccountResponse Accounts { get; set; }
        public ContactResponse Contacts { get; set; }
        public ProductResponse Products { get; set; }
        public ProductCategoryResponse ProductCategories { get; set; }
        public SalesOrderResponse SalesOrders { get; set; }
        public SalesOrderItemResponse SalesOrderItems { get; set; }
        public InvoiceResponse Invoices { get; set; }
        public InvoiceItemResponse InvoiceItems { get; set; }
        public CreditNoteResponse CreditNotes { get; set; }
        public CreditNoteItemResponse CreditNoteItems { get; set; }

        public LeadResponse Leads { get; set; }
        public OpportunityResponse Opportunities { get; set; }

        internal void Combine(CombinedDataSet data2)
        {
            if (this.Users == null) this.Users = new UserResponse() { list = new List<UserResponseData>() };
            if (this.Accounts == null) this.Accounts = new AccountResponse() { list = new List<AccountResponseData>() };
            if (this.Contacts == null) this.Contacts = new ContactResponse() { list = new List<ContactResponseData>() };
            if (this.Products == null) this.Products = new ProductResponse() { list = new List<ProductResponseData>() };
            if (this.ProductCategories == null) this.ProductCategories = new ProductCategoryResponse() { list = new List<ProductCategoryResponseData>() };
            if (this.SalesOrders == null) this.SalesOrders = new SalesOrderResponse() { list = new List<SalesOrderResponseData>() };
            if (this.SalesOrderItems == null) this.SalesOrderItems = new SalesOrderItemResponse() { list = new List<SalesOrderItemResponseData>() };
            if (this.Invoices == null) this.Invoices = new InvoiceResponse() { list = new List<InvoiceResponseData>() };
            if (this.InvoiceItems == null) this.InvoiceItems = new InvoiceItemResponse() { list = new List<InvoiceItemResponseData>() };
            if (this.CreditNotes == null) this.CreditNotes = new CreditNoteResponse() { list = new List<CreditNoteResponseData>() };
            if (this.CreditNoteItems == null) this.CreditNoteItems = new CreditNoteItemResponse() { list = new List<CreditNoteItemResponseData>() };
            if (this.Opportunities == null) this.Opportunities = new OpportunityResponse() { list = new List<OpportunityResponseData>() };
            if (this.Leads == null) this.Leads = new LeadResponse() { list = new List<LeadResponseData>() };



            Combine(this.Users, data2.Users);
            Combine(this.Accounts, data2.Accounts);
            Combine(this.Contacts, data2.Contacts);
            Combine(this.Products, data2.Products);
            Combine(this.ProductCategories, data2.ProductCategories);
            Combine(this.SalesOrders, data2.SalesOrders);
            Combine(this.SalesOrderItems, data2.SalesOrderItems);
            Combine(this.Invoices, data2.Invoices);
            Combine(this.InvoiceItems, data2.InvoiceItems);
            Combine(this.CreditNotes, data2.CreditNotes);
            Combine(this.CreditNoteItems, data2.CreditNoteItems);
            Combine(this.Opportunities, data2.Opportunities);
            Combine(this.Leads, data2.Leads);
        }
        public void Combine<TData>(GenericResponse<TData> data, GenericResponse<TData> data2) where TData : GenericDataResponse
        {            
            if (data != null && data2 != null)
            {
                data.total += data2.total;
                if (data.list != null && data2.list != null) data.list.AddRange(data2.list);
            }
        }


        private void Combine(GenericResponse<GenericDataResponse> users1, GenericResponse<GenericDataResponse> users2)
        {
            if (users2 != null)
            {
                users1.total = users2.total;
                users1.list.AddRange(users2.list);
            }
        }
    }

    public abstract class GenericResponse<T> where T : GenericDataResponse
    {
        public bool Success { get { return total == 0 || (list != null && list.Count > 0); } }
        public int total { get; set; }
        public List<T> list { get; set; }
    }
    public abstract class GenericDataResponse
    {
        public string id { get; set; }
        public string line_id { get; set; }
        public string Hash { get; set; }

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

    public class ProductResponse : GenericResponse<ProductResponseData>
    {
    }

    public class ProductResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public bool deleted { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public object partNumber { get; set; }
        public object url { get; set; }
        public object description { get; set; }
        public string pricingType { get; set; }
        public int pricingFactor { get; set; }
        public object costPrice { get; set; }
        public int listPrice { get; set; }
        public int unitPrice { get; set; }
        public bool allowFractionalQuantity { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public object weight { get; set; }
        public bool isTaxFree { get; set; }
        public bool isInventory { get; set; }
        public object inventoryNumberType { get; set; }
        public object expirationDays { get; set; }
        public string removalStrategy { get; set; }
        public object variantOrder { get; set; }
        public string costPriceCurrency { get; set; }
        public string listPriceCurrency { get; set; }
        public string unitPriceCurrency { get; set; }
        public object brandId { get; set; }
        public object brandName { get; set; }
        public string categoryId { get; set; }
        public object categoryName { get; set; }
        public object costPriceConverted { get; set; }
        public int listPriceConverted { get; set; }
        public int unitPriceConverted { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public string modifiedById { get; set; }
        public string modifiedByName { get; set; }
        public object templateId { get; set; }
        public object templateName { get; set; }
        public int versionNumber { get; set; }

        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }

        public string cSyncProductCategoryId { get; set; }

        public string cERPSource { get; set; }


    }
    public class ProductCategoryResponse : GenericResponse<ProductCategoryResponseData>
    {
    }

    public class ProductCategoryResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public bool deleted { get; set; }
        public int order { get; set; }
        public object description { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public object modifiedById { get; set; }
        public object modifiedByName { get; set; }
        public object parentId { get; set; }
        public object parentName { get; set; }

        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }

        public string cERPSource { get; set; }

    }

    public class InvoiceResponse : GenericResponse<InvoiceResponseData>
    {
    }
    public class InvoiceResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public bool deleted { get; set; }
        public string number { get; set; }
        public string numberA { get; set; }
        public string status { get; set; }
        public string dateInvoiced { get; set; }
        public string dateDue { get; set; }
        public object description { get; set; }
        public object taxRate { get; set; }
        public object shippingCost { get; set; }
        public object shippingTaxMode { get; set; }
        public decimal taxAmount { get; set; }
        public decimal discountAmount { get; set; }
        public decimal amount { get; set; }
        public decimal amountDue { get; set; }
        public string amountDueCurrency { get; set; }
        public decimal preDiscountedAmount { get; set; }
        public decimal grandTotalAmount { get; set; }
        public int weight { get; set; }
        public object buyerReference { get; set; }
        public string purchaseOrderReference { get; set; }
        public object note { get; set; }
        public bool isDone { get; set; }
        public bool isNotActual { get; set; }
        public bool isLocked { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string billingAddressStreet { get; set; }
        public string billingAddressCity { get; set; }
        public string billingAddressState { get; set; }
        public string billingAddressCountry { get; set; }
        public string billingAddressPostalCode { get; set; }
        public object shippingAddressStreet { get; set; }
        public object shippingAddressCity { get; set; }
        public object shippingAddressState { get; set; }
        public object shippingAddressCountry { get; set; }
        public object shippingAddressPostalCode { get; set; }
        public object shippingCostCurrency { get; set; }
        public string taxAmountCurrency { get; set; }
        public string discountAmountCurrency { get; set; }
        public string amountCurrency { get; set; }
        public string preDiscountedAmountCurrency { get; set; }
        public string grandTotalAmountCurrency { get; set; }
        public string accountId { get; set; }
        public object accountName { get; set; }
        public object opportunityId { get; set; }
        public object opportunityName { get; set; }
        public object quoteId { get; set; }
        public object quoteName { get; set; }
        public string salesOrderId { get; set; }
        public string salesOrderName { get; set; }
        public string billingContactId { get; set; }
        public string billingContactName { get; set; }
        public object shippingContactId { get; set; }
        public object shippingContactName { get; set; }
        public object taxId { get; set; }
        public object taxName { get; set; }
        public object shippingProviderId { get; set; }
        public object shippingProviderName { get; set; }
        public object shippingCostConverted { get; set; }
        public decimal taxAmountConverted { get; set; }
        public decimal discountAmountConverted { get; set; }
        public decimal amountConverted { get; set; }
        public decimal preDiscountedAmountConverted { get; set; }
        public decimal grandTotalAmountConverted { get; set; }
        public object priceBookId { get; set; }
        public object priceBookName { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public string modifiedById { get; set; }
        public string modifiedByName { get; set; }
        public object assignedUserId { get; set; }
        public object assignedUserName { get; set; }
        public int versionNumber { get; set; }

        public IList<InvoiceItemResponseData> itemList { get; set; }
        public string cAccruedStart { get; set; }
        public string cAccruedEnd { get; set; }
        public string cPDFInvoice { get; set; }
        public string cERPSource { get; set; }
        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }
        public string cERPInvoiceNo { get; set; }
        public string cERPDraftInvoiceNo { get; set; }
        public string cERPOrderNo { get; set; }
        
        public string cInvoicedFrom { get; set; }

        public string cAcentioInvoiceReference { get; set; }
        public string cPlatform { get; set; }
        public string cUsageType { get; set; }
        public string cType { get; internal set; }

        public string cInvoiceNumber { get; set; }
    }

    public class InvoiceItemResponse : GenericResponse<InvoiceItemResponseData>
    {
    }
    public class InvoiceItemResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public bool deleted { get; set; }
        public string invoiceStatus { get; set; }
        public object allowFractionalQuantity { get; set; }
        public object productType { get; set; }
        public decimal quantity { get; set; }
        public int quantityInt { get; set; }
        public decimal listPrice { get; set; }
        public decimal unitPrice { get; set; }
        public decimal discount { get; set; }
        public decimal amount { get; set; }
        public object unitWeight { get; set; }
        public object weight { get; set; }
        public decimal taxRate { get; set; }
        public string taxedAmount { get; set; }
        public int order { get; set; }
        public string description { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string listPriceCurrency { get; set; }
        public string unitPriceCurrency { get; set; }
        public string amountCurrency { get; set; }
        public string invoiceId { get; set; }
        public string invoiceName { get; set; }
        public object accountId { get; set; }
        public object accountName { get; set; }
        public string productId { get; set; }
        public object productName { get; set; }
        public decimal listPriceConverted { get; set; }
        public decimal unitPriceConverted { get; set; }
        public decimal amountConverted { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public object modifiedById { get; set; }
        public object modifiedByName { get; set; }



        public string cERPSource { get; set; }
        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }



    }
    public class CreditNoteResponse : GenericResponse<CreditNoteResponseData>
    {
    }
    public class CreditNoteResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public bool deleted { get; set; }
        public string number { get; set; }
        public string numberA { get; set; }
        public string status { get; set; }
        public string dateIssued { get; set; }
        public string dateDue { get; set; }
        public bool appliedToInvoice { get; set; }
        public object description { get; set; }
        public object taxRate { get; set; }
        public decimal taxAmount { get; set; }
        public decimal amount { get; set; }
        public object shippingCost { get; set; }
        public object shippingTaxMode { get; set; }
        public decimal grandTotalAmount { get; set; }
        public decimal amountDue { get; set; }
        public string amountDueCurrency { get; set; }
        public object buyerReference { get; set; }
        public object purchaseOrderReference { get; set; }
        public object note { get; set; }
        public bool isDone { get; set; }
        public bool isNotActual { get; set; }
        public bool isLocked { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string taxAmountCurrency { get; set; }
        public string amountCurrency { get; set; }
        public object shippingCostCurrency { get; set; }
        public string grandTotalAmountCurrency { get; set; }
        public string accountId { get; set; }
        public object accountName { get; set; }
        public string invoiceId { get; set; }
        public string invoiceName { get; set; }
        public string billingContactId { get; set; }
        public string billingContactName { get; set; }
        public object taxId { get; set; }
        public object taxName { get; set; }
        public decimal taxAmountConverted { get; set; }
        public decimal amountConverted { get; set; }
        public object shippingCostConverted { get; set; }
        public decimal grandTotalAmountConverted { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public object modifiedById { get; set; }
        public object modifiedByName { get; set; }
        public object assignedUserId { get; set; }
        public object assignedUserName { get; set; }
        public int versionNumber { get; set; }

        public string cERPCreditNoteNo { get; set; }
        public string cERPDraftCreditNoteNo { get; set; }

        public string cPDFInvoice { get; set; }
        public IList<CreditNoteItemResponseData> itemList { get; set; }
        

        public string cCreditNoteNumber { get; set; }
        public InvoiceResponseData Invoice { get; set; }
    }
    public class CreditNoteItemResponse : GenericResponse<CreditNoteItemResponseData>
    {
    }
    public class CreditNoteItemResponseData : GenericDataResponse
    {       
        public string name { get; set; }
        public bool deleted { get; set; }
        public string creditNoteStatus { get; set; }
        public bool allowFractionalQuantity { get; set; }
        public string productType { get; set; }
        public decimal quantity { get; set; }
        public int quantityInt { get; set; }
        public decimal unitPrice { get; set; }
        public decimal amount { get; set; }
        public decimal taxRate { get; set; }
        public string taxedAmount { get; set; }
        public int order { get; set; }
        public string description { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string unitPriceCurrency { get; set; }
        public string amountCurrency { get; set; }
        public string creditNoteId { get; set; }
        public string creditNoteName { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public string productId { get; set; }
        public string productName { get; set; }
        public double unitPriceConverted { get; set; }
        public double amountConverted { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public object modifiedById { get; set; }
        public object modifiedByName { get; set; }
    }
    public class ContactResponse : GenericResponse<ContactResponseData>
    {

    }
    public class ContactResponseData : GenericDataResponse
    {
        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }

        public string name { get; set; }
        public bool deleted { get; set; }
        public string salutationName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string title { get; set; }
        public object description { get; set; }
        public string emailAddress { get; set; }
        public object phoneNumber { get; set; }
        public object cPhoneNumber { get; set; }
        public bool doNotCall { get; set; }
        public object addressStreet { get; set; }
        public object addressCity { get; set; }
        public object addressState { get; set; }
        public object addressCountry { get; set; }
        public object addressPostalCode { get; set; }
        public bool? accountIsInactive { get; set; }
        public string accountType { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public bool hasPortalUser { get; set; }
        public object middleName { get; set; }
        public object emailAddressIsOptedOut { get; set; }
        public object emailAddressIsInvalid { get; set; }
        public object phoneNumberIsOptedOut { get; set; }
        public object phoneNumberIsInvalid { get; set; }
        public string streamUpdatedAt { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public object campaignId { get; set; }
        public object campaignName { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public string modifiedById { get; set; }
        public string modifiedByName { get; set; }
        public object assignedUserId { get; set; }
        public object assignedUserName { get; set; }
        public object portalUserId { get; set; }
        public object portalUserName { get; set; }
        public object originalLeadId { get; set; }
        public object originalLeadName { get; set; }
    }

    public class AccountResponse : GenericResponse<AccountResponseData>
    {

    }
    public class AccountResponseData : GenericDataResponse
    {

        public string name { get; set; }
        public bool deleted { get; set; }
        public string website { get; set; }
        public object emailAddress { get; set; }
        public object phoneNumber { get; set; }

        public string cAccountPhoneNumber { get; set; }


        public string type { get; set; }
        public object industry { get; set; }
        public object sicCode { get; set; }
        public string billingAddressStreet { get; set; }
        public string billingAddressCity { get; set; }
        public string billingAddressState { get; set; }
        public string billingAddressCountry { get; set; }
        public string billingAddressPostalCode { get; set; }
        public string shippingAddressStreet { get; set; }
        public string shippingAddressCity { get; set; }
        public object shippingAddressState { get; set; }
        public string shippingAddressCountry { get; set; }
        public string shippingAddressPostalCode { get; set; }
        public object description { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public object taxNumber { get; set; }
        public object electronicAddressScheme { get; set; }
        public object electronicAddressIdentifier { get; set; }
        public object cVATNo { get; set; }
        public string cEANGLNNo { get; set; }
        public string cEmailForPDFInvoicing { get; set; }
        public string cCustomerPrivatePublic { get; set; }
        public string cCustomerType { get; set; }
        public string cPaymentTerms { get; set; }
        public string cERPSupplier { get; set; }
        public string cProcurement { get; set; }
        public string cInvoiceSystem { get; set; }
        public string cContractMgmt { get; set; }
        public string cSpendMgmt { get; set; }
        public object cInvoiceCurrency { get; set; }
        public object cLastERPSync { get; set; }
        public bool cBarred { get; set; }
        public string cECVATZone { get; set; }
        public string cInvoicedFrom { get; set; }
        public string cECCustomerGroup { get; set; }
        public string cERPCustomerNo { get; set; }
        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }

        public string cERPSource { get; set; }

        public object emailAddressIsOptedOut { get; set; }
        public object emailAddressIsInvalid { get; set; }
        public object phoneNumberIsOptedOut { get; set; }
        public object phoneNumberIsInvalid { get; set; }
        public object cInvoiceCurrencyCurrency { get; set; }
        public string streamUpdatedAt { get; set; }
        public object campaignId { get; set; }
        public object campaignName { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public string modifiedById { get; set; }
        public string modifiedByName { get; set; }
        public string assignedUserId { get; set; }
        public string assignedUserName { get; set; }
        public object originalLeadId { get; set; }
        public object originalLeadName { get; set; }
        public object priceBookId { get; set; }
        public object priceBookName { get; set; }
        public object cInvoiceCurrencyConverted { get; set; }
        public bool isStarred { get; set; }
        public int versionNumber { get; set; }
        public object supplierId { get; set; }
        public object supplierName { get; set; }



    }
    public class UserResponse : GenericResponse<UserResponseData>
    {
    }
    public class UserResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public bool deleted { get; set; }
        public string userName { get; set; }
        public string type { get; set; }
        public string salutationName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool isActive { get; set; }
        public string title { get; set; }
        public string emailAddress { get; set; }
        public object phoneNumber { get; set; }
        public object avatarColor { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public object middleName { get; set; }
        public bool emailAddressIsOptedOut { get; set; }
        public bool emailAddressIsInvalid { get; set; }
        public object phoneNumberIsOptedOut { get; set; }
        public object phoneNumberIsInvalid { get; set; }
        public object defaultTeamId { get; set; }
        public object defaultTeamName { get; set; }
        public object contactId { get; set; }
        public object contactName { get; set; }
        public object avatarId { get; set; }
        public object avatarName { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public object workingTimeCalendarId { get; set; }
    }

    public class ChallangeResponseData
    {
        public string token { get; set; }
        public long serverTime { get; set; }
        public long expireTime { get; set; }

    }
    public class ChallangeResponse
    {
        public bool success { get; set; }
        public ChallangeResponseData result { get; set; }
    }
    public class LoginResponse
    {
        public bool success { get; set; }
        public LoginResponseData result { get; set; }
    }
    public class LoginResponseData
    {
        public string sessionName { get; set; }
        public string userId { get; set; }

    }

    public class SalesOrderResponse : GenericResponse<SalesOrderResponseData>
    {
    }
    public class SalesOrderResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public bool deleted { get; set; }
        public string number { get; set; }
        public string numberA { get; set; }
        public string status { get; set; }
        public object dateOrdered { get; set; }
        public object dateInvoiced { get; set; }
        public object description { get; set; }
        public object taxRate { get; set; }
        public object shippingCost { get; set; }
        public object shippingTaxMode { get; set; }
        public decimal taxAmount { get; set; }
        public decimal discountAmount { get; set; }
        public decimal amount { get; set; }
        public decimal preDiscountedAmount { get; set; }
        public decimal grandTotalAmount { get; set; }
        public int weight { get; set; }
        public bool isDone { get; set; }
        public bool isNotActual { get; set; }
        public bool isLocked { get; set; }
        public bool isHardLocked { get; set; }
        public bool isDeliveryCreated { get; set; }
        public bool hasInventoryItems { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string cContractStartDate { get; set; }
        public string cContractEndDate { get; set; }
        public string cContractPeriodOptions { get; set; }
        public int? cNoticeOfTerminationMonths { get; set; }
        public int? cIrrevocabilityMonth { get; set; }
        public object cReplacedBySO { get; set; }
        public string cPlatform { get; set; }
        public List<object> cModulesServices { get; set; }
        public object cDPASigned { get; set; }
        public string cInvoicedFrom { get; set; }
        public string cPOnumberReference { get; set; }
        public object cAdditionalInvoiceText { get; set; }
        public string cInvoicePaymentType { get; set; }
        public string cInvoiceFrequency { get; set; }
        public string cWhenToSendInvoice { get; set; }
        public object cLatestInvoiceDate { get; set; }
        public int cVersion { get; set; }
        public string cAcentioInvoiceReference { get; set; }
        public object cPriceIndexing { get; set; }
        public object cNextAdjustmentDate { get; set; }
        public object cSetPercentage { get; set; }
        public string cLicensePeriodStart { get; set; }
        public string cAccruedStartDate { get; set; }
        public string cAccruedEndDate { get; set; }

        public object cNextInvoiceDate { get; set; }
        public object cContractPeriodOptionsUsed { get; set; }
        public object cReplacedReason { get; set; }
        public object cTerminationReason { get; set; }
        public string cSoldDate { get; set; }
        public string cType { get; set; }
        public string cInvoiceDate { get; set; }
        public bool cCreateInvoice { get; set; }
        public bool cSyncInvoiceToERP { get; set; }
        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }
        public string cSimplySO { get; set; }
        public object cAdjustmentFloor { get; set; }
        public string billingAddressStreet { get; set; }
        public string billingAddressCity { get; set; }
        public object billingAddressState { get; set; }
        public string billingAddressCountry { get; set; }
        public string billingAddressPostalCode { get; set; }
        public object shippingAddressStreet { get; set; }
        public object shippingAddressCity { get; set; }
        public object shippingAddressState { get; set; }
        public object shippingAddressCountry { get; set; }
        public object shippingAddressPostalCode { get; set; }
        public object shippingCostCurrency { get; set; }
        public string taxAmountCurrency { get; set; }
        public string discountAmountCurrency { get; set; }
        public string amountCurrency { get; set; }
        public string preDiscountedAmountCurrency { get; set; }
        public string grandTotalAmountCurrency { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public object opportunityId { get; set; }
        public object opportunityName { get; set; }
        public object quoteId { get; set; }
        public object quoteName { get; set; }
        public string billingContactId { get; set; }
        public string billingContactName { get; set; }
        public object shippingContactId { get; set; }
        public object shippingContactName { get; set; }
        public object taxId { get; set; }
        public object taxName { get; set; }
        public object shippingProviderId { get; set; }
        public object shippingProviderName { get; set; }
        public object shippingCostConverted { get; set; }
        public decimal taxAmountConverted { get; set; }
        public decimal discountAmountConverted { get; set; }
        public decimal amountConverted { get; set; }
        public decimal preDiscountedAmountConverted { get; set; }
        public decimal grandTotalAmountConverted { get; set; }
        public object priceBookId { get; set; }
        public object priceBookName { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public object modifiedById { get; set; }
        public object modifiedByName { get; set; }
        public object assignedUserId { get; set; }
        public object assignedUserName { get; set; }
        public bool isStarred { get; set; }
        public int versionNumber { get; set; }

        public int cInvoiceCreationOffset { get; set; }

        public string cInvoiceCreationState { get; set; }

        public string cUsageType { get; set; }

        public List<SalesOrderItemResponseData> itemList { get; set; }

    }

    public class LeadResponse : GenericResponse<LeadResponseData>
    {
    }

    public class LeadResponseData : GenericDataResponse
    {


        public string name { get; set; }
        public bool deleted { get; set; }
        public int amount { get; set; }
        public int amountWeightedConverted { get; set; }
        public string stage { get; set; }
        public string lastStage { get; set; }
        public int probability { get; set; }
        public string leadSource { get; set; }
        public string closeDate { get; set; }
        public object description { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string amountCurrency { get; set; }
        public string streamUpdatedAt { get; set; }
        public int amountConverted { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public string contactId { get; set; }
        public object contactName { get; set; }
        public object campaignId { get; set; }
        public object campaignName { get; set; }
        public object originalLeadId { get; set; }
        public object originalLeadName { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public string modifiedById { get; set; }
        public string modifiedByName { get; set; }
        public string assignedUserId { get; set; }
        public string assignedUserName { get; set; }
        public object priceBookId { get; set; }
        public object priceBookName { get; set; }
        public int versionNumber { get; set; }

        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }

    }

    public class OpportunityResponse : GenericResponse<OpportunityResponseData>
    {

    }

    public class OpportunityResponseData : GenericDataResponse
    {

        public string name { get; set; }
        public bool deleted { get; set; }
        public int amount { get; set; }
        public int amountWeightedConverted { get; set; }
        public string stage { get; set; }
        public string lastStage { get; set; }
        public int probability { get; set; }
        public string leadSource { get; set; }
        public string closeDate { get; set; }
        public object description { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public object cOnedrivelink { get; set; }
        public object cTender { get; set; }
        public object cAdditionalservices { get; set; }
        public object cSystemlansdcapenotes { get; set; }
        public object cErpsupplier { get; set; }
        public object cProcurement { get; set; }
        public object cInvoice { get; set; }
        public object cContractmgmt { get; set; }
        public object cSpendmgmt { get; set; }
        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }
        public string amountCurrency { get; set; }
        public string streamUpdatedAt { get; set; }
        public int amountConverted { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public object contactId { get; set; }
        public object contactName { get; set; }
        public object campaignId { get; set; }
        public object campaignName { get; set; }
        public object originalLeadId { get; set; }
        public object originalLeadName { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public string modifiedById { get; set; }
        public string modifiedByName { get; set; }
        public string assignedUserId { get; set; }
        public string assignedUserName { get; set; }
        public object priceBookId { get; set; }
        public object priceBookName { get; set; }
        public int versionNumber { get; set; }

    }



    public class SalesOrderItemResponse : GenericResponse<SalesOrderItemResponseData>
    {
    }
    public class SalesOrderItemResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public bool deleted { get; set; }
        public string salesOrderStatus { get; set; }
        public bool? allowFractionalQuantity { get; set; }
        public string productType { get; set; }
        public decimal quantity { get; set; }
        public int quantityInt { get; set; }
        public decimal listPrice { get; set; }
        public decimal unitPrice { get; set; }
        public decimal discount { get; set; }
        public decimal amount { get; set; }
        public object inventoryNumberType { get; set; }
        public bool? isInventory { get; set; }
        public object unitWeight { get; set; }
        public object weight { get; set; }
        public decimal taxRate { get; set; }
        public int order { get; set; }
        public string description { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string listPriceCurrency { get; set; }
        public string unitPriceCurrency { get; set; }
        public string amountCurrency { get; set; }
        public string salesOrderId { get; set; }
        public string salesOrderName { get; set; }
        public string accountId { get; set; }
        public string accountName { get; set; }
        public string productId { get; set; }
        public string productName { get; set; }
        public decimal listPriceConverted { get; set; }
        public decimal unitPriceConverted { get; set; }
        public decimal amountConverted { get; set; }
        public string createdById { get; set; }
        public string createdByName { get; set; }
        public string modifiedById { get; set; }
        public string modifiedByName { get; set; }
        public string cSyncid { get; set; }
        public string cSyncHash { get; set; }
    }

    public class CurrencyResponse : GenericResponse<CurrencyResponseData>
    {
    }
    public class CurrencyResponseData : GenericDataResponse
    {
        public string currency_name { get; set; }
        public string currency_code { get; set; }
        public string currency_symbol { get; set; }
        public string conversion_rate { get; set; }
        public string currency_status { get; set; }
    }

    public class StreamNote
    {
        public string type { get; set; }
        public string parentType { get; set; }
        public string parentId { get; set; }

        public string post { get; set; }

    }
}
