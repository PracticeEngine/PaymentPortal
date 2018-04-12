using PEPaymentProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaymentPortal.Models
{
    public class SelectInvoicesModel
    {
        public ClientDetails Client { get; set; }

        public IEnumerable<InvoiceDetails> Invoices { get; set; }

    }
}