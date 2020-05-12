using PaymentPortal.Models;
using PaymentPortal.Services;
using PEPaymentProvider;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace PaymentPortal.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentProvider paymentProvider;
        private readonly AntiForgeryService antiForgeryService;
        private readonly EngineService engineService;

        public PaymentsController()
            :base()
        {
            this.paymentProvider = ProviderService.GetProvider();
            this.antiForgeryService = new AntiForgeryService();
            this.engineService = new EngineService();

            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            ViewBag.ProviderName = paymentProvider.DisplayName;
            ViewBag.Title = ConfigurationManager.AppSettings["FirmName"];
            base.Initialize(requestContext);
        }

        /// <summary>
        /// First request to the login page
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string id)
        {
            try
            {
                Guid link;
                if (String.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out link))
                {
                    return RedirectToAction("InvalidLink");
                }

                LoginModel model = new LoginModel
                {
                    Link = link
                };
                model.Request = antiForgeryService.GenerateToken(link);
                return View(model);

            }
            catch
            {
                return RedirectToAction("Error");
            }
        }


        /// <summary>
        /// Processes a Login Attempt, returning the same, or issuing an Auth Cookie and sending to the SelectInvoices page
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (antiForgeryService.ValidateToken(model.Link.Value, model.Request.Value))
                    {
                        int? clientId = engineService.ValidateCredential(model.Link.Value, model.ClientNumber);
                        if (clientId.HasValue)
                        {
                            FormsAuthentication.SetAuthCookie(clientId.Value.ToString(), false);
                            return RedirectToAction("SelectInvoices");
                        }
                        else
                        {
                            model.ClientNumber = String.Empty;
                        }
                    }
                    model.Request = antiForgeryService.GenerateToken(model.Link.Value);
                }
                return View(model);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// The Page where the user can select Invoices to pay
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult SelectInvoices()
        {
            try
            {
                int ContIndex = Convert.ToInt32(User.Identity.Name);
                var model = new SelectInvoicesModel
                {
                    Client = engineService.GetClientDetails(ContIndex),
                    Invoices = engineService.GetInvoiceDetails(ContIndex)
                };
                return View(model);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Signout (remove the Auth cookie)
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Signout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Goodbye");
        }

        /// <summary>
        /// Tell the user to close the browser
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Goodbye()
        {
            return View();
        }

        /// <summary>
        /// Selection of Invoices the user wants to pay, process and submit to Payment Provider
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> PayNow(string[] Invoices)
        {
            try
            {
                if (Invoices.Sum(i => i.Length) > 21)
                {
                    ModelState.AddModelError("", "Too many invoices selected. Sorry, but we cannot process all those invoices at one time.");
                    return View("SelectInvoices");
                }

                int ContIndex = Convert.ToInt32(User.Identity.Name);
                var client = engineService.GetClientDetails(ContIndex);
                var invoices = engineService.GetInvoiceDetails(ContIndex);
                var topay = invoices.Where(i => Invoices.Contains(i.DebtTranRefAlpha)).ToList();

                ProviderResult result = await paymentProvider.SubmitInvoices(client, topay);

                // Deal with Error First
                if (result is ProviderError)
                {
                    return View("ProviderError", (ProviderError)result);
                }
                else
                {
                    // If not error, record payment sent
                    engineService.SetPaymentMessage(ContIndex, String.Join(",", Invoices), paymentProvider.DisplayName);
                }

                // Now Handle the Response
                if (result is ProviderRedirectResult)
                {
                    ProviderRedirectResult redirectresult = (ProviderRedirectResult)result;
                    return Redirect(redirectresult.RedirectTo.AbsoluteUri);
                }
                // FUTURE: handle other result types

                return RedirectToAction("Error");
            }
            catch
            {
                return RedirectToAction("Error");
            }
        }

        /// <summary>
        /// Error Page
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Error()
        {
            return View();
        }

        /// <summary>
        /// Invalid Link Page
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult InvalidLink()
        {
            return View();
        }
    }
}