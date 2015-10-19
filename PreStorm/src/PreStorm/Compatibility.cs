using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;

namespace PreStorm
{
    internal static class Compatibility
    {
        public static string UrlEncode(string value)
        {
#if DOTNET
            return WebUtility.UrlEncode(value);
#else
            return System.Web.HttpUtility.UrlEncode(value);
#endif
        }

        public static void ModifyRequest(HttpWebRequest httpWebRequest)
        {
#if DOTNET

#else
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
            httpWebRequest.ServicePoint.Expect100Continue = false;
#endif
        }

        public static Stream GetRequestStream(HttpWebRequest httpWebRequest)
        {
#if DOTNET
            return httpWebRequest.GetRequestStreamAsync().Result;
#else
            return httpWebRequest.GetRequestStream();
#endif
        }

        public static WebResponse GetResponse(WebRequest webRequest)
        {
#if DOTNET
            return webRequest.GetResponseAsync().Result;
#else
            return webRequest.GetResponse();
#endif
        }

        public static T Deserialize<T>(string json)
        {
#if DOTNET
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
#else
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return serializer.Deserialize<T>(json);
#endif
        }

        public static string Serialize(object obj)
        {
#if DOTNET
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
#else
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            return serializer.Serialize(obj);
#endif
        }

        public static AssemblyBuilder DefineDynamicAssembly()
        {
#if DOTNET
            return AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);
#else
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);
#endif
        }

        public static bool IsGenericType(Type type)
        {
#if DOTNET
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static Type CreateType(TypeBuilder typeBuilder)
        {
#if DOTNET
            return typeBuilder.CreateTypeInfo().AsType();
#else
            return typeBuilder.CreateType();
#endif
        }

        public static T GetCustomAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
#if DOTNET
            return propertyInfo.GetCustomAttribute<T>();
#else
            return Attribute.GetCustomAttributes(propertyInfo).OfType<T>().SingleOrDefault();
#endif
        }
    }
}
