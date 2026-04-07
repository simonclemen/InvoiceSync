using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceScheduler_Consumer
{
    public static class SQL
    {
        internal static readonly string SaveRun = @"INSERT INTO [dbo].[Transfer_Runs]([System],[TransactionId],[Created])VALUES (@system,@transactionid,@dt)";
        internal static readonly string SaveLogs = @"INSERT INTO [dbo].[Transfer_Logs]([TransactionId],[Source], [Info]) VALUES (@transactionid,@source,@info)";
        internal static readonly string SaveWarnings = @"INSERT INTO [dbo].[Transfer_Warnings]([TransactionId],[Source], [Info],[EntityId]) VALUES (@transactionid,@source,@info,@entry)";
        internal static readonly string SaveErrors = @"INSERT INTO [dbo].[Transfer_Errors]([TransactionId],[Source], [Info],[EntityId]) VALUES (@transactionid,@source,@info,@entry)";
        internal static readonly string SaveSkip = @"INSERT INTO [dbo].[Transfer_Skip]([TransactionId],[Source], [Info]) VALUES (@transactionid,@source,@entry)";
        internal static readonly string SaveFullSync = @"INSERT INTO [dbo].[Transfer_FullSync]([TransactionId],[Source], [Info]) VALUES (@transactionid,@source,@entry)";
        internal static readonly string SaveUpdateIds = @"INSERT INTO [dbo].[Transfer_UpdatedRecords]([TransactionId],[Source],[EntryId]) VALUES (@transactionid,@source,@entry)";
        internal static readonly string SaveNewIds = @"INSERT INTO [dbo].[Transfer_NewRecords]([TransactionId],[Source],[EntryId]) VALUES (@transactionid,@source,@entry)";
        internal static readonly string SaveDeletedIds = @"INSERT INTO [dbo].[Transfer_DeletedRecords]([TransactionId],[Source],[EntryId]) VALUES (@transactionid,@source,@entry)";
        internal static readonly string GetSchedule = @"SELECT Sales_Order_Id,Invoice_Date,Payment_Date,Periode_Start_Date,Periode_End_Date,sales_order_version,erpsystem,scheduleid as Id FROM [ESPOCRM].[dbo].[GetPendingInvoicesForSchedule] (null)";
        internal static readonly string SaveSyncSchedule = @"INSERT INTO [ESPOCRM].[dbo].[SyncSchedule]([TransactionId],[ScheduleId],[InvoiceId],[salesOrderVersion],[Created]) VALUES (@TransactionId, @ScheduleId,@InvoiceId,@salesOrderVersion, GETDATE())";
        internal static readonly string InformOnSuccess = @"EXECUTE dbo.InformUsersOnSuccess @transactionId = @transactionId";
        internal static readonly string InformOnFailure = @"EXECUTE dbo.InformOnFailure @title = 'Invoice Creation Error!', @msg = @msg, @transactionId = @transactionId";
        internal static readonly string GetOrphanedInvoices = @"select id, ErpDraftNumber from [ESPOCRM].[dbo].[ERP_Deleted_Drafts] where Processed = 0";
        internal static readonly string ProcessOrphanedInvoice = @"Update [ESPOCRM].[dbo].[ERP_Deleted_Drafts] set processed = 1 where id = @id";



        internal static readonly string GetLastRun = @"select top 1 CONVERT(nvarchar(50), Dateadd(second, -20, DATEADD(MI, (DATEDIFF(MI, SYSDATETIME(), SYSUTCDATETIME())), r.created)), 126) from [dbo].[Transfer_Runs] r where r.[system] = @systemid and not exists(select top 1 1 From Transfer_Errors te where te.TransactionId = r.TransactionId) order by Created desc";
    }
}
