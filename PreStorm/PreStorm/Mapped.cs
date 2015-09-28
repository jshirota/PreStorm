using System;
using System.Reflection;

namespace PreStorm
{
    /// <summary>
    /// Provides a custom attribute for specifying the source field name.  This is used to map a database field to a property.
    /// </summary>
    public class Mapped : Attribute
    {
        /// <summary>
        /// The name of the database field.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// The property the field is mapped to.
        /// </summary>
        public PropertyInfo Property { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the Mapped class.
        /// </summary>
        /// <param name="fieldName">The name of the database field.  Case sensitive.</param>
        public Mapped(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
