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
            RawRecord = record;

            if (String.IsNullOrWhiteSpace(record))
                throw new ArgumentException(nameof(record), "Detail Record is Empty.");
            if (record.Length != 219)
                throw new ArgumentException(nameof(record), "Detail Record is wrong length.");

            if (record.Substring(0, 2) != "50")
                throw new ArgumentOutOfRangeException(nameof(record), "data does not indicate this is a detail record.");

            BillerCode = record.Substring(2, 10).Trim();
            ClientCode = record.Substring(12, 20).Trim();
            InstructionType = record.Substring(32, 2).Trim();
            TranReferenceNumber = record.Substring(34, 21).Trim();
            InvoiceNumber = record.Substring(55, 21).Trim();
            ErrorCorrectionReason = record.Substring(76, 3).Trim();
            Amount = (decimal)long.Parse(record.Substring(79, 12).Trim()) / 100M;
            UTCDateOfPayment = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(record.Substring(91,14).Trim(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture), TimeZoneInfo.FindSystemTimeZoneById(AEST_TIMEZONE_ID));
            BPaySettlementDate = DateTime.ParseExact(record.Substring(105, 8).Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);
            PaymentReference = record.Substring(113, 12).Trim().TrimStart('0');
        }

        public string RawRecord { get; set; }

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

        public string ErrorCorrectionDescription {
            get
            {
                switch(ErrorCorrectionReason)
                {
                    case "000":
                        return String.Empty;
                    case "001":
                        return "Payer paid twice (or more)";
                    case "002":
                        return "Payer paid wrong account ";
                    case "003":
                        return "Payer paid wrong Biller";
                    case "004":
                        return "Payer paid wrong amount";
                    case "005":
                        return "Payer did not authorise payment";
                    case "400":
                        return "Valid Visa chargeback reason code";
                    case "500":
                        return "Valid MasterCard chargeback reason code";
                    case "600":
                        return "Valid Bankcard chargeback reason code";
                    default:
                        return $"Unknown Reason Code '{ErrorCorrectionReason}' - Check with Quickfee";
                }
            }
        }

        public decimal Amount { get; private set; }

        public DateTime UTCDateOfPayment { get; private set; }

        public DateTime BPaySettlementDate { get; private set; }

        public string PaymentReference { get; set; }
    }
}
