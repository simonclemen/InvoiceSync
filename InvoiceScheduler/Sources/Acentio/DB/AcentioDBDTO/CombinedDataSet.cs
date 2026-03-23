using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.Schedule;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.DraftNumber;
namespace InvoiceScheduler.Acentio.DB
{
    public  class CombinedDataSet
    {
        public CombinedDataSet()
        {
        }

        public CombinedDataSet(IEnumerable<Schedule> schedules)
        {
            Schedules = schedules;
        }

        public IEnumerable<Schedule> Schedules { get; set; }
        public IEnumerable<DraftNumber> DraftNumbers { get;  set; }
    }
}
