using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPaymentProvider
{
    public class ProviderRedirectResult: ProviderResult
    {

        public Uri RedirectTo { get; set; }
    }
}
