using ContactsAPI.Application.Admins.Commands.ImportContacts;
using ContactsAPI.Application.Contacts.Commands.ChangeContactStatus;
using ContactsAPI.Application.Contacts.Commands.CreateContact;
using ContactsAPI.Application.Contacts.Commands.UpdateContact;
using ContactsAPI.Application.Contacts.Commands.MapCsvHeaders;
using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Application.Contacts.Queries.ExportContacts;
using ContactsAPI.Application.Contacts.Queries.GetContactById;
using ContactsAPI.Application.Contacts.Queries.GetContactsPaged;
using ContactsAPI.Application.Contacts.Queries.GetContactInsights;
using ContactsAPI.Application.Contacts.Queries.SearchContactsByAi;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ContactsAPI.API.Controllers.Contacts
{
    /// <summary>
    /// Handles all Contact CRUD operations. Delegates business logic to MediatR handlers
    /// so the controller stays thin and focused only on HTTP concerns.
    /// </summary>
    [ApiController]
    [Route("api/v1/contacts")]
    public class ContactsController(IMediator mediator) : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy" });

        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> Create([FromBody] CreateContactCommand command)
        {
            var newContactId = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = newContactId }, newContactId);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateContactCommand command)
        {
            if (id != command.Id) return BadRequest("Id mismatch");

            var success = await mediator.Send(command);
            if (!success) return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeContactStatusCommand command)
        {
            if (id != command.Id) return BadRequest();

            var result = await mediator.Send(command);
            if (!result) return NotFound();

            return NoContent();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            var contact = await mediator.Send(new GetContactByIdQuery { Id = id, IsAdmin = isAdmin });
            if (contact == null) return NotFound();

            return Ok(contact);
        }

        [HttpGet("{id}/insights")]
        [Authorize]
        public async Task<IActionResult> GetInsights(int id, [FromQuery] bool forceRegenerate = false)
        {
            var result = await mediator.Send(new GetContactInsightsQuery(id, forceRegenerate));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("ai-search")]
        [Authorize]
        public async Task<IActionResult> SearchByAi([FromQuery] string prompt)
        {
            var result = await mediator.Send(new SearchContactsByAiQuery(prompt));
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllContacts([FromQuery] GetAllContactsQuery query)
        {
            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            var isEditor = User.Identity?.IsAuthenticated == true && User.IsInRole("Editor");

            query = query with
            {
                IsAdmin = isAdmin,
                IsEditor = isEditor,
                PageSize = Math.Clamp(query.PageSize, 1, 100),
            };

            var result = await mediator.Send(query);

            if (isAdmin)
                return Ok(result);

            if (isEditor)
                return Ok(result.MapItems(c => new EditorContactDto
                {
                    Id = c.Id,
                    Sequence = c.Sequence,
                    Name = c.Name,
                    Email = c.Email,
                    ExtraFields = c.ExtraFields
                }));

            return Ok(result.MapItems(c => new PublicContactDto
            {
                Sequence = c.Sequence,
                Name = c.Name,
                Email = c.Email,
                ExtraFields = c.ExtraFields
            }));
        }

        [HttpPost("import/map-headers")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<IActionResult> MapHeaders([FromBody] MapCsvHeadersCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

        private const long MaxCsvBytes = 5 * 1024 * 1024;

        [HttpPost("import/preview")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PreviewImport(
            IFormFile file,
            [FromQuery] bool autoCreateDefinitions = false,
            [FromQuery] bool overwriteExisting = false,
            CancellationToken ct = default)
        {
            var (csvContent, error) = await ReadCsvFile(file);
            if (error is not null) return BadRequest(error);

            var result = await mediator.Send(
                new ImportContactsCommand(csvContent!, autoCreateDefinitions, overwriteExisting, DryRun: true), ct);

            return Ok(BuildResponse(result, dryRun: true));
        }

        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportContacts(
            IFormFile file,
            [FromQuery] bool autoCreateDefinitions = false,
            [FromQuery] bool overwriteExisting = false,
            CancellationToken ct = default)
        {
            var (csvContent, error) = await ReadCsvFile(file);
            if (error is not null) return BadRequest(error);

            var result = await mediator.Send(
                new ImportContactsCommand(csvContent!, autoCreateDefinitions, overwriteExisting), ct);

            return Ok(BuildResponse(result, dryRun: false));
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static async Task<(string? Content, string? Error)> ReadCsvFile(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return (null, "CSV file is required.");

            if (!string.Equals(Path.GetExtension(file.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
                return (null, "Only .csv files are accepted.");

            if (!file.ContentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase) &&
                !file.ContentType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase))
                return (null, "Invalid file type. Please upload a CSV file.");

            if (file.Length > MaxCsvBytes)
                return (null, $"File exceeds the maximum allowed size of {MaxCsvBytes / 1024 / 1024} MB.");

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            return (content, null);
        }

        private static object BuildResponse(ImportResult result, bool dryRun)
        {
            var verb = dryRun
                ? ("to import", "to update", "to skip")
                : ("imported", "updated", "skipped");

            return new
            {
                result.ImportedCount,
                result.UpdatedCount,
                result.SkippedCount,
                result.FailedRows,
                result.Errors,
                result.RowPreviews,
                Summary = $"{result.ImportedCount} {verb.Item1}, " +
                          $"{result.UpdatedCount} {verb.Item2}, " +
                          $"{result.SkippedCount} {verb.Item3}."
            };
        }

        [HttpGet("export")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportContacts(
            [FromQuery] string format = "csv",
            [FromQuery] string? columns = null,
            [FromQuery] string? ids = null)
        {
            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            var isEditor = User.Identity?.IsAuthenticated == true && User.IsInRole("Editor");

            var result = await mediator.Send(
                new ExportContactsQuery(format, columns, ids, isAdmin, isEditor)
            );

            var bytes = Encoding.UTF8.GetBytes(result.Content);
            return File(bytes, result.ContentType, result.FileName);
        }
    }
}