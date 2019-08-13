using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPaymentProvider.Receipting
{
    public class BankHeaderRecord
    {
        public BankHeaderRecord(string record)
        {
            if (String.IsNullOrWhiteSpace(record))
                throw new ArgumentException(nameof(record), "Header Record is Empty.");
            if (record.Length != 219)
                throw new ArgumentException(nameof(record), "Header Record is wrong length.");

            if (record.Substring(0, 2) != "00")
                throw new ArgumentOutOfRangeException(nameof(record), "data does not indicate this is a header record.");

            BillerCode = record.Substring(2, 10);
            BrokerName = record.Substring(12, 20);
            BrokerBSB = record.Substring(32, 6);
            BrokerAccount = record.Substring(38, 9);
            FileCreated = DateTime.ParseExact(record.Substring(47, 14), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        public string BillerCode { get; private set; }

        public string BrokerName { get; private set; }

        public string BrokerBSB { get; private set; }

        public string BrokerAccount { get; private set; }

        public DateTime FileCreated { get; private set; }
    }
}
