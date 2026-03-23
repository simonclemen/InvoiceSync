using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.PaymentTerm
{
    public class PaymentTermResponse : GenericResponse<PaymentTermResponseData>
    {
    }

    public class PaymentTermResponseData : GenericDataResponse
    {
        public int paymentTermsNumber { get; set; }
        public int daysOfCredit { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string paymentTermsType { get; set; }
        public string self { get; set; }
    }


}
