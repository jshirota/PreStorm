using System.Web.Script.Serialization;

namespace PreStorm
{
    internal static class Json
    {
        public static T Deserialize<T>(this string json)
        {
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return serializer.Deserialize<T>(json);
        }

        public static string Serialize(this object obj)
        {
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return serializer.Serialize(obj);
        }
    }
}
