using GMServer.Common.Types;
using GMServer.Extensions;
using GMServer.MediatR.Artefacts;
using GMServer.Models.RequestModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtefactsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ArtefactsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPut("BulkUpgrade")]
        public async Task<IActionResult> BulkUpgrade(BulkUpgradeArtefactBody body)
        {
            try
            {
                var resp = await _mediator.Send(new BulkUpgradeArtefactCommand(UserID: User.UserID(), Artefacts: body.Artefacts));

                return resp.ToResponse();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BulkUpgradeArtefact");
                return ServerError.InternalServerError;
            }
        }

        [Authorize]
        [HttpGet("Unlock")]
        public async Task<IActionResult> UnlockArtefact()
        {
            try
            {
                var resp = await _mediator.Send(new UnlockArtefactCommand(UserID: User.UserID()));

                return resp.ToResponse();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UnlockArtefact");
                return ServerError.InternalServerError;
            }
        }
    }
}