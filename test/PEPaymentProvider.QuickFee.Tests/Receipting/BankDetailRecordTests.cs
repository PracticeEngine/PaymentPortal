using Microsoft.VisualStudio.TestTools.UnitTesting;
using PEPaymentProvider.Receipting;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEPaymentProvider.QuickFee.Tests.Receipting
{
    [TestClass]
    public class BankDetailRecordTests
    {
        const string AEST_TIMEZONE_ID = "AUS Eastern Standard Time";

        string sampleBankFile;


        [TestInitialize]
        public void SetupBankfile()
        {
            sampleBankFile =
@"5000000127081126158415001442748505ANZ20060125700742                         00000000004850020060125101753200601250000  00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
";
        }

        [TestMethod]
        public void Sample_Is_Valid()
        {
            Assert.AreEqual(219, sampleBankFile.Length, "Sample is incorrect length.");
        }

        [TestMethod]
        public void Verify_Bank_Details()
        {
            var detail = new BankDetailRecord(sampleBankFile);
            Assert.AreEqual("0000012708", detail.BillerCode, "Biller Code did not match");
            Assert.AreEqual("11261584150014427485", detail.ClientCode, "Client Code did not match");
            Assert.AreEqual("05", detail.InstructionType, "Instruction Type did not match");
            Assert.AreEqual("ANZ20060125700742", detail.TranReferenceNumber, "TranReferenceNumber did not match");
            Assert.AreEqual("", detail.InvoiceNumber, "Invoice Number did not match");
            Assert.AreEqual("000", detail.ErrorCorrectionReason, "Correction Reason did not match");
            Assert.AreEqual(485.00M, detail.Amount, "Amount did not match");
            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2006, 1, 25, 10, 17,53), TimeZoneInfo.FindSystemTimeZoneById(AEST_TIMEZONE_ID)), detail.UTCDateOfPayment, "Payment Date did not match");
            Assert.AreEqual(new DateTime(2006, 1, 25), detail.BPaySettlementDate, "Settlement Date did not match");
        }
    }
}
