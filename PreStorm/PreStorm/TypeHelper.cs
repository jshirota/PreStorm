using System;
using System.Collections.Generic;
using System.Linq;

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

        private static readonly Func<Type, IEnumerable<Mapping>> GetMappingsMemoized = Memoization.Memoize<Type, IEnumerable<Mapping>>(t =>
            t.GetProperties().Select(p => new Mapping(p)).Where(m => m.Mapped != null).ToList());

        public static IEnumerable<Mapping> GetMappings(this Type type)
        {
            return GetMappingsMemoized(type);
        }
    }
}
