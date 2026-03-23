using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceScheduler.Acentio.DB.AcentioDBDTO.InvoiceRecord
{
    internal class InvoiceRecord
    {
        public InvoiceRecord(string scheduleId, string invoiceId , string salesOrderVersion) { this.ScheduleId = scheduleId; this.InvoiceId = invoiceId; this.SalesOrderVersion = salesOrderVersion; }
        public string ScheduleId { get; }
        public string InvoiceId { get; }

        public string SalesOrderVersion { get; }
        
    }
}
