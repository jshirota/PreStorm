using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PreStorm
{
    internal static class Mapper
    {
        public static T ToFeature<T>(this Esri.Graphic graphic, Esri.Layer layer) where T : Feature
        {
            var feature = Proxy.Create<T>();

            feature.Layer = layer;
            feature.OID = Convert.ToInt32(graphic.attributes[layer.GetObjectIdFieldName()]);

            foreach (var m in typeof(T).GetMappings())
            {
                if (!graphic.attributes.ContainsKey(m.Mapped.FieldName))
                    throw new Exception(string.Format("Field '{0}' does not exist.", m.Mapped.FieldName));

                var value = graphic.attributes[m.Mapped.FieldName];

                if (value != null)
                {
                    var t = m.Property.PropertyType;

                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                        t = new NullableConverter(t).UnderlyingType;

                    if (t == typeof(DateTime))
                    {
                        value = Config.BaseTime.AddMilliseconds(Convert.ToInt64(value));
                    }
                    else
                    {
                        var domainName = m.Mapped.DomainName;

                        if (domainName != null)
                            value = layer.GetCodedValueByCode(domainName, value).name;

                        try
                        {
                            value = Convert.ChangeType(value, t);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("'{0}.{1}' is not defined with the correct type.  Error trying to convert {2} to {3}.", typeof(T), m.Property.Name, value.GetType(), t), ex);
                        }
                    }
                }

                m.Property.SetValue(feature, value, null);
            }

            var g = graphic.geometry;

            if (g != null)
            {
                var point = feature as Feature<Point>;
                var multipoint = feature as Feature<Multipoint>;
                var polyline = feature as Feature<Polyline>;
                var polygon = feature as Feature<Polygon>;

                if (point != null) point.Geometry = new Point { x = g.x, y = g.y };
                else if (multipoint != null) multipoint.Geometry = new Multipoint { points = g.points };
                else if (polyline != null) polyline.Geometry = new Polyline { paths = g.paths };
                else if (polygon != null) polygon.Geometry = new Polygon { rings = g.rings };
                else throw new Exception("Invalid geometry type.");
            }

            feature.IsDirty = false;

            return feature;
        }

        public static object ToGraphic(this Feature feature, Esri.Layer layer, bool changesOnly)
        {
            if (changesOnly && feature.ChangedFields.Count == 0 && !feature.GeometryChanged)
                return null;

            var t = feature.GetType();

            var attributes = new Dictionary<string, object>();

            if (changesOnly)
                attributes.Add(layer.GetObjectIdFieldName(), feature.OID);

            foreach (var m in t.GetMappings())
            {
                if (changesOnly && !feature.ChangedFields.Contains(m.Mapped.FieldName))
                    continue;

                if (m.Property.GetSetMethod() == null)
                    continue;

                var value = m.Property.GetValue(feature, null);

                if (value != null)
                {
                    if (value is DateTime)
                    {
                        value = Convert.ToInt64(((DateTime)value).ToUniversalTime().Subtract(Config.BaseTime).TotalMilliseconds);
                    }
                    else
                    {
                        var domainName = m.Mapped.DomainName;

                        if (domainName != null)
                            value = layer.GetCodedValueByName(domainName, value).code;
                    }
                }

                attributes.Add(m.Mapped.FieldName, value);
            }

            return !(changesOnly && !feature.GeometryChanged) && t.HasGeometry()
                ? new { attributes, geometry = ((dynamic)feature).Geometry }
                : new { attributes } as object;
        }
    }
}
