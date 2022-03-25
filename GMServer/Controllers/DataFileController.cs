using GMServer.Exceptions;
using GMServer.MediatR;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataFileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DataFileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var datafile = await _mediator.Send(new GetDataFileRequest());

                return Ok(datafile);
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }
    }
}