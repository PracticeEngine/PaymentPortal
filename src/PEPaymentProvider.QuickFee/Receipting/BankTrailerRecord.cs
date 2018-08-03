using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPaymentProvider.Receipting
{
    public class BankTrailerRecord
    {
        public BankTrailerRecord(string record)
        {
            if (String.IsNullOrWhiteSpace(record))
                throw new ArgumentException(nameof(record), "Trailer Record is Empty.");
            if (record.Length != 219)
                throw new ArgumentException(nameof(record), "Trailer Record is wrong length.");

            if (record.Substring(0, 2) != "99")
                throw new ArgumentOutOfRangeException(nameof(record), "data does not indicate this is a trailer record.");

            BillerCode = record.Substring(2, 10);
            NumPayments = int.Parse(record.Substring(12, 9));
            TotalPayments = (decimal)int.Parse(record.Substring(21,15)) / 100M;
            NumErrorCorrections = int.Parse(record.Substring(36, 9));
            TotalErrorCorrections = (decimal)int.Parse(record.Substring(45, 15)) / 100M;
            NumReversals = int.Parse(record.Substring(60, 9));
            TotalReversals = (decimal)int.Parse(record.Substring(69, 15)) / 100M;
            SettlementAmount = (decimal)int.Parse(record.Substring(84, 15)) / 100M;
        }

        public string BillerCode { get; private set; }

        public int NumPayments { get; private set; }

        public decimal TotalPayments { get; private set; }

        public int NumErrorCorrections { get; private set; }

        public decimal TotalErrorCorrections { get; private set; }

        public int NumReversals { get; private set; }

        public decimal TotalReversals { get; private set; }

        public decimal SettlementAmount { get; private set; }
    }
}
