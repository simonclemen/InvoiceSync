using EconomicDataTransfer_Consumer;
using System;
using System.Threading.Tasks;

namespace InvoiceScheduler_Consumer
{
    internal class InvoiceERPDeletion
    {
        public InvoiceERPDeletion()
        {
        }

        internal async Task Execute()
        {
            try
            {
                //Candidates
                var retrieverAndParser_Acentio_DB = new RetrieverAndParser_AcentioDB();
                var data_acentio_db = retrieverAndParser_Acentio_DB.GetOrphanedInvoices();

                //Attempt Delete
                var retrieverAndParser_Economic = new RetrieverAndParser_Economic();
                await retrieverAndParser_Economic.DeleteDrafts(data_acentio_db);

                //Update queue
                retrieverAndParser_Acentio_DB.ProcessOrphanedInvoice(data_acentio_db.DraftNumbers);
            }
            catch (Exception ex) { }


        }
    }
}