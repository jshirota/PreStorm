using Newtonsoft.Json;

namespace PreStorm
{
    internal static class Json
    {
        public static T Deserialize<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Serialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
