using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPaymentProvider.Receipting
{
    public class BankDetailRecord
    {
        const string AEST_TIMEZONE_ID = "AUS Eastern Standard Time";

        public BankDetailRecord(string record)
        {
            if (String.IsNullOrWhiteSpace(record))
                throw new ArgumentException(nameof(record), "Detail Record is Empty.");
            if (record.Length != 219)
                throw new ArgumentException(nameof(record), "Detail Record is wrong length.");

            if (record.Substring(0, 2) != "50")
                throw new ArgumentOutOfRangeException(nameof(record), "data does not indicate this is a detail record.");

            BillerCode = record.Substring(2, 10);
            ClientCode = record.Substring(12, 20);
            InstructionType = record.Substring(32, 2);
            TranReferenceNumber = record.Substring(34, 21);
            InvoiceNumber = record.Substring(55, 21);
            ErrorCorrectionReason = record.Substring(76, 3);
            Amount = (decimal)int.Parse(record.Substring(79, 12)) / 100M;
            UTCDateOfPayment = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(record.Substring(91,14), "yyyyMMddHHmmss", CultureInfo.InvariantCulture), TimeZoneInfo.FindSystemTimeZoneById(AEST_TIMEZONE_ID));
            BPaySettlementDate = DateTime.ParseExact(record.Substring(105, 8), "yyyyMMdd", CultureInfo.InvariantCulture);

        }

        public string BillerCode { get; private set; }

        public string ClientCode { get; private set; }

        /// <summary>
        /// 05 = Payment
        /// 15 = Error Correction(credit amount) 
        /// 25 = Reversal
        /// </summary>
        public string InstructionType { get; private set; }

        public string TranReferenceNumber { get; private set; }

        public string InvoiceNumber { get; private set; }

        /// <summary>
        /// 000 = Not an Error Correction 
        /// 001 = Payer paid twice (or more) 
        /// 002 = Payer paid wrong account 
        /// 003 = Payer paid wrong Biller 
        /// 004 = Payer paid wrong amount
        /// 005 = Payer did not authorise payment
        /// 400 = Valid Visa chargeback reason code(not covered by BPay reason codes 001 – 005 above)
        /// 500 = Valid MasterCard chargeback reason code(not covered by BPay reason codes 001 – 005 above)
        /// 600 = Valid Bankcard chargeback reason code(not covered by BPay reason codes 001 – 005 above)
        /// </summary>
        public string ErrorCorrectionReason { get; private set; }

        public decimal Amount { get; private set; }

        public DateTime UTCDateOfPayment { get; private set; }

        public DateTime BPaySettlementDate { get; private set; }
    }
}
