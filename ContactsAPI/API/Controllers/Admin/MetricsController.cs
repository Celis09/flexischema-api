using ContactsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsAPI.API.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/metrics")]
    public class MetricsController : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy" });
        [HttpGet]
        public IActionResult Get()
        {
            var metrics = new
            {
                MetricsTracker.ValidationValidTotal,
                MetricsTracker.ValidationInvalidTotal,
                MetricsTracker.AuditLogsTotal,
                MetricsTracker.ExceptionsHandledTotal,
                MetricsTracker.ExceptionsUnhandledTotal,
                MetricsTracker.ExportSuccessTotal,
                MetricsTracker.ExportFailedTotal
            };
            return Ok(metrics);
        }
    }
}
