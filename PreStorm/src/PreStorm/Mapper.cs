using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PreStorm
{
    internal static class Mapper
    {
        public static T ToFeature<T>(this Graphic graphic, ServiceArgs args, Layer layer) where T : Feature
        {
            var feature = Proxy.Create<T>();

            feature.ServiceArgs = args;
            feature.Layer = layer;
            feature.OID = Convert.ToInt32(graphic.attributes[layer.GetObjectIdFieldName()]);

            var mappings = typeof(T).GetMappings().ToList();

            foreach (var m in mappings)
            {
                if (!graphic.attributes.ContainsKey(m.FieldName))
                    throw new MissingFieldException($"Field '{m.FieldName}' does not exist in '{layer.name}'.");

                var value = graphic.attributes[m.FieldName];

                if (value != null)
                {
                    var t = m.Property.PropertyType;

                    if (Compatibility.IsGenericType(t) && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                        t = new NullableConverter(t).UnderlyingType;

                    if (t == typeof(DateTime))
                    {
                        value = Esri.BaseTime.AddMilliseconds(Convert.ToInt64(value));
                    }
                    if (t == typeof(Guid))
                    {
                        value = Guid.Parse((string)value);
                    }
                    else
                    {
                        try
                        {
                            value = Convert.ChangeType(value, t);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"'{typeof(T)}.{m.Property.Name}' is not defined with the correct type.  Error trying to convert {value.GetType()} to {t}.", ex);
                        }
                    }
                }

                m.Property.SetValue(feature, value, null);
            }

            foreach (var a in graphic.attributes)
                if (a.Key != layer.GetObjectIdFieldName() && mappings.All(m => m.FieldName != a.Key))
                    feature.UnmappedFields.Add(a.Key, a.Value.ToDotNetValue(layer.fields.FirstOrDefault(f => f.name == a.Key)?.type));

            var g = graphic.geometry;

            if (g != null)
            {
                dynamic f = feature;

                if (g.x > double.MinValue && g.y > double.MinValue)
                    f.Geometry = new Point { x = g.x, y = g.y, z = g.z };
                else if (g.points != null)
                    f.Geometry = new Multipoint { points = g.points };
                else if (g.paths != null || g.curvePaths != null)
                    f.Geometry = new Polyline { paths = g.paths, curvePaths = g.curvePaths };
                else if (g.rings != null || g.curveRings != null)
                    f.Geometry = new Polygon { rings = g.rings, curveRings = g.curveRings };
            }

            feature.IsDirty = false;

            return feature;
        }

        public static object ToGraphic(this Feature feature, Layer layer, bool changesOnly)
        {
            if (changesOnly && feature.ChangedFields.Count == 0 && !feature.GeometryChanged)
                return null;

            var t = feature.GetType();

            var attributes = new Dictionary<string, object>();

            if (changesOnly)
                attributes.Add(layer.GetObjectIdFieldName(), feature.OID);

            var mappings = t.GetMappings().ToList();

            foreach (var m in mappings)
            {
                if (changesOnly && !feature.ChangedFields.Contains(m.FieldName))
                    continue;

                if (m.Property.GetSetMethod() == null)
                    continue;

                var value = m.Property.GetValue(feature, null).ToEsriValue();

                attributes.Add(m.FieldName, value);
            }

            foreach (var a in feature.UnmappedFields)
                if (!changesOnly || feature.ChangedFields.Contains(a.Key))
                    attributes.Add(a.Key, a.Value.ToEsriValue());

            return !(changesOnly && !feature.GeometryChanged) && t.HasGeometry()
                ? new { attributes, geometry = GetGeometry(feature) }
                : new { attributes } as object;
        }

        private static object ToEsriValue(this object value)
        {
            if (value == null)
                return null;

            if (value is DateTime)
                return Convert.ToInt64(((DateTime)value).ToUniversalTime().Subtract(Esri.BaseTime).TotalMilliseconds);

            if (value is Guid)
                return ((Guid)value).ToString("B").ToUpper();

            return value;
        }

        private static object ToDotNetValue(this object value, string type)
        {
            if (value == null)
                return null;

            if (type == "esriFieldTypeDate")
                return Esri.BaseTime.AddMilliseconds(Convert.ToInt64(value));

            if (type == "esriFieldTypeGlobalID" || type == "esriFieldTypeGUID")
                return Guid.Parse((string)value);

            return value;
        }

        private static object GetGeometry(Feature feature)
        {
            var g = ((dynamic)feature).Geometry;

            if (g != null)
                return g;

            if (feature.Layer == null)
                return null;

            var type = feature.Layer.geometryType;

            if (type == "esriGeometryPoint")
            {
                if (feature.Layer.hasZ)
                    return new { x = (object)null, y = (object)null, z = (object)null };

                return new { x = (object)null, y = (object)null };
            }

            if (type == "esriGeometryMultipoint")
                return new { points = new double[][] { } };

            if (type == "esriGeometryPolyline")
                return new { paths = new double[][][] { } };

            if (type == "esriGeometryPolygon")
                return new { rings = new double[][][] { } };

            return null;
        }
    }
}
