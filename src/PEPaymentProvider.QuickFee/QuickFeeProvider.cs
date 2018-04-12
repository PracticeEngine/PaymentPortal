using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PEPaymentProvider
{
    public class QuickFeeProvider : IPaymentProvider
    {
        /// <summary>
        /// Display Name of the Provider
        /// </summary>
        public string DisplayName
        {
            get
            {
                return "QuickFee";
            }
        }

        private Config ReadConfig()
        {
            return new Config
            {
                Url = ConfigurationManager.AppSettings["QuickFeeUrl"],
                Username = ConfigurationManager.AppSettings["QuickFeeUsername"],
                Password = ConfigurationManager.AppSettings["QuickFeePassword"]
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="invoices"></param>
        /// <returns></returns>
        public async Task<ProviderResult> SubmitInvoices(ClientDetails client, IEnumerable<InvoiceDetails> invoices)
        {
            try
            {
                var config = ReadConfig();
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(config.Url);

                // Login the the API
                var loginXML = buildLoginRequest(config);
                XDocument loginResult = await PostToQuickFee(httpClient, loginXML);
                if (!IsSuccess(loginResult))
                    return GetError(loginResult);

                // Request a New Quote
                var session = GetSession(loginResult);
                string quoteXML = buildQuoteRequest(session, client, invoices);
                XDocument quoteResult = await PostToQuickFee(httpClient, quoteXML);
                if (!IsSuccess(quoteResult))
                    return GetError(quoteResult);

                // Logoff the API
                //var logoffXML = buildLogoffRequest(session);
                //XDocument logoffResult = await PostToQuickFee(httpClient, logoffXML);

                //if (!IsSuccess(logoffResult))
                //    return GetError(logoffResult);

                // Return the Redirect
                return GetRedirect(quoteResult);
            }
            catch (ConfigurationErrorsException cex)
            {
                return ProviderResult.ErrorResult("CONFIG", cex.Message);
            }
            catch (HttpRequestException hex)
            {
                return ProviderResult.ErrorResult("HTTP", hex.Message);
            }
            catch (Exception ex)
            {
                return ProviderResult.ErrorResult("GENERAL", ex.Message);
            }
        }

        /// <summary>
        /// Builds the FundML Quote XML Request
        /// </summary>
        /// <param name="session">Session ID to Use</param>
        /// <param name="client"></param>
        /// <param name="invoices"></param>
        /// <returns></returns>
        private string buildQuoteRequest(string session, ClientDetails client, IEnumerable<InvoiceDetails> invoices)
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
        /// Posts an XML Message to Quick Fee
        /// </summary>
        /// <param name="client"></param>
        /// <param name="config"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        private async Task<XDocument> PostToQuickFee(HttpClient client, string xml)
        {
            var request = await client.PostAsync(String.Empty, new StringContent(xml)).ConfigureAwait(false);
            request.EnsureSuccessStatusCode();
            var loginResultXML = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            return XDocument.Parse(loginResultXML);           
        }

        /// <summary>
        /// Returns the FundML Session Id
        /// </summary>
        /// <param name="responseXML"></param>
        /// <returns></returns>
        private string GetSession(XDocument responseXML)
        {
            return responseXML.Descendants("Session").First().Value;
        }

        /// <summary>
        /// Determines if the response was a FundML Success Result
        /// </summary>
        /// <param name="responseXML"></param>
        /// <returns></returns>
        private bool IsSuccess(XDocument responseXML)
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
        private ProviderError GetError(XDocument responseXML)
        {
            var result = responseXML.Descendants("Error").First();
            return ProviderResult.ErrorResult(
                result.Descendants("Code").First().Value,
                result.Descendants("Message").First().Value);
        }

        /// <summary>
        /// Produces an Error from the response XML
        /// </summary>
        /// <param name="responseXML"></param>
        /// <returns></returns>
        private ProviderRedirectResult GetRedirect(XDocument responseXML)
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
        private string buildLoginRequest(Config config)
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
        private string buildLogoffRequest(string session)
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
    }
}
