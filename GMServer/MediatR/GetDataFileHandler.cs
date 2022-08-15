using GMServer.Caching.DataFiles.Models;
using MediatR;
using SRC.DataFiles.Cache;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR
{
    public record GetDataFilesCommand : IRequest<GetDataFileResponse>;

    public record GetDataFileResponse(List<Artefact> Artefacts,
                                      List<ArmouryItem> ArmouryItems,
                                      BountiesDataFile Bounties,
                                      MercsDataFile Mercs);

    public class GetDataFileHandler : IRequestHandler<GetDataFilesCommand, GetDataFileResponse>
    {
        private readonly IDataFileCache _dataFiles;

        public GetDataFileHandler(IDataFileCache dataFiles)
        {
            _dataFiles = dataFiles;
        }

        public Task<GetDataFileResponse> Handle(GetDataFilesCommand request, CancellationToken cancellationToken)
        {
            var response = new GetDataFileResponse(_dataFiles.Artefacts, _dataFiles.Armoury, _dataFiles.Bounties, _dataFiles.Mercs);

            return Task.FromResult(response);
        }
    }
}