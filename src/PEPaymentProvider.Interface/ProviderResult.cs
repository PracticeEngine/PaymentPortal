using System;

namespace PEPaymentProvider
{
    /// <summary>
    /// Base Class for Provider Results
    /// </summary>
    public abstract class ProviderResult
    {

        public static ProviderRedirectResult RedirectResult(Uri redirect)
        {
            return new ProviderRedirectResult
            {
                RedirectTo = redirect
            };
        }

        public static ProviderError ErrorResult(string code, string message)
        {
            return new ProviderError
            {
                Code = code,
                Message = message
            };
        }
    }
}
