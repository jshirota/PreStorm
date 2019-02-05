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
        public static void EnableTls12()
        {
#if NETCOREAPP1_0

#else
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
#endif
        }

        public static string UrlEncode(string value)
        {
#if NETCOREAPP1_0
            return WebUtility.UrlEncode(value);
#else
            return System.Web.HttpUtility.UrlEncode(value);
#endif
        }

        public static void ModifyRequest(HttpWebRequest httpWebRequest)
        {
#if NETCOREAPP1_0

#else
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
            httpWebRequest.ServicePoint.Expect100Continue = false;
#endif
        }

        public static Stream GetRequestStream(HttpWebRequest httpWebRequest)
        {
#if NETCOREAPP1_0
            return httpWebRequest.GetRequestStreamAsync().Result;
#else
            return httpWebRequest.GetRequestStream();
#endif
        }

        public static WebResponse GetResponse(WebRequest webRequest)
        {
#if NETCOREAPP1_0
            return webRequest.GetResponseAsync().Result;
#else
            return webRequest.GetResponse();
#endif
        }

        public static AssemblyBuilder DefineDynamicAssembly()
        {
#if NETCOREAPP1_0
            return AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);
#else
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);
#endif
        }

        public static bool IsGenericType(Type type)
        {
#if NETCOREAPP1_0
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static Type CreateType(TypeBuilder typeBuilder)
        {
#if NETCOREAPP1_0
            return typeBuilder.CreateTypeInfo().AsType();
#else
            return typeBuilder.CreateType();
#endif
        }

        public static T GetCustomAttribute<T>(PropertyInfo propertyInfo) where T : Attribute
        {
#if NETCOREAPP1_0
            return propertyInfo.GetCustomAttribute<T>();
#else
            return Attribute.GetCustomAttributes(propertyInfo).OfType<T>().SingleOrDefault();
#endif
        }
    }
}
