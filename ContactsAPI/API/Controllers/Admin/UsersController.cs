using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Users.Commands.ChangeUserStatus;
using ContactsAPI.Application.Users.Commands.CreateUser;
using ContactsAPI.Application.Users.Commands.UpdateUser;
using ContactsAPI.Application.Users.Dtos;
using ContactsAPI.Application.Users.Queries.GetAllUsers;
using ContactsAPI.Application.Users.Queries.GetUserById;
using ContactsAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsAPI.API.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/users")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController(IMediator mediator) : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy" });

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await mediator.Send(new GetUserByIdQuery { UserId = id });
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserDto>>> GetAll([FromQuery] GetAllUsersQuery query)
        {
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserCommand command)
        {
            if (id != command.UserId) return BadRequest("ID mismatch");
            var result = await mediator.Send(command);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] UserStatus status)
        {
            var result = await mediator.Send(new ChangeUserStatusCommand
            {
                UserId = id,
                Status = status
            });

            if (!result) return NotFound();
            return NoContent();
        }
    }
}