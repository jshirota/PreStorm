using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json;

namespace PreStorm
{
    internal static class Compatibility
    {
        public static string UrlEncode(string value)
        {
            return WebUtility.UrlEncode(value);
        }

        public static void ModifyRequest(HttpWebRequest httpWebRequest)
        {
            //httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
            //httpWebRequest.ServicePoint.Expect100Continue = false;
        }

        public static Stream GetRequestStream(HttpWebRequest httpWebRequest)
        {
            return httpWebRequest.GetRequestStreamAsync().Result;
        }

        public static WebResponse GetResponse(WebRequest webRequest)
        {
            return webRequest.GetResponseAsync().Result;
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static AssemblyBuilder DefineDynamicAssembly()
        {
            return AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("_" + Guid.NewGuid().ToString("N")),
                AssemblyBuilderAccess.Run);
        }

        public static bool IsGenericType(Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static Type CreateType(TypeBuilder typeBuilder)
        {
            return typeBuilder.CreateTypeInfo().AsType();
        }

        public static T GetCustomAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            return propertyInfo.GetCustomAttribute<T>();
        }
    }
}
