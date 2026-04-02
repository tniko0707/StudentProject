namespace Project.Models
{
    /// <summary>
    /// Пагинация событий
    /// </summary>
    public class PaginatedResult
    {
        public PaginatedResult(int eventCounter, List<Event> events, int currentPage, int elementsOnPage)
        {
            EventCounter = eventCounter;
            Events = events;
            CurrentPage = currentPage;
            ElementsOnPage = elementsOnPage;
        }

        public int EventCounter { get; set; }
        public List<Event> Events { get; set; }
        public int CurrentPage {  get; set; }
        public int ElementsOnPage { get; set; }



    }
}
