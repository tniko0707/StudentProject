using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class EventsController (IEventService eventService) : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(eventService.GetAllEvents());
        }
        
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var evente = eventService.GetEventById(id);
            if (evente == null) return NotFound();
            return Ok(evente);
        }

        [HttpPost]
        public IActionResult Create(Event evente)
        {
            eventService.CreateEvent(evente);
            return new CreatedResult();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]Event evente)
        {
            if (eventService.GetEventById(id) == null) return NotFound();
            eventService.UpdateEvent(id, evente);
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (eventService.GetEventById(id) == null) return NotFound();
            eventService.DeleteEvent(id);
            return new OkResult();
        }
    }
}
