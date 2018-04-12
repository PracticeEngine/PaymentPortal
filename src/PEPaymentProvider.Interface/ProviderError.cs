using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPaymentProvider
{
    public class ProviderError : ProviderResult
    {
        public string Code { get; set; }

        public string Message { get; set; }
    }
}
