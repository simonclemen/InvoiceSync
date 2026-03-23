using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.VatZone
{

    public class VatZoneResponse : GenericResponse<VatZoneResponseData>
    {
    }

    public class VatZoneResponseData : GenericDataResponse
    {
        public string name { get; set; }
        public int vatZoneNumber { get; set; }
        public bool enabledForCustomer { get; set; }
        public bool enabledForSupplier { get; set; }
        public string self { get; set; }
    }


}




