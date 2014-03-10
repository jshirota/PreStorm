using System.Web.Script.Serialization;

namespace PreStorm.Tool
{
    internal static class Json
    {
        public static T Deserialize<T>(this string json)
        {
            var s = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return s.Deserialize<T>(json);
        }
    }
}
