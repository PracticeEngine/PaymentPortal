using PEPaymentProvider.Receipting;
using PEPaymentProvider.RedPlanet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml.Linq;

namespace PEPaymentProvider
{
    public class QuickFeeProvider : IPaymentProvider
    {
        private readonly RedPlanetXML redPlanetXML;

        public QuickFeeProvider()
        {
            redPlanetXML = new RedPlanetXML();
        }

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
                var loginXML = redPlanetXML.BuildLoginRequest(config);
                XDocument loginResult = await PostToQuickFee(httpClient, loginXML);
                if (!redPlanetXML.IsSuccess(loginResult))
                    return redPlanetXML.GetError(loginResult);

                // Request a New Quote
                var session = redPlanetXML.GetSession(loginResult);
                string quoteXML = redPlanetXML.BuildQuoteRequest(session, client, invoices);
                XDocument quoteResult = await PostToQuickFee(httpClient, quoteXML);
                if (!redPlanetXML.IsSuccess(quoteResult))
                    return redPlanetXML.GetError(quoteResult);

                // Logoff the API
                //var logoffXML = buildLogoffRequest(session);
                //XDocument logoffResult = await PostToQuickFee(httpClient, logoffXML);

                //if (!IsSuccess(logoffResult))
                //    return GetError(logoffResult);

                // Return the Redirect
                return redPlanetXML.GetRedirect(quoteResult);
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
        /// Returns Hosted Service to do Background Tasks
        /// </summary>
        /// <returns></returns>
        public IRegisteredObject GetHostedService()
        {
            return new ReceiptingService();
        }
    }
}
