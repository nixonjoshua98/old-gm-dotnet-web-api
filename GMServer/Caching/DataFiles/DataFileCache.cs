using Newtonsoft.Json;
using SRC.Caching.DataFiles.Models;
using System.Collections.Generic;
using System.IO;

namespace SRC.DataFiles.Cache
{
    public interface IDataFileCache
    {
        MercsDataFile Mercs { get; }
        BountiesDataFile Bounties { get; }
        List<Artefact> Artefacts { get; }
        QuestsDataFile Quests { get; }
        BountyShopDataFile BountyShop { get; }
        List<ArmouryItem> Armoury { get; }
    }

    public class DataFileCache : IDataFileCache
    {
        public MercsDataFile Mercs => LoadFile<MercsDataFile>(DataFiles.Mercs);
        public BountiesDataFile Bounties => LoadFile<BountiesDataFile>(DataFiles.Bounties);
        public QuestsDataFile Quests => LoadFile<QuestsDataFile>(DataFiles.Quests);
        public List<Artefact> Artefacts => LoadFile<List<Artefact>>(DataFiles.Artefacts);
        public List<ArmouryItem> Armoury => LoadFile<List<ArmouryItem>>(DataFiles.Armoury);
        public BountyShopDataFile BountyShop => LoadFile<BountyShopDataFile>(DataFiles.BountyShop);

        private T LoadFile<T>(string fp) where T : class
        {
            string txt = File.ReadAllText(fp);

            return JsonConvert.DeserializeObject<T>(txt);
        }
    }
}
