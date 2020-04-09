//TODO - Logging Service

//using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure.Storage;
using Serilog;
using Serilog.Sinks.Email;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PELoggingService
{
    public class LoggingService
    {

        /// <summary>
        /// The Logger to write to
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Creates a Logging Service Configured Per Configuration Settings
        /// </summary>
        //public LoggingService()
        //{
        //    //var emailConnectionInfo = new EmailConnectionInfo
        //    //{
        //    //    FromEmail = ConfigurationManager.AppSettings["ReceiptLogFrom"],
        //    //    ToEmail = ConfigurationManager.AppSettings["ReceiptLogTo"],
        //    //    MailServer = ConfigurationManager.AppSettings["ReceiptLogMailServer"],
        //    //    EmailSubject = "Receipt Processing Log {timestamp}",
        //    //    Port = int.Parse(ConfigurationManager.AppSettings["ReceiptLogPort"]),
        //    //    EnableSsl = bool.Parse(ConfigurationManager.AppSettings["ReceiptLogSsl"])
        //    //};

        //    //if (!String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ReceiptLogUser"]))
        //    //{
        //    //    if (!String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ReceiptLogDomain"]))
        //    //    {
        //    //        emailConnectionInfo.NetworkCredentials = new NetworkCredential(
        //    //            ConfigurationManager.AppSettings["ReceiptLogUser"], 
        //    //            ConfigurationManager.AppSettings["ReceiptLogPassword"], 
        //    //            ConfigurationManager.AppSettings["ReceiptLogDomain"]);
        //    //    }
        //    //    else
        //    //    {
        //    //        emailConnectionInfo.NetworkCredentials = new NetworkCredential(
        //    //            ConfigurationManager.AppSettings["ReceiptLogUser"], 
        //    //            ConfigurationManager.AppSettings["ReceiptLogPassword"]);
        //    //    }
        //    //}

        //    //Logger = new LoggerConfiguration()
        //    //    .WriteTo.Email(emailConnectionInfo)
        //    //    .CreateLogger();

        //    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["CloudStorageAccount"]);

        //    Logger = new LoggerConfiguration()
        //        .WriteTo.AzureBlobStorage(null, storageAccount, Serilog.Events.LogEventLevel.Information, "paymentportallogs")
        //        .CreateLogger();
        //}

        public int BankFileLog(Nullable<int> logId, string bankFile, string errorMessage, bool processed)
        {
            int newLogId;

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["EngineDb"].ConnectionString))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PP_Upsert_Bank_File_Log_Entry";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@LogId", logId);
                    cmd.Parameters.AddWithValue("@BankFile", bankFile);
                    cmd.Parameters.AddWithValue("@ErrorMessage", errorMessage);
                    cmd.Parameters.AddWithValue("@Processed", processed);

                    SqlParameter outputNewLogIdParam = new SqlParameter("@NewLogId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(outputNewLogIdParam);

                    cmd.Connection.Open();

                    cmd.ExecuteNonQuery();

                    newLogId = (int)outputNewLogIdParam.Value;

                    cmd.Connection.Close();
                }

                connection.Close();
            }

            return newLogId;
        }

        public int ReceiptProcessorLog(Nullable<int> logId, Nullable<int> bankFileLogId, string record, string errorMessage, bool processed)
        {
            int newLogId;

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["EngineDb"].ConnectionString))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PP_Upsert_Receipt_Processor_Log_Entry";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@LogId", logId);
                    cmd.Parameters.AddWithValue("@BankFileLogId", bankFileLogId);
                    cmd.Parameters.AddWithValue("@Record", record);
                    cmd.Parameters.AddWithValue("@ErrorMessage", errorMessage);
                    cmd.Parameters.AddWithValue("@Processed", processed);

                    SqlParameter outputNewLogIdParam = new SqlParameter("@NewLogId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(outputNewLogIdParam);

                    cmd.Connection.Open();

                    cmd.ExecuteNonQuery();

                    newLogId = (int)outputNewLogIdParam.Value;

                    cmd.Connection.Close();
                }

                connection.Close();
            }

            return newLogId;
        }
    }
}
