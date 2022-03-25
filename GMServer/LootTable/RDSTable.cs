using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GMServer.LootTable
{
	/// <summary>
	/// Holds a table of RDS objects. This class is "the randomizer" of the RDS.
	/// The Result implementation of the IRDSTable interface uses the RDSRandom class
	/// to determine which elements are hit.
	/// </summary>
	public class RDSTable : IRDSTable
	{
		Random SeededRandom;

		/// <summary>
		/// Initializes a new instance of the <see cref="RDSTable"/> class.
		/// </summary>
		public RDSTable(): this(null, 1, 1, false, false, true)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RDSTable"/> class.
		/// </summary>
		/// <param name="contents">The contents.</param>
		/// <param name="count">The count.</param>
		/// <param name="probability">The probability.</param>
		/// <param name="unique">if set to <c>true</c> any item of this table (or contained sub tables) can be in the result only once.</param>
		/// <param name="always">if set to <c>true</c> the probability is disabled and the result will always contain (count) entries of this table.</param>
		/// <param name="enabled">if set to <c>true</c> [enabled].</param>
		public RDSTable(List<IRDSObject> contents, int count, double probability, bool unique, bool always, bool enabled)
		{
			Contents = contents ?? new();

			Count = count;
			Weight = probability;
			Unique = unique;
			Always = always;
			Enabled = enabled;
		}

		/// <summary>
		/// The maximum number of entries expected in the Result. The final count of items in the result may be lower
		/// if some of the entries may return a null result (no drop)
		/// </summary>
		public int Count { get; set; }

		/// <summary>
		/// Gets or sets the contents of this table
		/// </summary>
		public List<IRDSObject> Contents { get; set; }

		/// <summary>
		/// Adds the given entry to contents collection.
		/// </summary>
		/// <param name="entry">The entry.</param>
		public virtual void AddEntry(IRDSObject entry)
		{
			Contents.Add(entry);
			entry.Table = this;
		}

		private List<IRDSObject> UniqueDrops = new List<IRDSObject>();

		private void AddToResult(List<IRDSObject> rv, IRDSObject o)
		{
			if (!o.Unique || !UniqueDrops.Contains(o))
			{
				if (o.Unique)
					UniqueDrops.Add(o);

				if (o is IRDSTable tbl)
				{
					rv.AddRange(tbl.GetResults(SeededRandom ?? throw new Exception("Random not found")));
				}
				else
				{
					rv.Add(o);
				}
			}
		}

		public List<IRDSObject> GetResults(Random rnd)
		{
			SeededRandom = rnd;

			// The return value, a list of hit objects
			List<IRDSObject> rv = new List<IRDSObject>();
			UniqueDrops = new List<IRDSObject>();

			// Add all the objects that are hit "Always" to the result
			// Those objects are really added always, no matter what "Count"
			// is set in the table! If there are 5 objects "always", those 5 will
			// drop, even if the count says only 3.
			foreach (IRDSObject o in Contents.Where(e => e.Always && e.Enabled))
				AddToResult(rv, o);

			// Now calculate the real dropcount, this is the table's count minus the
			// number of Always-drops.
			// It is possible, that the remaining drops go below zero, in which case
			// no other objects will be added to the result here.
			int alwayscnt = Contents.Count(e => e.Always && e.Enabled);
			int realdropcnt = Count - alwayscnt;

			// Continue only, if there is a Count left to be processed
			if (realdropcnt > 0)
			{
				for (int dropcount = 0; dropcount < realdropcnt; dropcount++)
				{
					// Find the objects, that can be hit now
					// This is all objects, that are Enabled and that have not already been added through the Always flag
					List<IRDSObject> dropables = Contents.Where(e => e.Enabled && !e.Always).ToList();

					// This is the magic random number that will decide, which object is hit now
					double hitvalue = rnd.NextDouble() * dropables.Sum(e => e.Weight);

					// Find out in a loop which object's probability hits the random value...
					double runningvalue = 0;
					foreach (IRDSObject o in dropables)
					{
						// Count up until we find the first item that exceeds the hitvalue...
						runningvalue += o.Weight;
						if (hitvalue < runningvalue)
						{
							// ...and the oscar goes too...
							AddToResult(rv, o);
							break;
						}
					}
				}
			}

			// Return the set now
			return rv;
		}

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
		/// <param name="indentationlevel">The indentationlevel. 4 blanks at the beginning of each line for each level.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(int indentationlevel)
		{
			string indent = "".PadRight(4 * indentationlevel, ' ');

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(indent + "(RDSTable){0} Entries:{1},Prob:{2},UAE:{3}{4}{5}{6}", 
				this.GetType().Name, Contents.Count, Weight,
				(Unique ? "1" : "0"), (Always ? "1" : "0"), (Enabled ? "1" : "0"), (Contents.Count > 0 ? "\r\n" : ""));

			foreach (IRDSObject o in Contents)
				sb.AppendLine(indent + o.ToString(indentationlevel + 1));

			return sb.ToString();
		}
	}
}
