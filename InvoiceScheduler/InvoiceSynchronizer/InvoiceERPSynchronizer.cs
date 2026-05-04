using EconomicDataTransfer_Consumer;
using TripleTexDataTransfer_Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceScheduler_Consumer
{
    internal class InvoiceToERPSynchronizer
    {
        public async Task Execute()
        {
            //Setup
            var settings = new Settings("Invoices To ERP");
            var persisterDB = new DBPersister(settings);

            try
            {
                //Get Last Run
                var lastrunDT = persisterDB.GetLastRun();                

                //Get Acentio CRM Data
                var retrieverAndParser_Acentio = new RetrieverAndParser_AcentioCRM(lastrunDT, settings);
                var data_acentio = await retrieverAndParser_Acentio.GetData();

                //Get live data

                if (data_acentio.Invoices.list.Count > 0)
                {
                    //Get Economic Data                
                    var retrieverAndParser_Economic = new RetrieverAndParser_Economic();
                    var data_economic = await retrieverAndParser_Economic.GetData(data_acentio);

                    //Get TripleTex Data
                    var retrieverAndParser_TripleTex = new RetrieverAndParser_TripleTex();
                    var data_tripletex = await retrieverAndParser_TripleTex.GetData(data_acentio);

                    //Persist Data                
                    var economic_persister = new Economic_SynchPersister(settings);
                    await economic_persister.SaveEconomicData(data_acentio, data_economic);

                    var tripletex_persister = new Tripletex_SynchPersister(settings);
                    await tripletex_persister.SaveTripleTexData(data_acentio, data_tripletex);
                }

                //Save Stats & Creations                
                persisterDB.Save();

                var wmsg = "";
                foreach (var w in settings.Warning)
                {
                    wmsg += w.Item1 + ", " + w.Item2 + "<br/>";
                }
                var emsg = "";
                foreach (var e in settings.Error)
                {
                    emsg += e.Item1 + ", " + e.Item2 + "<br/>";
                }

                if (settings.Warning.Count > 0 && settings.Error.Count > 0) persisterDB.InformOnFailure(settings.SystemId + ": There are warnings (" + settings.Warning.Count + ") & errors (" + settings.Error.Count + ")!<br/>" + wmsg + "<br/>" + emsg, settings);
                else if (settings.Warning.Count > 0) persisterDB.InformOnFailure(settings.SystemId + ": There are warnings (" + settings.Warning.Count + ")!<br/>" + wmsg, settings);
                else if (settings.Error.Count > 0) persisterDB.InformOnFailure(settings.SystemId + ": There are errors (" + settings.Error.Count + ")!<br/>" + emsg, settings);

            }
            catch (Exception ex)
            {
                persisterDB.InformOnFailure(ex.Message, settings);
                throw;
            }
        }
    
    }
}
