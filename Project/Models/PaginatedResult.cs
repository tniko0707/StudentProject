namespace Project.Models
{
    /// <summary>
    /// Пагинация событий
    /// </summary>
    public class PaginatedResult
    {
        public List<Event> Events { get; set; }
        public int TotalPages {  get; set; }
        public int Page {  get; set; }
        public int PageSize { get; set; }


        public PaginatedResult(List<Event> events, int totalPages, int page, int pageSize)
        {
            Events = events;
            TotalPages = totalPages;
            Page = page;
            PageSize = pageSize;
        }
    }
}
