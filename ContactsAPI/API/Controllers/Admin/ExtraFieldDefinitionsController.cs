using ContactsAPI.Application.Admins.Commands.AddExtraFieldDefinition;
using ContactsAPI.Application.Admins.Commands.AddExtraFieldOption;
using ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionActiveStatus;
using ContactsAPI.Application.Admins.Commands.ChangeExtraFieldDefinitionRequiredStatus;
using ContactsAPI.Application.Admins.Commands.UpdateExtraFieldDefinition;
using ContactsAPI.Application.Admins.Dtos;
using ContactsAPI.Application.Admins.Queries.GetExtraFieldDefinitions;
using ContactsAPI.Data;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContactsAPI.API.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin/extra-fields")]
    public class ExtraFieldDefinitionsController(IMediator mediator, ContactsDbContext context) : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy" });

        // ── Definition endpoints ──────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddExtraFieldDefinitionCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExtraFieldDefinitionCommand command)
        {
            if (id != command.ExtraFieldDefinitionId)
                return BadRequest("ID mismatch");

            var result = await mediator.Send(command);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ExtraFieldDefinitionDto>>> GetAll([FromQuery] bool? isActive)
        {
            var result = await mediator.Send(new GetExtraFieldDefinitionsQuery { IsActive = isActive });
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ExtraFieldDefinitionDto>> GetById(int id)
        {
            var result = await mediator.Send(new GetExtraFieldDefinitionsQuery());
            var definition = result.FirstOrDefault(d => d.ExtraFieldDefinitionId == id);
            if (definition == null) return NotFound();
            return Ok(definition);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeActiveStatus(int id, [FromBody] ChangeExtraFieldDefinitionActiveStatusCommand command)
        {
            if (id != command.ExtraFieldDefinitionId) return BadRequest("ID mismatch");

            var result = await mediator.Send(command);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/required-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRequiredStatus(int id, [FromBody] ChangeExtraFieldDefinitionRequiredStatusCommand command)
        {
            command.ExtraFieldDefinitionId = id;
            var result = await mediator.Send(command);
            if (!result) return NotFound();
            return NoContent();
        }

        // ── Options endpoints ─────────────────────────────────────────────────────

        [HttpGet("{id}/options")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOptions(int id, CancellationToken ct)
        {
            var definitionExists = await context.ExtraFieldDefinitions
                .AsNoTracking()
                .AnyAsync(d => d.ExtraFieldDefinitionId == id, ct);

            if (!definitionExists) return NotFound($"Definition {id} not found");

            var options = await context.ExtraFieldOptions
                .AsNoTracking()
                .Where(o => o.ExtraFieldDefinitionId == id)
                .OrderBy(o => o.DisplayOrder)
                .Select(o => new
                {
                    o.ExtraFieldOptionId,
                    o.OptionValue,
                    o.DisplayOrder
                })
                .ToListAsync(ct);

            return Ok(options);
        }

        [HttpPost("{id}/options")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddOption(
            int id,
            [FromBody] string optionValue,
            CancellationToken ct)
        {
            var result = await mediator.Send(
                new AddExtraFieldOptionCommand(id, optionValue), ct);

            return CreatedAtAction(nameof(GetOptions), new { id }, result);
        }

        // [HttpDelete("{id}/options/{optionId}")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> DeleteOption(int id, int optionId, CancellationToken ct)
        // {
        //     var option = await context.ExtraFieldOptions
        //         .FirstOrDefaultAsync(o => o.ExtraFieldOptionId == optionId &&
        //                                   o.ExtraFieldDefinitionId == id, ct);
        //
        //     if (option == null) return NotFound();
        //
        //     context.ExtraFieldOptions.Remove(option);
        //     await context.SaveChangesAsync(ct);
        //     return NoContent();
        // }
    }
}