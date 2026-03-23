using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripleTexDataTransfer_Consumer_TripleTex.Product
{
    public class ProductResponse : GenericResponse<ProductResponseData>
    {
    }

    public class ProductResponseData : GenericDataResponse
    {
        
        public int version { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string number { get; set; }
        public string displayNumber { get; set; }
        public string description { get; set; }
        public string orderLineDescription { get; set; }
        public int costExcludingVatCurrency { get; set; }
        public int costPrice { get; set; }
        public int priceExcludingVatCurrency { get; set; }
        public int priceIncludingVatCurrency { get; set; }
        public bool isInactive { get; set; }
        public ProductUnit productUnit { get; set; }
        public int incomingStock { get; set; }
        public int outgoingStock { get; set; }
        public VatType vatType { get; set; }
        public Currency currency { get; set; }
        public object department { get; set; }
        public object account { get; set; }
        public object supplier { get; set; }
        public object resaleProduct { get; set; }
        public bool isDeletable { get; set; }
        public bool hasSupplierProductConnected { get; set; }
        public object image { get; set; }
        public string displayName { get; set; }
        public object mainSupplierProduct { get; set; }
        public bool isRoundPriceIncVat { get; set; }
        public int priceInTargetCurrency { get; set; }
        public int purchasePriceCurrency { get; set; }

    }

    public class Currency
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class ProductUnit
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
