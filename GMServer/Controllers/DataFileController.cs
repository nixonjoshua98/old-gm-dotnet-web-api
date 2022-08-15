using GMServer.MediatR;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
            var datafile = await _mediator.Send(new GetDataFilesCommand());

            return Ok(datafile);
        }
    }
}