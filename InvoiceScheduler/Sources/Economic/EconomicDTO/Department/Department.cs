using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EconomicDataTransfer_Consumer_Economic.Department
{
    public class DepartmentResponse : GenericResponse<DepartmentResponseData>
    {
    }


    public class DepartmentResponseData : GenericDataResponse
    {
        public int departmentNumber { get; set; }
        public string name { get; set; }
        public string self { get; set; }
    }
}
