using EconomicDataTransfer_Consumer_Economic.Customer;
using EconomicDataTransfer_Consumer_Economic.CustomerGroup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EconomicDataTransfer_Consumer_Economic.Employee
{
    public class EmployeeResponse : GenericResponse<EmployeeResponseData>
    {
    }


    public class EmployeeResponseData : GenericDataResponse
    {
        public int employeeNumber { get; set; }
        public EmployeeGroup employeeGroup { get; set; }
        public string name { get; set; }
        public string customers { get; set; }
        public string draftInvoices { get; set; }
        public string bookedInvoices { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string self { get; set; }
        public bool? barred { get; set; }
    }
    public class EmployeeGroup
    {
        public int employeeGroupNumber { get; set; }
        public string self { get; set; }
    }
}
