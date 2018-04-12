using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPaymentProvider
{
    public interface IPaymentProvider
    {

        /// <summary>
        /// DisplayName of the Provider
        /// </summary>
        string DisplayName { get; }


        /// <summary>
        /// Submits Invoices to a Payment Provider
        /// </summary>
        /// <param name="client">The Client Details</param>
        /// <param name="invoices">The invoices selected to pay</param>
        /// <returns>Any Standard Provider result (e.g. ProviderRedirectResult) </returns>
        Task<ProviderResult> SubmitInvoices(ClientDetails client, IEnumerable<InvoiceDetails> invoices);
    }
}
