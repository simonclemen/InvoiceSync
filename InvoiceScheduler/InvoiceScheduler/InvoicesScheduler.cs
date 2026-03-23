using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceScheduler_Consumer
{
    internal class InvoicesScheduler
    {
        public async Task Execute()
        {
            //Setup
            var settings = new Settings("Scheduled Invoice Service");
            var persisterDB = new DBPersister(settings);

            try
            {
                //Get Scheduled Invoices
                var retrieverAndParser_Acentio_DB = new RetrieverAndParser_AcentioDB();
                var data_acentio_db = retrieverAndParser_Acentio_DB.Get();

                //Get Acentio CRM Data
                var retrieverAndParser_Acentio = new RetrieverAndParser_AcentioCRM(null, settings);
                var data_acentio = await retrieverAndParser_Acentio.GetSalesOrders(data_acentio_db);

                //Persist Data
                var persister = new SchedulerPersister(settings);
                var data = await persister.Save(data_acentio, data_acentio_db);

                //Save Stats & Creations                
                persisterDB.Save(data);
                if (data.Count > 0) persisterDB.InformOnSuccess(settings);

                if (settings.Warning.Count > 0 && settings.Error.Count > 0) persisterDB.InformOnFailure(settings.SystemId+": There are warnings (" + settings.Warning.Count + ") & errors (" + settings.Error.Count + ")!", settings);
                else if (settings.Warning.Count>0) persisterDB.InformOnFailure(settings.SystemId+": There are warnings (" + settings.Warning.Count + ")!", settings);
                else if (settings.Error.Count > 0) persisterDB.InformOnFailure(settings.SystemId + ": There are errors (" + settings.Error.Count + ")!", settings);

            }
            catch (Exception ex)
            {
                persisterDB.InformOnFailure(ex.Message, settings);
                throw;
            }
        }
    }
}
