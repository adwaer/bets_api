using System.Collections.Generic;
using Bets.Games.Services.models;
using Microsoft.AspNetCore.Mvc;

namespace Bets.Web.Controllers
{
    /// <summary>
    /// Test api
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
//        [ProducesResponseType()]
        [HttpGet]
        public ActionResult<IEnumerable<IEnumerable<Game>>> Get()
        {
            return Ok();
        }
    }
}