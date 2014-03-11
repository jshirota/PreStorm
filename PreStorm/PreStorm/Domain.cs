namespace PreStorm
{
    /// <summary>
    /// Represents the coded value domain.
    /// </summary>
    public class Domain
    {
        /// <summary>
        /// The name of the domain.
        /// </summary>
        public string name { get; internal set; }

        /// <summary>
        /// The array of coded values.
        /// </summary>
        public CodedValue[] codedValues { get; internal set; }
    }

    /// <summary>
    /// Represents the coded value.
    /// </summary>
    public class CodedValue
    {
        /// <summary>
        /// The name of the coded value.
        /// </summary>
        public string name { get; internal set; }

        /// <summary>
        /// The actual value stored in the database.
        /// </summary>
        public object code { get; internal set; }
    }
}
