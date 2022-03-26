using GMServer.UserModels.DataFileModels;
using GMServer.Services;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR
{
    public class GetDataFileRequest : IRequest<GetDataFileResponse>
    {

    }

    public class GetDataFileResponse
    {
        public List<Artefact> Artefacts { get; set; }
        public List<ArmouryItem> ArmouryItems { get; set; }
        public BountiesDataFile Bounties { get; set; }
        public MercsDataFile Mercs { get; set; }
    }

    public class GetDataFileHandler : IRequestHandler<GetDataFileRequest, GetDataFileResponse>
    {
        private readonly ArtefactsService _artefacts;
        private readonly ArmouryService _armoury;
        private readonly MercService _mercs;
        private readonly BountiesService _bounties;

        public GetDataFileHandler(ArtefactsService artefacts, ArmouryService armoury, BountiesService bounties, MercService mercs)
        {
            _artefacts = artefacts;
            _armoury = armoury;
            _mercs = mercs;
            _bounties = bounties;
        }

        public Task<GetDataFileResponse> Handle(GetDataFileRequest request, CancellationToken cancellationToken)
        {
            GetDataFileResponse response = new()
            {
                Artefacts = _artefacts.GetDataFile(),
                ArmouryItems = _armoury.GetDataFile(),
                Bounties = _bounties.GetDataFile(),
                Mercs = _mercs.GetDataFile()
            };

            return Task.FromResult(response);
        }
    }
}