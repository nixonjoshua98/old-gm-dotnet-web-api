using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GMServer.LootTable
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
		/// Gets or sets a value indicating whether this <see cref="IRDSObject"/> is enabled.
		/// Only enabled entries can be part of the result of a RDSTable.
		/// </summary>
		/// <value>
		///   <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		bool Enabled { get; set; }

		/// <summary>
		/// Gets or sets the table this Object belongs to.
		/// Note to inheritors: This property has to be auto-set when an item is added to a table via the AddEntry method.
		/// </summary>
		RDSTable Table { get; set; }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="indentationlevel">The indentationlevel. 4 blanks at the beginning of each line for each level.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		string ToString(int indentationlevel);
	}

	/// <summary>
	/// This interface describes a table of IRDSObjects. One (or more) of them is/are picked as the result set.
	/// </summary>
	public interface IRDSTable : IRDSObject
	{
		List<IRDSObject> GetResults(Random rnd);
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
