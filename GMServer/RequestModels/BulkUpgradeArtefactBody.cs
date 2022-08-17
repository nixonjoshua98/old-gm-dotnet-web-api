using System.Collections.Generic;

namespace SRC.Models.RequestModels
{
    public class UserArtefactUpgrade
    {
        public int ArtefactID;
        public int Levels;
    }


    public class BulkUpgradeArtefactBody
    {
        public List<UserArtefactUpgrade> Artefacts;
    }
}
