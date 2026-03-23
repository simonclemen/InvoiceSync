using EconomicDataTransfer_Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceScheduler_Consumer
{
    internal class MasterdataERPSynchronizer
    {
        public async Task Execute()
        {
            //Setup
            var settings = new Settings("Masterdata ERP Synchronizer");
            var persisterDB = new DBPersister(settings);

            try
            {
                //Get Last Run
                var lastrunDT = persisterDB.GetLastRun();

                //Get Acentio CRM Masterdata                 
                var retrieverAndParser_Acentio = new RetrieverAndParser_AcentioCRM(lastrunDT, settings);
                var data_acentio = await retrieverAndParser_Acentio.GetMasterdata();

                //Get economic matching data
                var retrieverAndParser_Economic = new RetrieverAndParser_Economic();
                var data_economic = await retrieverAndParser_Economic.GetMasterdata(data_acentio);

                                //Get TripleTex matching data


                //Persist Data
                var persister = new MasterdataPersister(settings);
                var data = await persister.Save(data_acentio);

                //Save Stats & Creations                
                persisterDB.Save(data);
                
                if (settings.Warning.Count > 0 && settings.Error.Count > 0) persisterDB.InformOnFailure(settings.SystemId + ": There are warnings (" + settings.Warning.Count + ") & errors (" + settings.Error.Count + ")!", settings);
                else if (settings.Warning.Count > 0) persisterDB.InformOnFailure(settings.SystemId + ": There are warnings (" + settings.Warning.Count + ")!", settings);
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
