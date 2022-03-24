using GMServer.Models.DataFileModels;
using GMServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataFileController : ControllerBase
    {
        private readonly ArtefactsService _artefacts;

        public DataFileController(ArtefactsService artefacts)
        {
            _artefacts = artefacts;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            GameDataFile datafile = new()
            {
                Artefacts = _artefacts.GetDataFile()
            };

            return Ok(datafile);
        }
    }
}