using ContactsAPI.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Linq;
using System.Text.Json;

namespace ContactsAPI.Application.Contacts.Commands.MapCsvHeaders
{
    public class MapCsvHeadersCommandHandler(ContactsDbContext dbContext, IChatClient chatClient)
        : IRequestHandler<MapCsvHeadersCommand, Dictionary<string, string>>
    {
        public async Task<Dictionary<string, string>> Handle(MapCsvHeadersCommand request, CancellationToken cancellationToken)
        {
            // Fetch active extra field definitions
            var activeDefinitions = await dbContext.ExtraFieldDefinitions
                .Where(d => d.IsActive)
                .Select(d => d.FieldName)
                .ToListAsync(cancellationToken);

            // Combine with standard fields
            var standardFields = new List<string> { "Name", "Email" };
            var allSystemFields = standardFields.Concat(activeDefinitions).ToList();

            var payloadContext = new
            {
                HeadersToMap = request.CsvHeaders,
                SampleRows = request.SampleData,
                AvailableSystemFields = allSystemFields
            };

            var serializedContext = JsonSerializer.Serialize(payloadContext);

            var systemPrompt = 
                "You are an expert CRM data migration tool. Your task is to map messy headers from a user's uploaded CSV file to our system's fields.\n" +
                "Requirements:\n" +
                "1. Output MUST be a valid JSON dictionary containing ONLY mappings in the format {\"Messy Header\": \"System Field Name\"}.\n" +
                "2. The \"System Field Name\" values MUST be chosen from the provided list of AvailableSystemFields. Do not invent new field names.\n" +
                "3. If a messy header cannot be reasonably mapped to any system field, map it to an empty string \"\".\n" +
                "4. Use the provided sample row data to infer the context of each header.\n" +
                "5. Output ONLY the raw JSON. Do not include markdown code block formatting (like ```json), explanations, or trailing comments.";

            var response = await chatClient.GetResponseAsync(
                [
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, $"Context Data:\n{serializedContext}")
                ],
                cancellationToken: cancellationToken
            );

            var responseText = response.Text ?? string.Empty;

            // Strip markdown block formatting if present
            if (responseText.StartsWith("```"))
            {
                var lines = responseText.Split('\n');
                var cleanedLines = lines.Where(line => !line.Trim().StartsWith("```")).ToArray();
                responseText = string.Join("\n", cleanedLines);
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(responseText, options);
                return mappings ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                // Fallback: return empty dictionary if AI failed to return valid JSON
                return new Dictionary<string, string>();
            }
        }
    }
}
