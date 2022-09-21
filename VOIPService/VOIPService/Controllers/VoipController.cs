using Microsoft.AspNetCore.Mvc;

namespace VOIPService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VoipController : ControllerBase
    {

        private readonly ILogger<VoipController> _logger;
        private readonly VOIPServer Voip;

        public VoipController(ILogger<VoipController> logger, VOIPServer voip)
        {
            _logger = logger;
            Voip = voip;
        }

        [HttpGet(Name = "GetConnections")]
        public IEnumerable<int> Get()
        {
            return Voip.GetConnections();
        }
    }
}