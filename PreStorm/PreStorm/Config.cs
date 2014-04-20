using System;
using System.Text.RegularExpressions;

namespace PreStorm
{
    internal static class Config
    {
        public static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool IsArcGISOnline(this string url)
        {
            return Regex.IsMatch(url, @"\.arcgis\.com/", RegexOptions.IgnoreCase);
        }
    }
}
