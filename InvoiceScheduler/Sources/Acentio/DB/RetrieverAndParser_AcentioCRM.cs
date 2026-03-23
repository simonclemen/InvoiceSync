using Dapper;
using InvoiceScheduler.Acentio.DB;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Dapper.SqlMapper;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.DraftNumber;
using InvoiceScheduler.Acentio.DB.AcentioDBDTO.Schedule;
using System.Collections;
namespace InvoiceScheduler_Consumer
{
    public class RetrieverAndParser_AcentioDB
    {
        public CombinedDataSet Get()
        {
            using (var context = DB.ConnectionFactory())
            {
                context.Open();
                try
                {
                    var parms = new DynamicParameters();
                    var schedules = context.Context.Query<Schedule>(sql: SQL.GetSchedule, param: parms, transaction: context._currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: context.Timeout);

                    var data = new CombinedDataSet(schedules);
                    return data;

                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    context.Close();
                }
            }
        }

        internal CombinedDataSet GetOrphanedInvoices()
        {
            using (var context = DB.ConnectionFactory())
            {
                context.Open();
                try
                {
                    var parms = new DynamicParameters();
                    var draftnumbers = context.Context.Query<DraftNumber>(sql: SQL.GetOrphanedInvoices, param: parms, transaction: context._currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: context.Timeout);

                    var data = new CombinedDataSet();
                    data.DraftNumbers = draftnumbers;
                    return data;

                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    context.Close();
                }
            }
        }
        internal void ProcessOrphanedInvoice(IEnumerable<DraftNumber> list)
        {
            using (var context = DB.ConnectionFactory())
            {
                context.Open();
                try                   
                {
                    foreach (var item in list)
                    {
                        var parms = new DynamicParameters();
                        parms.Add(name: "@id", value: item.Id, direction: System.Data.ParameterDirection.Input);
                        context.Context.Execute(sql: SQL.ProcessOrphanedInvoice, param: parms, transaction: context._currenttrans, commandType: System.Data.CommandType.Text, commandTimeout: context.Timeout);

                    }

                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    context.Close();
                }
            }
        }
        
    }
}
