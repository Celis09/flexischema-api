using ContactsAPI.Application.Admins.Commands.UpdateAdminConfig;
using ContactsAPI.Application.Admins.Dtos;
using ContactsAPI.Application.Admins.Queries.GetAdminConfigs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsAPI.API.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/configs")]
    [Authorize(Roles = "Admin")]
    public class AdminConfigsController(IMediator mediator) : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy" });

        [HttpGet]
        public async Task<ActionResult<List<AdminConfigDto>>> GetConfigs()
            => await mediator.Send(new GetAdminConfigsQuery());

        [HttpPut("{id}")]
        public async Task<ActionResult<int>> UpdateConfig(int id, [FromBody] UpdateAdminConfigCommand command)
        {
            if (id != command.Id) return BadRequest();
            return await mediator.Send(command);
        }
    }
}