using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Notifications
{
    public class PhoneTemplate
    {
        public string CountryCode { get; set; }
        public string DialCode { get; set; }
        public string E164Number { get; set; }
        public string InternationalNumber { get; set; }
        public string NationalNumber { get; set; }
        public string Number { get; set; }

    }
}
