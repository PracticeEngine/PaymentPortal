using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Hosting;

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

        /// <summary>
        /// Returns a Registered Object that is loaded and run in the Hosting Environment in order to run background tasks
        /// Return null if no background tasks are necessary for the provider
        /// </summary>
        /// <returns></returns>
        IRegisteredObject GetHostedService();
    }
}
