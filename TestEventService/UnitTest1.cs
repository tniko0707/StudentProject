using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace TestEventService
{

    public class EventServiceTest
    {
        private readonly EventService _eventService;
        public EventServiceTest()
        {
            _eventService = new EventService();
        }

        /// <summary>
        /// создание события
        /// </summary>
        [Fact]
        public void CreateEvent_ShouldReturnEvent()
        {
            //arrange
            var createdEventDTO = new CreateEventDto()
            {
                Title = "тест",
                Description = "описание",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1)
            };
            
            //act
            _eventService.CreateEvent(createdEventDTO);

            //assert
            Assert.True(_eventService.GetAllEvents().Any(e => e.Title == "тест"));
            //Assert.Contains(_eventService.GetAllEvents(), createdEventDTO);
        }



        /// <summary>
        /// получение всех событий
        /// </summary>
        [Fact]
        public void GetAllEvents_ShouldReturnEvents()
        {
            //act
            var result = _eventService.GetAllEvents();

            //assert
            Assert.NotEmpty(result);
        }
        /// <summary>
        /// получение события по ID
        /// </summary>
        [Fact]
        public void GetEventById_ShouldReturnEvent()
        {

            //act
            var result = _eventService.GetEventById(3);

            //assert
            Assert.NotNull(result);
        }
        /// <summary>
        /// обновление существующего события
        /// </summary>
        [Fact]
        public void UpdateEventTest()
        {
            //arrange
            var createdEventDTO = new UpdateEventDto()
            {
                Title = "тест",
                Description = "описание",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1)
            };
            int id = 3;
            //act
            _eventService.UpdateEvent(3, createdEventDTO);

            //assert
            Assert.True(_eventService.GetAllEvents().Any(e => e.Title == "тест"));
        }
        /// <summary>
        /// удаление существующего события;
        /// </summary>
        [Fact]
        public void DeleteEventTest()
        {
            //arrange
            int id = 3;
            //act
            _eventService.DeleteEvent(id);

            //assert
            Assert.True(!_eventService.GetAllEvents().Any(e => e.Id == id));
        }
        /// <summary>
        /// фильтрация по названию
        /// </summary>
        [Fact]
        public void FilterByName_ShouldReturnPaginationResult()
        {
            //arrange
            string title = "иМя";

            //act
            var events = _eventService.GetFilteredEvents(title);

            //assert
            Assert.NotEmpty(events.Events);
        }
        /// <summary>
        /// фильтрация по датам (startDate, endDate)
        /// </summary>
        [Fact]
        public void FilterByDate_ShouldReturnPaginationResult()
        {
            //arrange
            DateTime fromd = DateTime.Now.AddDays(-1);
            DateTime tod = DateTime.Now.AddDays(2);

            //act
            var events = _eventService.GetFilteredEvents(from: fromd, to: tod);

            //assert
            Assert.NotEmpty(events.Events);
        }
        /// <summary>
        /// пагинация событий
        /// </summary>
        [Fact]
        public void PaginationTest_ShouldReturnPaginationResult()
        {
            //act
            var result = _eventService.GetFilteredEvents(pageSize: 2);

            //assert
            Assert.True(result.Events.Count() == 2);
        }
        /// <summary>
        /// комбинированная фильтрация.
        /// </summary>
        [Fact]
        public void CombinedFiltration_ShouldReturnPaginationResult()
        {
            //act
            var result = _eventService.GetFilteredEvents("имя", 
                DateTime.Now.AddDays(-1),
                DateTime.Now.AddDays(2),
                1,
                2);

            //assert
            Assert.True(result.Events.Count() == 1);
        }

        [Fact]
        public void GetEventWithErrorId()
        {
            //arrange
            int id = 55;

            //act
            var exception = Record.Exception(() => _eventService.GetEventById(id));

            //assert
            Assert.NotNull(exception);
        }
        /// <summary>
        /// обновление события несуществующим ID
        /// </summary>
        [Fact]
        public void UpdateEventWithErrorId()
        {
            //arrange
            var createdEventDTO = new UpdateEventDto()
            {
                Title = "UpdateEventWithErrorId",
                Description = "описание",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1)
            };
            int id = 55;
            //act
            var exception = Record.Exception(() => _eventService.UpdateEvent(id, createdEventDTO));

            //assert
            Assert.NotNull(exception);
        }
        /// <summary>
        /// обновление события с EndAt раньше StartAt
        /// </summary>
        [Fact]
        public void UpdateEventWithErrorDate()
        {
            //arrange
            var createdEventDTO = new UpdateEventDto()
            {
                Title = "UpdateEventWithErrorDate",
                Description = "описание",
                StartAt = DateTime.Now.AddDays(3),
                EndAt = DateTime.Now.AddDays(1)
            };
            int id = 3;
            //act assert
            var exception = Assert.Throws<ValidationException>(() => _eventService.UpdateEvent(id, createdEventDTO));
        }
        /// <summary>
        /// создание с некорректными данными
        /// </summary>
        [Fact]
        public void CreateEventWithDataError()
        {
            //arrange
            var createdEventDTO = new CreateEventDto()
            {
                Title = "CreateEventWithDataError",
                Description = "описание",
                StartAt = DateTime.Now.AddDays(3),
                EndAt = DateTime.Now.AddDays(1)
            };
            //act assert
            var exception = Assert.Throws<ValidationException>(() => _eventService.CreateEvent(createdEventDTO));
        }
    }
}
