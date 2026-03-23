using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.ProductGroup
{
    public class ProductGroupResponse : GenericResponse<ProductGroupResponseData>
    {    
    }


    public class ProductGroupResponseData : GenericDataResponse
    {
        public int productGroupNumber { get; set; }
        public string name { get; set; }
        public string salesAccounts { get; set; }
        public string products { get; set; }
        public Accrual accrual { get; set; }
        public bool inventoryEnabled { get; set; }
        public string self { get; set; }
    }
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
   

}
