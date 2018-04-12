using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PEPaymentProvider
{
    public class InvoiceDetails : ExtensibleModel
    {
        public int DebtTranIndex { get; set; }

        public DateTime DebtTranDate { get; set; }

        public string DebtTranRefAlpha { get; set; }

        public string ClientCode { get; set; }

        public string DebtTranName { get; set; }

        public decimal DebtForTotal { get; set; }

        public decimal DebtForUnpaid { get; set; }

        public string DebtTranCur {get;set;} 
    }
}