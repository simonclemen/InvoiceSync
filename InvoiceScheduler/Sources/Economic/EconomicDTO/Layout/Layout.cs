using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.Layout
{
 
    public class LayoutResponse : GenericResponse<LayoutResponseData>
    {
    }


    public class LayoutResponseData : GenericDataResponse
    {
        public int layoutNumber { get; set; }
        public string name { get; set; }
        public string self { get; set; }
    }
}
