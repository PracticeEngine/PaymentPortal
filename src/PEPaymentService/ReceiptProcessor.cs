using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPaymentService
{
    public class ReceiptProcessor
    {
        private readonly SqlConnection connection;

        public ReceiptProcessor()
        {
            var connectionBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["EngineDb"].ConnectionString);
            connectionBuilder.MultipleActiveResultSets = false;
            connection = new SqlConnection(connectionBuilder.ConnectionString);
        }

        /// <summary>
        /// Processes an Incoming Payment to the firm
        /// </summary>
        /// <param name="Invoices">List of DebtTranRefAlphas</param>
        /// <param name="PaymentRef">Provider Reference Number</param>
        /// <param name="Amount">Amount of Payment</param>
        /// <param name="utcDateOfPayment">Date of Payment</param>
        /// <returns></returns>
        public Task ProcessPayment(string[] Invoices, string PaymentRef, decimal Amount, DateTime utcDateOfPayment)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes an Error Correction (reason must be provided)
        /// </summary>
        /// <param name="Invoices">List of DebtTranRefAlphas</param>
        /// <param name="PaymentRef">Provider Reference Number</param>
        /// <param name="Amount">Amount of Correction</param>
        /// <param name="utcDateOfCorrection">Date of Correction</param>
        /// <param name="CorrectionReason">Reason given for the Error Correction</param>
        /// <returns></returns>
        public Task ProcessErrorCorrection(string[] Invoices, string PaymentRef, decimal Amount, DateTime utcDateOfCorrection, string CorrectionReason)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes a Reversal of Payment
        /// </summary>
        /// <param name="Invoices">List of DebtTranRefAlphas</param>
        /// <param name="PaymentRef">Provider Reference Number</param>
        /// <param name="Amount">Amount of Reversal</param>
        /// <param name="utcDateOfReversal">Date of Reversal</param>
        /// <returns></returns>
        public Task ProcessReversal(string[] Invoices, string PaymentRef, decimal Amount, DateTime utcDateOfReversal)
        {
            throw new NotImplementedException();
        }
    }
}
