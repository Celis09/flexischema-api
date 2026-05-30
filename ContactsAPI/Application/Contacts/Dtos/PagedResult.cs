namespace ContactsAPI.Application.Contacts.Dtos
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;

        public PagedResult<TOut> MapItems<TOut>(Func<T, TOut> selector) => new()
        {
            Items = [.. Items.Select(selector)],
            TotalCount = TotalCount,
            Page = Page,
            PageSize = PageSize
        };
    }
}