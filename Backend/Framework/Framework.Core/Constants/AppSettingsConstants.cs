using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Constants
{
    public class AppSettingsConstants
    {
        public static string AD_DomainName = "AD_DomainName";
        public static string Environment = "Environment";
    }
    public static class AppSettingsEnvironmentVariables
    {
        public static string DEV = "DEV";
        public static string TESTOrSTG = "STG";
        public static string PROD = "PROD";
    }
}
