using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NomNomGo.IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(
            HealthCheckService healthCheckService,
            ILogger<HealthCheckController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        [HttpGet("run")]
        public async Task<IActionResult> RunHealthChecks()
        {
            _logger.LogInformation("Запуск ручной проверки работоспособности");
            
            var report = await _healthCheckService.CheckHealthAsync();
            
            var result = new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration,
                Entries = report.Entries.Select(entry => new
                {
                    Key = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration,
                    ExceptionMessage = entry.Value.Exception?.Message
                })
            };
            
            return report.Status == HealthStatus.Healthy 
                ? Ok(result) 
                : StatusCode(503, result);
        }
    }
}