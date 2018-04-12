using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PEPaymentProvider
{
    public class ClientDetails : ExtensibleModel
    {
        public int ContIndex { get; set; }

        public string ClientGovCode { get; set; }

        public string ClientCode { get; set; }

        public string ClientName { get; set; }

        public string ContAddress { get; set; }

        public string ContCity { get; set; }

        public string ContEmail { get; set; }

        public string ContPhone { get; set; }

        public string ContFax { get; set; }

        public string ContPostCode { get; set; }

        public string ContCounty { get; set; }

        public string ContTownCity { get; set; }

        public string PrimName { get; set; }

        public string PrimPhone { get; set; }

        public string PrimFax { get; set; }

        public string PrimMobile { get; set; }

        public string PrimEmail { get; set; }
    }
}