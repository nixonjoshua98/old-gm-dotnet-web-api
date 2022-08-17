using System;
using System.Collections.Generic;

namespace SRC.LootTable
{
    /// <summary>
    /// This interface contains the properties an object must have to be a valid rds result object.
    /// </summary>
    public interface IRDSObject
    {
        /// <summary>
        /// Gets or sets the probability for this object to be (part of) the result
        /// </summary>
        double Weight { get; set; }

        /// <summary>
        /// Gets or sets whether this object may be contained only once in the result set
        /// </summary>
        bool Unique { get; set; }

        /// <summary>
        /// Gets or sets whether this object will always be part of the result set
        /// (Probability is ignored when this flag is set to true)
        /// </summary>
        bool Always { get; set; }

        /// <summary>
        /// Gets or sets the table this Object belongs to.
        /// Note to inheritors: This property has to be auto-set when an item is added to a table via the AddEntry method.
        /// </summary>
        RDSTable Table { get; set; }
    }

    /// <summary>
    /// This interface describes a table of IRDSObjects. One (or more) of them is/are picked as the result set.
    /// </summary>
    public interface IRDSTable : IRDSObject
    {
        List<IRDSObject> GetResults(int count, Random rnd);
    }

    /// <summary>
    /// Generic template for classes that return only one value, which will be good enough in most use cases.
    /// </summary>
    /// <typeparam name="T">The type of the value of this object</typeparam>
    public interface IRDSValue<T> : IRDSObject
    {
        /// <summary>
        /// The value of this object
        /// </summary>
        T Value { get; set; }
    }
}
