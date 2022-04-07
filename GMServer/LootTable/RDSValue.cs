namespace GMServer.LootTable
{
    /// <summary>
    /// This class holds a single RDS value.
    /// It's a generic class to allow the developer to add any type to a RDSTable.
    /// T can of course be either a value type or a reference type, so it's possible,
    /// to add RDSValue objects that contain a reference type, too.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    public class RDSValue<T> : IRDSValue<T>
    {
        public RDSValue(T value) : this(value, 1, false, false, true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RDSValue&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="weight">The probability.</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        /// <param name="always">if set to <c>true</c> [always].</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        public RDSValue(T value, double weight, bool unique, bool always, bool enabled)
        {
            Value = value;
            Weight = weight;
            Unique = unique;
            Always = always;
            Enabled = enabled;
            Table = null;
        }

        /// <summary>
        /// The value of this object
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the probability for this object to be (part of) the result
        /// </summary>
        public double Weight { get; set; }
        /// <summary>
        /// Gets or sets whether this object may be contained only once in the result set
        /// </summary>
        public bool Unique { get; set; }
        /// <summary>
        /// Gets or sets whether this object will always be part of the result set
        /// (Probability is ignored when this flag is set to true)
        /// </summary>
        public bool Always { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRDSObject"/> is enabled.
        /// Only enabled entries can be part of the result of a RDSTable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }
        /// <summary>
        /// Gets or sets the table this Object belongs to.
        /// Note to inheritors: This property has to be auto-set when an item is added to a table via the AddEntry method.
        /// </summary>
        public RDSTable Table { get; set; }
    }
}
