using System;
using System.Collections.Generic;
using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Represents the layer.
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// The layer ID.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// The name of the layer.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The type of the layer.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The fields of the layer.
        /// </summary>
        public Field[] fields { get; set; }
    }

    /// <summary>
    /// Represents the field.
    /// </summary>
    public class Field
    {
        /// <summary>
        /// The name of the field.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The type of the field.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The domain this field depends on.
        /// </summary>
        public Domain domain { get; set; }
    }

    /// <summary>
    /// Represents the domain.
    /// </summary>
    public class Domain
    {
        /// <summary>
        /// The type of the domain.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The name of the domain.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The coded values.
        /// </summary>
        public CodedValue[] codedValues { get; set; }
    }

    /// <summary>
    /// Represents the coded value.
    /// </summary>
    public class CodedValue
    {
        /// <summary>
        /// The name of the coded value.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The actual value stored in the database.
        /// </summary>
        public object code { get; set; }
    }

    internal static class LayerHelper
    {
        #region Esri REST API Helper

        public static string GetObjectIdFieldName(this Layer layer)
        {
            var objectIdFields = layer.fields.Where(f => f.type == "esriFieldTypeOID").ToArray();

            if (objectIdFields.Length != 1)
                throw new Exception("Layer must have one and only one field of type esriFieldTypeOID.");

            return objectIdFields.Single().name;
        }

        private static IEnumerable<CodedValue> GetCodeValues(this Layer layer, string domainName)
        {
            var domain = layer.fields.Select(f => f.domain).FirstOrDefault(d => d != null && d.type == "codedValue" && d.name == domainName);

            if (domain == null)
                throw new Exception(string.Format("Coded value domain '{0}' does not exist.", domainName));

            return domain.codedValues;
        }

        public static CodedValue GetCodedValueByCode(this Layer layer, string domainName, object code)
        {
            var codedValues = layer.GetCodeValues(domainName).Where(c => c.code.ToString() == code.ToString()).ToArray();

            if (codedValues.Length == 1)
                return codedValues.Single();

            if (codedValues.Length == 0)
                throw new Exception(string.Format("Coded value domain '{0}' does not contain code '{1}'.", domainName, code));

            throw new Exception(string.Format("Coded value domain '{0}' contains {1} occurrences of code '{2}'.", domainName, codedValues.Length, code));
        }

        public static CodedValue GetCodedValueByName(this Layer layer, string domainName, object name)
        {
            var codedValues = layer.GetCodeValues(domainName).Where(c => c.name == name.ToString()).ToArray();

            if (codedValues.Length == 1)
                return codedValues.Single();

            if (codedValues.Length == 0)
                throw new Exception(string.Format("Coded value domain '{0}' does not contain name '{1}'.", domainName, name));

            throw new Exception(string.Format("Coded value domain '{0}' contains {1} occurrences of name '{2}'.", domainName, codedValues.Length, name));
        }

        #endregion
    }
}
