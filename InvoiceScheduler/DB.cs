using Dapper;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.InvoiceRecord;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace InvoiceScheduler_Consumer
{
    public static class DB
    {
        public static Func<DataAccess> ConnectionFactory = () => new DataAccess(new SqlConnection(ConnectionString.Connection));

    }
    public static class ConnectionString
    {
        public static string Connection = ConfigurationManager.AppSettings["connectionString"];
    }
    public class DataAccess : IDisposable
    {
        private int _timeout = 60 * 10;

        public int Timeout { get { return _timeout; } }
        public DbConnection Context { get; private set; }
        public DbTransaction _currenttrans { get; private set; }

        public DataAccess(DbConnection context) { this.Context = context; }

        public void Dispose()
        {
            if (Context != null) Context.Dispose();
        }
        public DbTransaction BeginTransaction()
        {
            return BeginTransaction(System.Data.IsolationLevel.Snapshot);
        }
        public DbTransaction BeginTransaction(System.Data.IsolationLevel level)
        {
            _currenttrans = Context.BeginTransaction(level);
            return _currenttrans;
        }




        public void Open() { Context.Open(); }
        public void Close() { try { Context.Close(); } catch (Exception) { } }

        internal void Save(InvoiceScheduler_Consumer.Settings settings)
        {
            try
            {
                var parms = new DynamicParameters();
                parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);
                parms.Add(name: "@system", value: settings.SystemId, direction: System.Data.ParameterDirection.Input);
                parms.Add(name: "@dt", value: settings.Start, direction: System.Data.ParameterDirection.Input);
                Context.Execute(sql: SQL.SaveRun, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);

                foreach (var data in settings.Log)
                {
                    parms = new DynamicParameters();
                    parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);

                    parms.Add(name: "@source", value: data.Item1, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@info", value: data.Item2, direction: System.Data.ParameterDirection.Input);
                    Context.Execute(sql: SQL.SaveLogs, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
                }

                foreach (var data in settings.Warning)
                {
                    parms = new DynamicParameters();
                    parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);

                    parms.Add(name: "@source", value: data.Item1, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@info", value: data.Item2.Info, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@entry", value: data.Item2.EntityId, direction: System.Data.ParameterDirection.Input);
                    Context.Execute(sql: SQL.SaveWarnings, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
                }


                foreach (var data in settings.Error)
                {
                    parms = new DynamicParameters();
                    parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);

                    parms.Add(name: "@source", value: data.Item1, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@info", value: data.Item2.Info, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@entry", value: data.Item2.EntityId, direction: System.Data.ParameterDirection.Input);
                    Context.Execute(sql: SQL.SaveErrors, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
                }
                foreach (var data in settings.FullSync)
                {
                    parms = new DynamicParameters();
                    parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);

                    parms.Add(name: "@source", value: data.Item1, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@entry", value: data.Item2, direction: System.Data.ParameterDirection.Input);
                    Context.Execute(sql: SQL.SaveFullSync, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
                }

                foreach (var data in settings.Skip)
                {
                    parms = new DynamicParameters();
                    parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);

                    parms.Add(name: "@source", value: data.Item1, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@entry", value: data.Item2, direction: System.Data.ParameterDirection.Input);
                    Context.Execute(sql: SQL.SaveSkip, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
                }

                foreach (var data in settings.UpdatedIds)
                {
                    parms = new DynamicParameters();
                    parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);

                    parms.Add(name: "@source", value: data.Item1, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@entry", value: data.Item2.EntityId, direction: System.Data.ParameterDirection.Input);
                    Context.Execute(sql: SQL.SaveUpdateIds, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
                }
                foreach (var data in settings.DeletedIds)
                {
                    parms = new DynamicParameters();
                    parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);

                    parms.Add(name: "@source", value: data.Item1, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@entry", value: data.Item2.EntityId, direction: System.Data.ParameterDirection.Input);
                    Context.Execute(sql: SQL.SaveDeletedIds, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
                }

                foreach (var data in settings.NewIds)
                {
                    parms = new DynamicParameters();
                    parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);

                    parms.Add(name: "@source", value: data.Item1, direction: System.Data.ParameterDirection.Input);
                    parms.Add(name: "@entry", value: data.Item2.EntityId, direction: System.Data.ParameterDirection.Input);
                    Context.Execute(sql: SQL.SaveNewIds, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
                }
            }
            catch (Exception)
            {


            }

        }

        internal void Save(IList<InvoiceRecord> data, InvoiceScheduler_Consumer.Settings settings)
        {
            foreach (var item in data)
            {
                var parms = new DynamicParameters();
                parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);
                parms.Add(name: "@scheduleId", value: item.ScheduleId, direction: System.Data.ParameterDirection.Input);
                parms.Add(name: "@invoiceId", value: item.InvoiceId, direction: System.Data.ParameterDirection.Input);
                parms.Add(name: "@salesOrderVersion", value: item.SalesOrderVersion, direction: System.Data.ParameterDirection.Input);

                Context.Execute(sql: SQL.SaveSyncSchedule, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);

            }
        }

        internal void InformOnSuccess(Settings settings)
        {
            var parms = new DynamicParameters();
            parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);
            Context.Execute(sql: SQL.InformOnSuccess, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
        }

        internal void InformOnFailure(string message, Settings settings)
        {
            var parms = new DynamicParameters();
            parms.Add(name: "@transactionId", value: settings.Transaction, direction: System.Data.ParameterDirection.Input);
            parms.Add(name: "@msg", value: (message??""), direction: System.Data.ParameterDirection.Input);
            Context.Execute(sql: SQL.InformOnFailure, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
        }

        internal string GetLastRun(string systemId)
        {
            var parms = new DynamicParameters();
            parms.Add(name: "@systemId", value: systemId, direction: System.Data.ParameterDirection.Input);           

            return Context.QueryFirstOrDefault<string>(sql: SQL.GetLastRun, param: parms, transaction: _currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: _timeout);
        }
    }
}