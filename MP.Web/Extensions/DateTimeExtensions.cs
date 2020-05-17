using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MintPlayer.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToISOString(this DateTime dateTime)
        {
            return dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture);
        }
    }
}
