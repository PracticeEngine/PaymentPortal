﻿using Newtonsoft.Json;
using PEPaymentProvider.RedPlanet;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml.Linq;

namespace PEPaymentProvider.Receipting
{
    /// <summary>
    /// Object that is created and run when the ReceiptingService triggers it
    /// </summary>
    public class ReceiptSync
    {
        private readonly RedPlanetXML redPlanetXML;
        private readonly PEPaymentService.ReceiptProcessor receiptProcessor;
        private readonly PELoggingService.LoggingService loggingService;
        private readonly Config config;
        private readonly string resilianceFile;

        public ReceiptSync()
        {
            config = new Config
            {
                Url = ConfigurationManager.AppSettings["QuickFeeUrl"],
                Username = ConfigurationManager.AppSettings["QuickFeeUsername"],
                Password = ConfigurationManager.AppSettings["QuickFeePassword"]
            };
            redPlanetXML = new RedPlanetXML();
            receiptProcessor = new PEPaymentService.ReceiptProcessor();
            loggingService = new PELoggingService.LoggingService();
            resilianceFile = HostingEnvironment.MapPath("~/App_Data/ReceiptData.xml");
        }

        /// <summary>
        /// Runs the Receipt Sync 
        /// </summary>
        /// <param name="token">Cancellation Token that should be checked whenever possible for Requested Cancellation</param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken token)
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(config.Url);

                // Login the the API
                loggingService.Logger.Information("Logging into QuickFee");
                var loginXML = redPlanetXML.BuildLoginRequest(config);
                XDocument loginResult = await PostToQuickFee(httpClient, loginXML, token);
                if (!redPlanetXML.IsSuccess(loginResult))
                    throw new Exception(redPlanetXML.GetError(loginResult).Message);

                // Check Token
                if (token.IsCancellationRequested)
                    return;

                // Request Loan Status Updates
                loggingService.Logger.Information("Requesting Loan Status");
                var session = redPlanetXML.GetSession(loginResult);
                string quoteXML = redPlanetXML.BuildLoanStatusRequest(session);
                XDocument loanStatusResponse = await PostToQuickFee(httpClient, quoteXML, token);
                if (!redPlanetXML.IsSuccess(loanStatusResponse))
                    throw new Exception(redPlanetXML.GetError(loanStatusResponse).Message);

                // Important Save this file in-case we are interrupted later.
                loggingService.Logger.Information("Writing Resiliance File");
                loanStatusResponse.Save(resilianceFile);

                // Check Token
                if (token.IsCancellationRequested)
                    return;

                // Process Loan Status Data
                loggingService.Logger.Information("Processing Loan Status Data");
                await ProcessLoanStatusData(loanStatusResponse);
                loggingService.Logger.Information("Processing Receipts Complete");
            }
            catch (Exception ex)
            {
                loggingService.Logger.Error(ex.Message);
                Trace.TraceError(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Runs the Receipt Sync 
        /// </summary>
        /// <param name="token">Cancellation Token that should be checked whenever possible for Requested Cancellation</param>
        /// <returns></returns>
        public async Task ResumeInterruptedFile(CancellationToken token)
        {
            try
            {
                if (File.Exists(resilianceFile))
                {
                    loggingService.Logger.Information("Processing Data from Resiliance File");
                    XDocument loanStatusResponse = XDocument.Load(resilianceFile);
                    // Process Loan Status Data
                    await ProcessLoanStatusData(loanStatusResponse);
                }
            }
            catch (Exception ex)
            {
                loggingService.Logger.Error(ex.Message);
                Trace.TraceError(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Parses an incoming set of Data and Sends it to PE
        /// </summary>
        /// <param name="loanStatusResponse"></param>
        /// <returns></returns>
        private async Task ProcessLoanStatusData(XDocument loanStatusResponse)
        {

            // Process the Bank Data
            foreach (var bankData in redPlanetXML.ExtractBankFiles(loanStatusResponse))
            {
                try
                {
                    var bankFile = new BankFile(bankData);

                    // Call PE System to notify of Payment
                    foreach(var detail in bankFile.Details)
                    {
                        try
                        {
                            switch (detail.InstructionType)
                            {
                                case "05":
                                    await receiptProcessor.ProcessPaymentAsync(detail.InvoiceNumber.Split(','), detail.TranReferenceNumber, detail.Amount, detail.UTCDateOfPayment);
                                    break;
                                case "15":
                                    await receiptProcessor.ProcessErrorCorrectionAsync(detail.InvoiceNumber.Split(','), detail.TranReferenceNumber, detail.Amount, detail.UTCDateOfPayment, detail.ErrorCorrectionDescription);
                                    break;
                                case "25":
                                    await receiptProcessor.ProcessReversalAsync(detail.InvoiceNumber.Split(','), detail.TranReferenceNumber, detail.Amount, detail.UTCDateOfPayment);
                                    break;
                                default:
                                    throw new Exception("Invalid InstructionType received in the BankFile from QuickFee");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log and Swallow Errors to Ensure single processing of the resilianceFile
                            loggingService.Logger.Error($"Error Processing Detail Record: {ex.Message}");
                            loggingService.Logger.Error($"Detail Record:\r\n{JsonConvert.SerializeObject(detail, Formatting.Indented)}");
                            Trace.TraceError(ex.Message);
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log and Swallow Errors to Ensure single processing of the resilianceFile
                    loggingService.Logger.Error($"Error in BankFile: {ex.Message}");
                    loggingService.Logger.Error($"BankData:\r\n{bankData}");
                    Trace.TraceError(ex.Message);
                    Debug.WriteLine(ex.Message);
                }
            }

            File.Delete(resilianceFile);
        }


        /// <summary>
        /// Posts an XML Message to Quick Fee
        /// </summary>
        /// <param name="client"></param>
        /// <param name="xml"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<XDocument> PostToQuickFee(HttpClient client, string xml, CancellationToken token)
        {
            var request = await client.PostAsync(String.Empty, new StringContent(xml), token).ConfigureAwait(false);
            request.EnsureSuccessStatusCode();
            var loginResultXML = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            return XDocument.Parse(loginResultXML);
        }

    }
}
