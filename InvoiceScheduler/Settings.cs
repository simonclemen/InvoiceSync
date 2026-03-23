
using System;
using System.Collections.Generic;
using System.Linq;
namespace InvoiceScheduler_Consumer
{
    public class Settings
    {
        public string Transaction { get; }
        public DataSetting Customers { get; set; }

        public string SystemId { get; set; }

        public DataSetting Invoices { get; set; }
        public DataSetting CreditNotes { get; set; }
        public DataSetting PersistEconomicInvoices { get; set; }
        public DataSetting PersistTripleTexInvoices { get; set; }
        


        public class EntityData
        {
            public EntityData(string entityId)
            {
                EntityId = entityId;
            }
            public EntityData(string entityId, string info)
            {
                EntityId = entityId;
                Info = info;
            }
            public string Info { get; set; }
            public string EntityId { get; set; }
        }
        public class DataSetting
        {
            public bool Skip { get; set; }
            public bool FullSync { get; set; }
            public IList<EntityData> NewIds { get; set; }

            public IList<EntityData> UpdatedIds { get; set; }
            public IList<EntityData> DeletedIds { get; set; }

            public IList<string> Log { get; set; }

            public IList<EntityData> Warning { get; set; }

            public IList<EntityData> Error { get; set; }

            public DataSetting()
            {

                this.FullSync = false;
                this.NewIds = new List<EntityData>();
                this.UpdatedIds = new List<EntityData>();
                this.DeletedIds = new List<EntityData>();
                this.Log = new List<string>();
                this.Warning = new List<EntityData>();
                this.Error = new List<EntityData>();
            }

        }        
        public Settings(string system)
        {
            this.Transaction = System.Guid.NewGuid().ToString();
            this.Customers = new DataSetting();          
            this.Invoices = new DataSetting();
            this.CreditNotes = new DataSetting();
            this.PersistEconomicInvoices = new DataSetting();
            this.PersistTripleTexInvoices = new DataSetting();
            
            this.SystemId = system;
            this.Start = DateTime.Now;

        }
        public IList<Tuple<string, string>> FullSync
        {
            get
            {
                var list = new List<Tuple<string, string>>();
                list.Add(new Tuple<string, string>("invoices", Invoices.FullSync.ToString()));
                list.Add(new Tuple<string, string>("creditnotes", CreditNotes.FullSync.ToString()));
                list.Add(new Tuple<string, string>("persisteconomicinvoices", PersistEconomicInvoices.FullSync.ToString()));
                list.Add(new Tuple<string, string>("persisttripletexinvoices", PersistTripleTexInvoices.FullSync.ToString()));

                return list;

            }
        }
        public IList<Tuple<string, string>> Skip
        {
            get
            {
                var list = new List<Tuple<string, string>>();
                list.Add(new Tuple<string, string>("invoices", Invoices.Skip.ToString()));
                list.Add(new Tuple<string, string>("creditnotes", CreditNotes.Skip.ToString()));
                list.Add(new Tuple<string, string>("persisteconomicinvoices", PersistEconomicInvoices.Skip.ToString()));
                list.Add(new Tuple<string, string>("persisttripletexinvoices", PersistTripleTexInvoices.Skip.ToString()));
                return list;

            }
        }
        public IList<Tuple<string, EntityData>> NewIds
        {
            get
            {
                return Invoices.NewIds.Select(r => new Tuple<string, EntityData>("invoices", r))
                    .Union(CreditNotes.NewIds.Select(r => new Tuple<string, EntityData>("creditnotes", r)))
                    .Union(PersistEconomicInvoices.NewIds.Select(r => new Tuple<string, EntityData>("persisteconomicinvoices", r)))
                    .Union(PersistTripleTexInvoices.NewIds.Select(r => new Tuple<string, EntityData>("persisttripletexinvoices", r)))

                    

                .ToList();
            }
        }


        public IList<Tuple<string, EntityData>> DeletedIds
        {
            get
            {
                return Invoices.DeletedIds.Select(r => new Tuple<string, EntityData>("invoices", r))
                    .Union(CreditNotes.DeletedIds.Select(r => new Tuple<string, EntityData>("creditnotes", r)))
                    .Union(PersistEconomicInvoices.DeletedIds.Select(r => new Tuple<string, EntityData>("persisteconomicinvoices", r)))
                    .Union(PersistTripleTexInvoices.DeletedIds.Select(r => new Tuple<string, EntityData>("persisttripletexinvoices", r)))
               .ToList();
            }
        }


        public IList<Tuple<string, EntityData>> UpdatedIds
        {
            get
            {
                return Invoices.UpdatedIds.Select(r => new Tuple<string, EntityData>("invoices", r))
                    .Union(CreditNotes.UpdatedIds.Select(r => new Tuple<string, EntityData>("creditnotes", r)))
                    .Union(PersistEconomicInvoices.UpdatedIds.Select(r => new Tuple<string, EntityData>("persisteconomicinvoices", r)))
                    .Union(PersistTripleTexInvoices.UpdatedIds.Select(r => new Tuple<string, EntityData>("persisttripletexinvoices", r)))
              .ToList();
            }
        }

        public IList<Tuple<string, string>> Log
        {
            get
            {
                return Invoices.Log.Select(r => new Tuple<string, string>("invoices", r))
                    .Union(CreditNotes.Log.Select(r => new Tuple<string, string>("creditnotes", r)))
                    .Union(PersistEconomicInvoices.Log.Select(r => new Tuple<string, string>("persisteconomicinvoices", r)))
                    .Union(PersistTripleTexInvoices.Log.Select(r => new Tuple<string, string>("persisttripletexinvoices", r)))
                    
             .ToList();
            }
        }

        public IList<Tuple<string, EntityData>> Warning
        {
            get
            {
                return Invoices.Warning.Select(r => new Tuple<string, EntityData>("invoices", r))
                    .Union(CreditNotes.Warning.Select(r => new Tuple<string, EntityData>("creditnotes", r)))
                    .Union(PersistEconomicInvoices.Warning.Select(r => new Tuple<string, EntityData>("persisteconomicinvoices", r)))
                    .Union(PersistTripleTexInvoices.Warning.Select(r => new Tuple<string, EntityData>("persisttripletexinvoices", r)))
             .ToList();
            }
        }

        public IList<Tuple<string, EntityData>> Error
        {
            get
            {
                return Invoices.Error.Select(r => new Tuple<string, EntityData>("invoices", r))
                    .Union(CreditNotes.Error.Select(r => new Tuple<string, EntityData>("creditnotes", r)))
                    .Union(PersistEconomicInvoices.Error.Select(r => new Tuple<string, EntityData>("persisteconomicinvoices", r)))
                    .Union(PersistTripleTexInvoices.Error.Select(r => new Tuple<string, EntityData>("persisttripletexinvoices", r)))
           .ToList();
            }
        }

        public DateTime Start { get; internal set; }
    }
}