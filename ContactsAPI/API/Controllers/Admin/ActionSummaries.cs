using ContactsAPI.Application.AuditLogs.Dtos;
using ContactsAPI.Application.AuditLogs.Queries.GetAdminActionsSummary;
using ContactsAPI.Application.Contacts.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsAPI.API.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/action-summaries")]
    [Authorize(Roles = "Admin")]
    public class ActionSummaries(IMediator mediator) : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy" });

        [HttpGet]
        public async Task<ActionResult<PagedResult<AdminActionSummaryDto>>> GetAdminSummary(
            [FromQuery] string? role,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "Count",
            [FromQuery] string sortOrder = "desc")
        {
            var filter = new AdminActionSummaryFilter
            {
                Role = role,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await mediator.Send(new GetAdminActionsSummaryQuery(filter));
            return Ok(result);
        }
    }
}