using ICM.FormatSupervisor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ICM.FormatSupervisor.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        private readonly SupervisorService _supervisorService;

        public HomeController(SupervisorService supervisorService)
        {
            _supervisorService = supervisorService;
        }

        [HttpGet("publish")]
        public async Task<string> Publish()
        {
            var ret = await _supervisorService.PublishAdpRules("schemas.adp");
            return ret ? "OK" : "Failed";
        }
    }
}
