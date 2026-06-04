using ContactsAPI.Application.Contacts.Dtos;
using ContactsAPI.Data;
using ContactsAPI.Entities;
using ContactsAPI.Application.Helper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Linq;
using System.Text.Json;

namespace ContactsAPI.Application.Contacts.Queries.GetContactInsights
{
    public class GetContactInsightsQueryHandler(ContactsDbContext dbContext, IChatClient chatClient)
        : IRequestHandler<GetContactInsightsQuery, ContactInsightDto?>
    {
        public async Task<ContactInsightDto?> Handle(GetContactInsightsQuery request, CancellationToken cancellationToken)
        {
            // Check cache first
            if (!request.ForceRegenerate)
            {
                var cached = await dbContext.ContactInsights
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ci => ci.ContactId == request.ContactId, cancellationToken);

                if (cached != null)
                {
                    return new ContactInsightDto
                    {
                        Summary = cached.Summary,
                        Tag = cached.Tag
                    };
                }
            }

            var contact = await dbContext.Contacts
                .Include(c => c.ExtraFields)
                    .ThenInclude(f => f.Definition)
                .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken);

            if (contact == null)
            {
                return null;
            }

            // Serialize contact details to JSON for the AI context
            var contactData = new
            {
                contact.Id,
                contact.Name,
                contact.Email,
                Status = contact.Status.ToString(),
                ExtraFields = contact.ExtraFields
                    .Where(f => f.Definition.IsActive)
                    .Select(f => new
                    {
                        f.Definition.FieldName,
                        f.Definition.FieldType,
                        f.FieldValue
                    })
            };

            var serializedContact = JsonSerializer.Serialize(contactData);

            var systemPrompt = 
                "You are an intelligent CRM assistant. Your task is to analyze the provided contact information (in JSON format) and output a JSON object containing a short summary and a relationship status tag.\n" +
                "Requirements:\n" +
                "1. Output MUST be a valid JSON object matching this schema:\n" +
                "   {\n" +
                "     \"Summary\": \"A concise 2-sentence summary of the contact's profile and dynamic attributes.\",\n" +
                "     \"Tag\": \"One of: Lead, Active, At Risk\"\n" +
                "   }\n" +
                "2. The Summary MUST be exactly 2 sentences.\n" +
                "3. The Tag MUST be exactly one of: Lead, Active, At Risk.\n" +
                "4. Output ONLY the raw JSON. Do not include markdown code block formatting (like ```json), explanations, or trailing comments.";

            var response = await chatClient.GetResponseAsync(
                [
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, $"Contact Data:\n{serializedContact}")
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

            ContactInsightDto? result;
            bool aiSucceeded;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<ContactInsightDto>(responseText, options);
                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Summary) && !string.IsNullOrWhiteSpace(parsed.Tag))
                {
                    result = parsed;
                    aiSucceeded = true;
                }
                else
                {
                    result = new ContactInsightDto { Summary = "Failed to generate summary.", Tag = "At Risk" };
                    aiSucceeded = false;
                }
            }
            catch (JsonException)
            {
                result = new ContactInsightDto { Summary = "Error reading response format from AI.", Tag = "At Risk" };
                aiSucceeded = false;
            }

            // Only cache if the AI call succeeded
            if (aiSucceeded)
            {
                var insight = await dbContext.ContactInsights
                    .FirstOrDefaultAsync(ci => ci.ContactId == request.ContactId, cancellationToken);

                if (insight != null)
                {
                    insight.Summary = result.Summary;
                    insight.Tag = result.Tag;
                    insight.GeneratedAt = PhilippineTime.Now;
                }
                else
                {
                    dbContext.ContactInsights.Add(new ContactInsight
                    {
                        ContactId = request.ContactId,
                        Summary = result.Summary,
                        Tag = result.Tag,
                        GeneratedAt = PhilippineTime.Now
                    });
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }
}
