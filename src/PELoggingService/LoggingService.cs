using Serilog;
using Serilog.Sinks.Email;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        public LoggingService()
        {
            var emailConnectionInfo = new EmailConnectionInfo
            {
                FromEmail = ConfigurationManager.AppSettings["ReceiptLogFrom"],
                ToEmail = ConfigurationManager.AppSettings["ReceiptLogTo"],
                MailServer = ConfigurationManager.AppSettings["ReceiptLogMailServer"],
                EmailSubject = "Receipt Processing Log {timestamp}",
                Port = int.Parse(ConfigurationManager.AppSettings["ReceiptLogPort"]),
                EnableSsl = bool.Parse(ConfigurationManager.AppSettings["ReceiptLogSsl"])
            };

            if (!String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ReceiptLogUser"]))
            {
                if (!String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ReceiptLogDomain"]))
                {
                    emailConnectionInfo.NetworkCredentials = new NetworkCredential(
                        ConfigurationManager.AppSettings["ReceiptLogUser"], 
                        ConfigurationManager.AppSettings["ReceiptLogPassword"], 
                        ConfigurationManager.AppSettings["ReceiptLogDomain"]);
                }
                else
                {
                    emailConnectionInfo.NetworkCredentials = new NetworkCredential(
                        ConfigurationManager.AppSettings["ReceiptLogUser"], 
                        ConfigurationManager.AppSettings["ReceiptLogPassword"]);
                }
            }

            Logger = new LoggerConfiguration()
                .WriteTo.Email(emailConnectionInfo)
                .CreateLogger();
        }
    }
}
