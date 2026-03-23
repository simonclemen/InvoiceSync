using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.Product
{
    public class ProductResponse : GenericResponse<ProductResponseData>
    {
    }

    public class ProductResponseData : GenericDataResponse
    {
        public string productNumber { get; set; }
        public string name { get; set; }
        public double costPrice { get; set; }
        public double recommendedPrice { get; set; }
        public double salesPrice { get; set; }
        public bool barred { get; set; }
        public DateTime lastUpdated { get; set; }
        public DepartmentalDistribution departmentalDistribution { get; set; }
        public ProductGroup productGroup { get; set; }
        public Invoices invoices { get; set; }
        public Pricing pricing { get; set; }
        public string self { get; set; }
        public Unit unit { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Accrual
    {
        public int accountNumber { get; set; }
        public string accountType { get; set; }
        public double balance { get; set; }
        public bool blockDirectEntries { get; set; }
        public string debitCredit { get; set; }
        public string name { get; set; }
        public string accountingYears { get; set; }
        public string self { get; set; }
    }


    public class Create
    {
        public string description { get; set; }
        public string href { get; set; }
        public string httpMethod { get; set; }
    }

    public class DepartmentalDistribution
    {
        public int departmentalDistributionNumber { get; set; }
        public string distributionType { get; set; }
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

    
    public class Pricing
    {
        public string currencySpecificSalesPrices { get; set; }
    }

    public class ProductGroup
    {
        public int productGroupNumber { get; set; }
        public string name { get; set; }
        public string salesAccounts { get; set; }
        public string products { get; set; }
        public Accrual accrual { get; set; }
        public bool inventoryEnabled { get; set; }
        public string self { get; set; }
    }
        

    public class Unit
    {
        public int unitNumber { get; set; }
        public string name { get; set; }
        public string products { get; set; }
        public string self { get; set; }
    }




}
