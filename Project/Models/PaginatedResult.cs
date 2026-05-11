namespace Project.Models
{
    /// <summary>
    /// Пагинация событий
    /// </summary>
    public class PaginatedResult
    {
        public PaginatedResult(int totalEvents, List<Event> events, int currentPage,
            int elementsOnPage, int totalPages)
        {
            TotalEvents = totalEvents;
            Events = events;
            CurrentPage = currentPage;
            ElementsOnPage = elementsOnPage;
            TotalPages = totalPages;
        }

        public int TotalEvents { get; set; }
        public List<Event> Events { get; set; }
        public int CurrentPage {  get; set; }
        public int ElementsOnPage { get; set; }
        public int TotalPages { get; set; }



    }
}
