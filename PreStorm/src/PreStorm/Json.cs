namespace PreStorm
{
    internal static class Json
    {
        public static T Deserialize<T>(this string json)
        {
            return Compatibility.Deserialize<T>(json);
        }

        public static string Serialize(this object obj)
        {
            return Compatibility.Serialize(obj);
        }
    }
}
