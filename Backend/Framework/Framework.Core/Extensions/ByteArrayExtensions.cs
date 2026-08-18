using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToBase64String(this byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
            {
                return string.Empty; // Return empty if no data
            }

            return Convert.ToBase64String(byteArray);
        }
    }
}
