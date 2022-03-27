using GMServer.Exceptions;
using GMServer.Extensions;
using GMServer.MediatR.ArtefactHandler;
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


        [HttpPut("BulkUpgrade")]
        [Authorize]
        public async Task<IActionResult> BulkUpgrade(BulkUpgradeArtefactBody body)
        {
            try
            {
                var resp = await _mediator.Send(new BulkUpgradeArtefactRequest
                {
                    UserID = User.UserID(),
                    Artefacts = body.Artefacts
                });

                return Ok(resp);
            }
            catch (ServerException ex)
            {
                return new ServerError(ex.Message, ex.StatusCode);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "BulkUpgradeArtefact");
                return new InternalServerError("Failed to upgrade artefacts");
            }
        }

        [HttpGet("Unlock")]
        [Authorize]
        public async Task<IActionResult> UnlockArtefact()
        {
            try
            {
                var resp = await _mediator.Send(new UnlockArtefactRequest
                {
                    UserID = User.UserID(),
                });

                return Ok(resp);
            }
            catch (ServerException ex)
            {
                return new ServerError(ex.Message, ex.StatusCode);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "UnlockArtefact");
                return new InternalServerError("Failed to unlock new artefact");
            }
        }
    }
}