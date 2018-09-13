using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
        public async Task ProcessPaymentAsync(string[] Invoices, string PaymentRef, decimal Amount, DateTime utcDateOfPayment)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PP_Add_Payments";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.Add("@Invoices", SqlDbType.Structured).Value = ToStringListTypeData(Invoices);
                cmd.Parameters.AddWithValue("@PaymentRef", PaymentRef);
                cmd.Parameters.AddWithValue("@Amount", Amount);
                cmd.Parameters.AddWithValue("@PaymentDate", utcDateOfPayment.Date);

                await cmd.Connection.OpenAsync();
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
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
        public async Task ProcessErrorCorrectionAsync(string[] Invoices, string PaymentRef, decimal Amount, DateTime utcDateOfCorrection, string CorrectionReason)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PP_Add_Corrections";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.Add("@Invoices", SqlDbType.Structured).Value = ToStringListTypeData(Invoices);
                cmd.Parameters.AddWithValue("@PaymentRef", PaymentRef);
                cmd.Parameters.AddWithValue("@Amount", Amount);
                cmd.Parameters.AddWithValue("@PaymentDate", utcDateOfCorrection.Date);

                await cmd.Connection.OpenAsync();
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Processes a Reversal of Payment
        /// </summary>
        /// <param name="Invoices">List of DebtTranRefAlphas</param>
        /// <param name="PaymentRef">Provider Reference Number</param>
        /// <param name="Amount">Amount of Reversal</param>
        /// <param name="utcDateOfReversal">Date of Reversal</param>
        /// <returns></returns>
        public async Task ProcessReversalAsync(string[] Invoices, string PaymentRef, decimal Amount, DateTime utcDateOfReversal)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PP_Add_Reversals";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.Add("@Invoices", SqlDbType.Structured).Value = ToStringListTypeData(Invoices);
                cmd.Parameters.AddWithValue("@PaymentRef", PaymentRef);
                cmd.Parameters.AddWithValue("@Amount", Amount);
                cmd.Parameters.AddWithValue("@PaymentDate", utcDateOfReversal.Date);

                await cmd.Connection.OpenAsync();
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Returns Data as Records for StringListType
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private IEnumerable<SqlDataRecord> ToStringListTypeData(string[] data)
        {
            var stringListType = new SqlMetaData("Name", System.Data.SqlDbType.NVarChar, 256);

            foreach(var value in data)
            {
                var record = new SqlDataRecord(stringListType);
                record.SetString(0, value);
                yield return record;
            }
        }
    }
}
