using MediatR;

namespace ContactsAPI.Application.Contacts.Commands.MapCsvHeaders
{
    public class MapCsvHeadersCommand : IRequest<Dictionary<string, string>>
    {
        public List<string> CsvHeaders { get; set; } = new();
        public List<List<string>> SampleData { get; set; } = new();
    }
}
