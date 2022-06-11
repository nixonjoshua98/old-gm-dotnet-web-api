using System;
using System.Collections.Generic;
using System.Linq;

namespace GMServer.LootTable
{
    /// <summary>
    /// Holds a table of RDS objects. This class is "the randomizer" of the RDS.
    /// The Result implementation of the IRDSTable interface uses the RDSRandom class
    /// to determine which elements are hit.
    /// </summary>
    public class RDSTable : IRDSTable
    {
        private Random SeededRandom;

        private readonly List<IRDSObject> AllPossibleItems = new();
        private List<IRDSObject> AvailableItems;

        /// <summary>
        /// Adds the given entry to contents collection.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public virtual void AddEntry(IRDSObject entry)
        {
            AllPossibleItems.Add(entry);

            entry.Table = this;
        }

        private void AddToResult(ref List<IRDSObject> rv, IRDSObject item)
        {
            if (item.Unique)
            {
                AvailableItems.Remove(item);
            }

            if (item is IRDSTable tbl)
            {
                var results = tbl.GetResults(1, SeededRandom ?? throw new Exception("Random not found"));

                rv.AddRange(results);

                if (results.Count == 0)
                {
                    AvailableItems.Remove(item);
                }
            }
            else
            {
                rv.Add(item);
            }
        }

        public List<IRDSObject> GetResults(int count, Random rnd)
        {
            List<IRDSObject> results = new();

            SeededRandom = rnd;

            // We only want to add the items which are always added once, so we use this null check
            // to confirm if this is the first time this instance method has been called in case this is a nested table
            if (AvailableItems is null)
            {
                AvailableItems = AllPossibleItems.ToList();

                foreach (IRDSObject o in AvailableItems.Where(e => e.Always))
                    AddToResult(ref results, o);
            }

            while (count > results.Count && AvailableItems.Count > 0)
            {
                AddItemToResult(rnd, ref results);
            }

            return results;
        }

        private void AddItemToResult(Random rnd, ref List<IRDSObject> results)
        {
            double hitvalue = rnd.NextDouble() * AvailableItems.Sum(e => e.Weight);

            foreach (IRDSObject item in AvailableItems)
            {
                hitvalue -= item.Weight;

                if (hitvalue <= 0.0f)
                {
                    AddToResult(ref results, item);
                    break;
                }
            }
        }

        public double Weight { get; set; } = 1;
        public bool Unique { get; set; } = false;
        public bool Always { get; set; } = false;
        public RDSTable Table { get; set; }
    }
}
