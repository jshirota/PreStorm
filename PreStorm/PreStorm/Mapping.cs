using System;
using System.Linq;
using System.Reflection;

namespace PreStorm
{
    internal class Mapping
    {
        public PropertyInfo Property { get; private set; }
        public Mapped Mapped { get; private set; }

        public Mapping(PropertyInfo property)
        {
            Property = property;
            Mapped = Attribute.GetCustomAttributes(property).OfType<Mapped>().SingleOrDefault();
        }
    }
}
