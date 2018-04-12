using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PaymentPortal.Models
{
    public class LoginModel
    {
        /// <summary>
        /// A Valid Link from an Invoice
        /// </summary>
        [Required]
        public Guid? Link { get; set; }

        /// <summary>
        /// The RequestId
        /// </summary>
        [Required]
        public Guid? Request { get; set; }

        /// <summary>
        /// Client Code
        /// </summary>
        [Required]
        public string ClientNumber { get; set; }
    }
}