using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.CustomerGroup
{

    public class CustomerGroupResponse : GenericResponse<CustomerGroupResponseData>
    {
    }

    public class CustomerGroupResponseData : GenericDataResponse
    {
        public int customerGroupNumber { get; set; }
        public string name { get; set; }
        public Account account { get; set; }
        public string customers { get; set; }
        public string self { get; set; }
        public Layout layout { get; set; }
    }
    public class Account
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
    public class Layout
    {
        public int layoutNumber { get; set; }
        public string name { get; set; }
        public string self { get; set; }
    }

}




