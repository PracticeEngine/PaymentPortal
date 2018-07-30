using PEPaymentProvider;
using System.Configuration;

namespace PaymentPortal.Services
{
    public static class ProviderService
    {
        /// <summary>
        /// Returns the Correct PaymentProvider (as configured in Web.config)
        /// </summary>
        /// <returns></returns>
        public static IPaymentProvider GetProvider()
        {
            var type = ConfigurationManager.AppSettings["ProviderType"];

            switch (type)
            {
                case "PEPaymentProvider.QuickFeeProvider":
                    return new QuickFeeProvider();
            }
            return null;
        }
    }
}