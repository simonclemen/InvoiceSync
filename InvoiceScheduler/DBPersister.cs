using InvoiceScheduler.Acentio.DB.AcentioDBDTO.InvoiceRecord;
using System;
using System.Collections.Generic;
using static Dapper.SqlMapper;
namespace InvoiceScheduler_Consumer
{
    internal class DBPersister
    {
        private Settings Settings { get; set; }

        public DBPersister(Settings settings)
        {
            this.Settings = settings;
        }
        internal void Save(IList<InvoiceRecord> data)
        {
            using (var context = DB.ConnectionFactory())
            {
                context.Open();
                var transaction = context.BeginTransaction();

                context.Save(Settings);
                context.Save(data, Settings);                
                transaction.Commit();

            }
        }
        internal void Save()
        {
            using (var context = DB.ConnectionFactory())
            {
                context.Open();
                var transaction = context.BeginTransaction();

                context.Save(Settings);
                transaction.Commit();
            }
        }
        internal void InformOnSuccess(Settings settings)
        {
            using (var context = DB.ConnectionFactory())
            {
                try
                {
                    context.Open();
                    var transaction = context.BeginTransaction();

                    context.InformOnSuccess(settings);
                    transaction.Commit();
                }
                finally
                {
                    context.Close();
                }

            }
            
        }

        internal void InformOnFailure(string message, Settings settings)
        {
            using (var context = DB.ConnectionFactory())
            {
                try
                {
                    context.Open();
                    var transaction = context.BeginTransaction();

                    context.InformOnFailure(message, settings);
                    transaction.Commit();
                }
                finally
                {
                    context.Close();
                }

            }
        }

        internal string GetLastRun()
        {
            using (var context = DB.ConnectionFactory())
            {
                try
                {
                    context.Open();
                    return context.GetLastRun(Settings.SystemId);
                   
                }
                finally
                {
                    context.Close();
                }

            }
        }
    }
}