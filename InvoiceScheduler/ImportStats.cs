using System;

namespace InvoiceScheduler_Consumer
{
  
    internal class ImportStats
    {
        private Guid _id;


        public ImportStats()
        {
        }

        public Guid Id
        {
            get
            {
                if (_id == Guid.Empty) _id = Guid.NewGuid();
                return _id;
            }
        }
        public long Rows { get; internal set; }
        
        public DateTime End { get; internal set; }
        public DateTime Start { get; internal set; }
    }
}