using System.Collections.Generic;

namespace GMServer.Models.RequestModels
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
