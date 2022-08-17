using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SRC.Common.Types;
using SRC.Extensions;
using SRC.MediatR.Artefacts;
using SRC.Models.RequestModels;
using System;
using System.Threading.Tasks;

namespace SRC.Controllers
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