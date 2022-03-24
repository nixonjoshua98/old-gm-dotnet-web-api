using GMServer.Common;
using GMServer.Exceptions;
using GMServer.Models;
using GMServer.Models.UserModels;
using GMServer.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GMServer.MediatR.UserData
{
    public class GetUserDataRequest : IRequest<GetUserDataResponse>
    {
        public string UserID { get; set; }
    }

    public class GetUserDataResponse
    {
        public List<UserArtefact> Artefacts { get; set; }
    }

    public class GetUserDataHandler : IRequestHandler<GetUserDataRequest, GetUserDataResponse>
    {
        private readonly ArtefactsService _artefacts;

        public GetUserDataHandler(ArtefactsService artefacts)
        {
            _artefacts = artefacts;
        }

        public async Task<GetUserDataResponse> Handle(GetUserDataRequest request, CancellationToken cancellationToken)
        {
            GetUserDataResponse response = new()
            {
                Artefacts = await _artefacts.GetUserArtefactsAsync(request.UserID)
            };

            return response;
        }
    }
}
