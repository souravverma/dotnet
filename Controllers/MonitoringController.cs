using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelemetryLibrary;

namespace WebApplication4.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MonitoringController : Controller
    {
        private readonly ILogger<MonitoringController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MonitoringController(ILogger<MonitoringController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

        }

       // [HttpGet("Monitoring")]

       [Route("{name}/{path}")]
        public string Get(string name, string path)
        {
            TelemetryService objTelService = new TelemetryService(name, path, _httpContextAccessor);
            
            string jsonData = objTelService.getMetrics();

            return jsonData;

        }

        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            return new ContentResult()
            {
                StatusCode = 404,
                Content = "Not found"
            };
        }
    }
}
