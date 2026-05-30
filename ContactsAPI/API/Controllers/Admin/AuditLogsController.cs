using ContactsAPI.Application.AuditLogs.Dtos;
using ContactsAPI.Application.AuditLogs.Queries.GetAllAuditLogs;
using ContactsAPI.Application.Contacts.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsAPI.API.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/audit-logs")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController(IMediator mediator) : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy" });

        [HttpGet]
        public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs([FromQuery] GetAllAuditLogsQuery query)
        {
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("action-types")]
        public async Task<IActionResult> GetActionTypes(CancellationToken cancellationToken)
        {
            var types = await mediator.Send(new GetActionTypesQuery(), cancellationToken);
            return Ok(types);
        }

        [HttpGet("entity-names")]
        public async Task<IActionResult> GetEntityNames(CancellationToken cancellationToken)
        {
            var names = await mediator.Send(new GetEntityNamesQuery(), cancellationToken);
            return Ok(names);
        }
    }
}