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
        public Mapped(string fieldName, string domainName)
            : this(fieldName)
        {
            DomainName = GetDomainName == null ? domainName : GetDomainName(domainName);
        }

        /// <summary>
        /// The function used to retieve the field name.  If this is set to null (default), the text sent to the Mapped constructor is the actual field name.  This can be replaced by another function such as s => ConfigurationManager.AppSettings[s], which will use the string to retrieve the real field name from app.config.
        /// </summary>
        public static Func<string, string> GetFieldName { get; set; }

        /// <summary>
        /// The function used to retieve the domain name.  If this is set to null (default), the text sent to the Mapped constructor is the actual domain name.  This can be replaced by another function such as s => ConfigurationManager.AppSettings[s], which will use the string to retrieve the real domain name from app.config.
        /// </summary>
        public static Func<string, string> GetDomainName { get; set; }
    }
}
