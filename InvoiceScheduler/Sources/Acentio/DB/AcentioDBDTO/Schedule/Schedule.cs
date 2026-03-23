using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceScheduler.Acentio.DB.AcentioDBDTO.Schedule
{
    public class Schedule
    {
        public string Id { get; set; }  
        public string Sales_Order_Id { get; set; }
        public DateTime Invoice_Date { get; set; }
        public DateTime Payment_Date { get; set; }
        public DateTime Periode_Start_Date { get; set; }
        public DateTime Periode_End_Date { get; set; }
        public string Sales_Order_Version { get; }

        public string ERPSystem { get; }
    }
}
