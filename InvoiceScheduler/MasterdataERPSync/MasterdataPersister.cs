using InvoiceScheduler.Acentio.DB.AcentioDBDTO.InvoiceRecord;
using InvoiceScheduler_Consumer_AcentioCRM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceScheduler_Consumer
{
    internal class MasterdataPersister
    {
        private Settings settings;

        public MasterdataPersister(Settings settings)
        {
            this.settings = settings;
        }

        internal async Task<IList<InvoiceRecord>> Save(CombinedDataSet data_acentio)
        {
            throw new NotImplementedException();
        }
    }
}