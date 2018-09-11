using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPaymentProvider.Receipting
{
    public class BankFile
    {
        const int RECORD_LENGTH = 219;
        const string BPAY_INSTRUCTION = "05";
        const string BPAY_ERROR = "15";
        const string BPAY_REVERSAL = "25";

        IList<BankDetailRecord> _details;

        public BankFile(string bankData)
        {
            if (String.IsNullOrWhiteSpace(bankData))
                throw new ArgumentNullException(nameof(bankData), "Bank Data was empty or missing.");

            if (bankData.Length % RECORD_LENGTH != 0)
                throw new ArgumentOutOfRangeException(nameof(bankData), "Bank Data has incomplete records.");

            if (bankData.Length / RECORD_LENGTH < 3)
                throw new ArgumentOutOfRangeException(nameof(bankData), "Bank Data has no detail records.");

            Header = new BankHeaderRecord(bankData.Substring(0, RECORD_LENGTH));
            var record = 1;
            var records = (bankData.Length / RECORD_LENGTH) - 2;
            _details = new List<BankDetailRecord>();
            while (record <= records)
            {
                _details.Add(new BankDetailRecord(bankData.Substring(record * RECORD_LENGTH, RECORD_LENGTH)));
                record++;
            }
            Trailer = new BankTrailerRecord(bankData.Substring(record * RECORD_LENGTH, RECORD_LENGTH));

            VerifyFile();
        }

        /// <summary>
        /// Verifies that the File Agrees with itself
        /// </summary>
        private void VerifyFile()
        {
            if (Trailer.NumPayments != Details.Where(d => d.InstructionType == BPAY_INSTRUCTION).Count())
                throw new Exception("Number of Payments does not match.");
            if (Trailer.TotalPayments != Details.Where(d => d.InstructionType == BPAY_INSTRUCTION).Sum(d => d.Amount))
                throw new Exception("Amount of Payments does not match.");

            if (Trailer.NumErrorCorrections != Details.Where(d => d.InstructionType == BPAY_ERROR).Count())
                throw new Exception("Number of Error Corrections does not match.");
            if (Trailer.TotalErrorCorrections != Details.Where(d => d.InstructionType == BPAY_ERROR).Sum(d => d.Amount))
                throw new Exception("Amount of Error Corrections does not match.");

            if (Trailer.NumReversals != Details.Where(d => d.InstructionType == BPAY_REVERSAL).Count())
                throw new Exception("Number of Reversals does not match.");
            if (Trailer.TotalReversals != Details.Where(d => d.InstructionType == BPAY_REVERSAL).Sum(d => d.Amount))
                throw new Exception("Amount of Reversals does not match.");

            if (Trailer.SettlementAmount != (Trailer.TotalPayments - Trailer.TotalErrorCorrections - Trailer.TotalReversals))
                throw new Exception("Settlement Amount does not agree.");
        }

        /// <summary>
        /// Header Record of the Bank File
        /// </summary>
        public BankHeaderRecord Header { get; private set; }

        /// <summary>
        /// Detail Records of the Bank File
        /// </summary>
        public IEnumerable<BankDetailRecord> Details {
            get {
                return _details;
            }
        }

        /// <summary>
        /// Trailer Record of the Bank File
        /// </summary>
        public BankTrailerRecord Trailer { get; private set; }
    }
}
