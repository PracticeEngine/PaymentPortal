using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PEPaymentProvider.RedPlanet
{
    public class RedPlanetXML
    {

        public RedPlanetXML()
        {

        }

        /// <summary>
        /// Builds the FundML Quote XML Request
        /// </summary>
        /// <param name="session">Session ID to Use</param>
        /// <param name="client"></param>
        /// <param name="invoices"></param>
        /// <returns></returns>
        public string BuildQuoteRequest(string session, ClientDetails client, IEnumerable<InvoiceDetails> invoices)
        {
            var invtotal = invoices.Select(i => i.DebtForUnpaid).Sum();
            var invnumbers = String.Join(",", invoices.Select(i => i.DebtTranRefAlpha).ToArray());

            var linebreaks = @"[\r\n]+";
            var regex = new Regex(linebreaks);
            var addrlines = regex.Split(client.ContAddress);
            string addr1 = null;
            string addr2 = null;
            if (addrlines.Count() > 0)
                addr1 = addrlines[0];
            if (addrlines.Count() > 1)
                addr2 = addrlines[1];

            var request = new XDocument(
                new XElement("FundML",
                new XElement("Version", 1),
                new XElement("Request",
                new XElement("Code", "NewQuote"),
                new XElement("Session", session),
                new XElement("Client",
                new XElement("ABN", client.ClientGovCode),
                new XElement("Name", client.ClientName),
                new XElement("RegisteredName", client.ClientName),
                new XElement("BrokerID", client.ContIndex),
                new XElement("PostalAddress",
                new XElement("Address1", addr1),
                new XElement("Address2", addr2),
                new XElement("Suburb", client.ContTownCity),
                new XElement("State", client.ContCounty),
                new XElement("Postcode", client.ContPostCode)
                ),  // PostalAddress
                new XElement("StreetAddress",
                new XElement("Address1", addr1),
                new XElement("Address2", addr2),
                new XElement("Suburb", client.ContTownCity),
                new XElement("State", client.ContCounty),
                new XElement("Postcode", client.ContPostCode)
                ),  // StreetAddresss
                new XElement("Telephone", client.ContPhone),
                new XElement("Fax", client.ContFax),
                new XElement("Email", client.ContEmail),
                new XElement("FSRAType"),
                new XElement("BankAccountNumber"),
                new XElement("BankName"),
                new XElement("BankBranch"),
                new XElement("CreditCardToken"),
                new XElement("CreditCardExpiry"),
                new XElement("Contact",
                new XElement("Name", client.PrimName),
                new XElement("Telephone", client.PrimPhone),
                new XElement("Mobile", client.PrimMobile),
                new XElement("Fax", client.PrimFax),
                new XElement("Email", client.PrimEmail)
                )   // Contact
                ),  // Client
                 new XElement("InvoiceDetails",
                 new XElement("InvoicesTotal", invtotal),
                 new XElement("InvoiceNumbers", invnumbers)
                 )   // Invoices
                )));
            return request.ToString();
        }



        /// <summary>
        /// Returns the FundML Session Id
        /// </summary>
        /// <param name="responseXML"></param>
        /// <returns></returns>
        public string GetSession(XDocument responseXML)
        {
            return responseXML.Descendants("Session").First().Value;
        }

        /// <summary>
        /// Determines if the response was a FundML Success Result
        /// </summary>
        /// <param name="responseXML"></param>
        /// <returns></returns>
        public bool IsSuccess(XDocument responseXML)
        {
            var result = responseXML.Descendants("Result").First().Value;
            if (result.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Produces an Error from the response XML
        /// </summary>
        /// <param name="responseXML"></param>
        /// <returns></returns>
        public ProviderError GetError(XDocument responseXML)
        {
            var result = responseXML.Descendants("Error").First();
            return ProviderResult.ErrorResult(
                result.Descendants("Code").First().Value,
                result.Descendants("Message").First().Value);
        }

        /// <summary>
        /// Returns the Redirect Information from a New Loan Response
        /// </summary>
        /// <param name="responseXML"></param>
        /// <returns></returns>
        public ProviderRedirectResult GetRedirect(XDocument responseXML)
        {
            var result = responseXML.Descendants("Response").First();
            var uri = new Uri(result.Descendants("URL").First().Value);
            return ProviderResult.RedirectResult(uri);
        }

        /// <summary>
        /// Builds the Login XML
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public string BuildLoginRequest(Config config)
        {
            var request = new XDocument(
                new XElement("FundML",
                new XElement("Version", 1),
                new XElement("Request",
                new XElement("Code", "LogOn"),
                new XElement("Broker",
                new XElement("Username", config.Username),
                new XElement("Password", config.Password)
                ))));
            return request.ToString();
        }

        /// <summary>
        /// Builds the Login XML
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public string BuildLogoffRequest(string session)
        {
            var request = new XDocument(
                new XElement("FundML",
                new XElement("Version", 1),
                new XElement("Request",
                new XElement("Code", "LogOff"),
                new XElement("Session", session)
                )));
            return request.ToString();
        }

        /// <summary>
        /// Builds a Request for LoanStatus Details
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public string BuildLoanStatusRequest(string session)
        {
            var request = new XDocument(
                new XElement("FundML",
                new XElement("Version", 1),
                new XElement("Request",
                new XElement("Code", "LoanStatus"),
                new XElement("Session", session),
                new XElement("Filter", "All")
                )));
            return request.ToString();
        }

        /// <summary>
        /// Builds a Request for LoanStatusConfirmation
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public string BuildLoanStatusConfirmationRequest(string session)
        {
            var request = new XDocument(
                new XElement("FundML",
                new XElement("Version", 1),
                new XElement("Request",
                new XElement("Code", "LoanStatusConfirmation"),
                new XElement("Session", session)
                )));
            return request.ToString();
        }

        /// <summary>
        /// Returns Raw BankFile Fixed-width Values from the response
        /// </summary>
        /// <param name="responseXML"></param>
        /// <returns></returns>
        public IEnumerable<string> ExtractBankFiles(XDocument responseXML)
        {
            var bankFiles = responseXML.Descendants("BankFiles").First();
            foreach(var bankFile in bankFiles.Descendants("BankFile"))
            {
                yield return bankFile.Value.Replace("\n", "").Replace("\r", "");
            }
        }
    }
}
