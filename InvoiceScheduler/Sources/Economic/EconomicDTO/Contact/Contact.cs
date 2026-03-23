
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.Contact
{

    public class ContactResponse : GenericResponse<ContactResponseData>
    {
    }

    public class ContactResponseData : GenericDataResponse
    {
        public int customerContactNumber { get; set; }
        public string name { get; set; }
        public Customer customer { get; set; }
        public int sortKey { get; set; }
        public string self { get; set; }
        public string email { get; set; }
    }
    public class Customer
    {
        public int customerNumber { get; set; }
        public string self { get; set; }
    }

}




