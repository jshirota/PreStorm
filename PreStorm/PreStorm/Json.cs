using System.Web.Script.Serialization;

namespace PreStorm
{
    internal static class Json
    {
        public static T Deserialize<T>(this string json)
        {
            var s = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return s.Deserialize<T>(json);
        }

        public static string Serialize(this object obj)
        {
            var s = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return s.Serialize(obj);
        }
    }
}
