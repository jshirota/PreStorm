using System;
using System.Linq;
using System.Reflection;

namespace PreStorm
{
    internal static class TypeHelper
    {
        private static readonly Func<Type, bool> HasGeometryMemoized = Memoization.Memoize<Type, bool>(t =>
            typeof(Feature<Point>).IsAssignableFrom(t) ||
            typeof(Feature<Multipoint>).IsAssignableFrom(t) ||
            typeof(Feature<Polyline>).IsAssignableFrom(t) ||
            typeof(Feature<Polygon>).IsAssignableFrom(t) ||
            typeof(Feature<Geometry>).IsAssignableFrom(t));

        public static bool HasGeometry(this Type type)
        {
            return HasGeometryMemoized(type);
        }

        private static readonly Func<Type, Mapped[]> GetMappingsMemoized = Memoization.Memoize<Type, Mapped[]>(t =>
            t.GetProperties().Select(p =>
            {
                var mapped = Compatibility.GetCustomAttribute<Mapped>(p);

                if (mapped == null)
                    return null;

                mapped.Property = p;

                return mapped;
            }).Where(m => m != null).ToArray());

        public static Mapped[] GetMappings(this Type type)
        {
            return GetMappingsMemoized(type);
        }
    }
}
