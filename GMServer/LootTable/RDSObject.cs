namespace GMServer.LootTable
{
    /// <summary>
    /// Base implementation of the IRDSObject interface.
    /// This class only implements the interface and provides all events required.
    /// Most methods are virtual and ready to be overwritten. Unless there is a good reason,
    /// do not implement IRDSObject for yourself, instead derive your base classes that shall interact
    /// in *any* thinkable way as a result source with any RDSTable from this class.
    /// </summary>
    public class RDSObject : IRDSObject
    {
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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(int indentationlevel)
        {
            string indent = "".PadRight(4 * indentationlevel, ' ');

            return string.Format(indent + "(RDSObject){0} Prob:{1},UAE:{2}{3}{4}",
                this.GetType().Name, Weight,
                (Unique ? "1" : "0"), (Always ? "1" : "0"), (Enabled ? "1" : "0"));
        }
    }
}
