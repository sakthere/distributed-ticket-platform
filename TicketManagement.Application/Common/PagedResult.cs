namespace TicketManagement.Application.Common
{
    // The first genuinely reusable generic type in this codebase's Application
    // layer - every future paged list (tickets, users, comments, ...) returns
    // this same shape instead of each feature inventing its own Page/PageSize/
    // TotalCount envelope.
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
