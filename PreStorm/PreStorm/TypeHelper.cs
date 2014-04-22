using System;
using System.Collections.Generic;
using System.Linq;

namespace PreStorm
{
    internal static class TypeHelper
    {
        private static readonly Func<Type, bool> HasGeometryMemoized = Memoization.Memoize<Type, bool>(t =>
                t.IsSubclassOf(typeof(Feature<Point>)) ||
                t.IsSubclassOf(typeof(Feature<Multipoint>)) ||
                t.IsSubclassOf(typeof(Feature<Polyline>)) ||
                t.IsSubclassOf(typeof(Feature<Polygon>)) ||
                t.IsSubclassOf(typeof(Feature<Geometry>)) ||
                t == typeof(Feature<Point>) ||
                t == typeof(Feature<Multipoint>) ||
                t == typeof(Feature<Polyline>) ||
                t == typeof(Feature<Polygon>) ||
                t == typeof(Feature<Geometry>));

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
