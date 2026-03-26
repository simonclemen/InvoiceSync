using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace InvoiceScheduler_Consumer
{
    public class Consumer
    {
        public async Task Execute()
        {
            //Sync masterdata to ERP
     //       var masterdata = new MasterdataERPSynchronizer();
       //      await masterdata.Execute();

            //Create All Scheduled Invoices
            var invoicescheduler = new InvoicesScheduler();
            await invoicescheduler.Execute();

            //Synchronize all Invoices to ERP
            var invoiceERPsynchronizer = new InvoiceToERPSynchronizer();
            await invoiceERPsynchronizer.Execute();

            //Delete all orphaned ERP invoices
            var invoiceERPDeletion = new InvoiceERPDeletion();
            await invoiceERPDeletion.Execute();
        }
    }  
}
