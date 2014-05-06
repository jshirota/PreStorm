using System;

namespace PreStorm
{
    /// <summary>
    /// Provides a custom attribute for specifying the source field name.  This is used to map a database field to a property.
    /// </summary>
    public class Mapped : Attribute
    {
        internal readonly string FieldName;
        internal readonly string DomainName;
        internal readonly bool StrictDomain;

        /// <summary>
        /// Initializes a new instance of the Mapped class.
        /// </summary>
        /// <param name="fieldName">The name of the database field.  Case sensitive.</param>
        public Mapped(string fieldName)
        {
            FieldName = GetFieldName == null ? fieldName : GetFieldName(fieldName);
        }

        /// <summary>
        /// Initializes a new instance of the Mapped class.
        /// </summary>
        /// <param name="fieldName">The name of the database field.  Case sensitive.</param>
        /// <param name="domainName">The name of the coded value domain.  Case sensitive.  Optional.  If not specified, the raw values from the database are returned.</param>
        /// <param name="strictDomain">If set to false, returns the raw value (as string) when the value is not one of the codes defined in the domain.  This only applies to when reading data from the data source.  When writing to the data source, the domain conversion is always strict.</param>
        public Mapped(string fieldName, string domainName, bool strictDomain = true)
            : this(fieldName)
        {
            DomainName = GetDomainName == null ? domainName : GetDomainName(domainName);
            StrictDomain = strictDomain;
        }

        /// <summary>
        /// The function used to retrieve the field name.  If this is set to null (default), the text sent to the Mapped constructor is the actual field name.  This can be replaced by another function such as s => ConfigurationManager.AppSettings[s], which will use the string to retrieve the real field name from app.config.
        /// </summary>
        public static Func<string, string> GetFieldName { get; set; }

        /// <summary>
        /// The function used to retrieve the domain name.  If this is set to null (default), the text sent to the Mapped constructor is the actual domain name.  This can be replaced by another function such as s => ConfigurationManager.AppSettings[s], which will use the string to retrieve the real domain name from app.config.
        /// </summary>
        public static Func<string, string> GetDomainName { get; set; }
    }
}
