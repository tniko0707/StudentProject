namespace TestEventService;

using Application.DTO;
using Application.Repositories;
using Application.Services;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

public sealed class EventServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IEventService _eventService;
    private readonly IUserService _userService;
    private readonly CancellationToken cancellationToken;
    private readonly User _user;
    public EventServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();

        _user = new User()
        {
            UserId = Guid.NewGuid(),
            Login = "log",
            PasswordHash = PasswordHasher.HashPassword("abracadabra"),
            Role = Role.Admin,
            Bookings = new List<Booking>()
        };

        cancellationToken = new CancellationTokenSource().Token;
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }

    #region CreateEventAsync Tests

    [Fact]
    public async Task CreateEventAsync_WithValidData_ReturnsEventInfo()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var CreateEventDto = new CreateEventDto()
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        };

        var result = await _eventService.CreateEventAsync(CreateEventDto, cancellationToken);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Event", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(futureDate, result.StartAt);
        Assert.Equal(futureDate.AddHours(2), result.EndAt);
    }

    [Fact]
    public async Task CreateEventAsync_WithNullTitle_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createEventDto = new CreateEventDto
        {
            Title = null,
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
        _eventService.CreateEventAsync(createEventDto, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);
    }

    [Fact]
    public async Task CreateEventAsync_WithEmptyTitle_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var CreateEventDto = new CreateEventDto
        {
            Title = "   ",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
        _eventService.CreateEventAsync(CreateEventDto, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);
    }

    [Fact]
    public async Task CreateEventAsync_WithNullStartAt_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var CreateEventDto = new CreateEventDto
        {
            Title = "Test Event",
            StartAt = null,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
        _eventService.CreateEventAsync(CreateEventDto, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);
    }

    [Fact]
    public async Task CreateEventAsync_WithNullEndAt_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var CreateEventDto = new CreateEventDto
        {
            Title = "Test Event",
            StartAt = futureDate,
            EndAt = null,
            TotalSeats = 10,
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
        _eventService.CreateEventAsync(CreateEventDto, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);
    }

    [Fact]
    public async Task CreateEventAsync_WithPastStartAt_ThrowsValidationException()
    {
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var CreateEventDto = new CreateEventDto
        {
            Title = "Test Event",
            StartAt = pastDate,
            EndAt = pastDate.AddHours(2),
            TotalSeats = 10,
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
        _eventService.CreateEventAsync(CreateEventDto, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);
    }

    [Fact]
    public async Task CreateEventAsync_WithEndAtBeforeStartAt_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var CreateEventDto = new CreateEventDto
        {
            Title = "Test Event",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(-1),
            TotalSeats = 10,
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
        _eventService.CreateEventAsync(CreateEventDto, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);
    }

    [Fact]
    public async Task CreateEventAsync_WithEndAtEqualToStartAt_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var CreateEventDto = new CreateEventDto
        {
            Title = "Test Event",
            StartAt = futureDate,
            EndAt = futureDate,
            TotalSeats = 10,
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
        _eventService.CreateEventAsync(CreateEventDto, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);
    }

    [Fact]
    public async Task CreateEventAsync_WithTitleWhitespace_TrimsTitleAndCreatesEvent()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createEventDto = new CreateEventDto
        {
            Title = "  Test Event  ",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        };

        var result = await _eventService.CreateEventAsync(createEventDto, cancellationToken);

        Assert.Equal("Test Event", result.Title);
    }

    #endregion

    #region GetEventByIdAsync Tests

    [Fact]
    public async Task GetEventByIdAsync_WithValidId_ReturnsEventInfo()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createdEvent = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.GetEventByIdAsync(createdEvent.Id, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(createdEvent.Id, result.Id);
        Assert.Equal("Test Event", result.Title);
    }

    [Fact]
    public async Task GetEventByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        var invalidId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
        _eventService.GetEventByIdAsync(invalidId, cancellationToken));
        Assert.Equal($"Событие {invalidId} не найдено", exception.Message);
    }

    #endregion

    #region GetAllEventsAsync Tests

    [Fact]
    public async Task GetAllEventsAsync_WithNoEvents_ReturnsEmptyArray()
    {
        var result = await _eventService.GetAllEventsAsync(cancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.Equal(0, result.Count());
    }

    [Fact]
    public async Task GetAllEventsAsync_WithMultipleEvents_ReturnsAllEvents()
    {
        var futureDate1 = DateTime.UtcNow.AddDays(1);
        var futureDate2 = DateTime.UtcNow.AddDays(2);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event 1",
            StartAt = futureDate1,
            EndAt = futureDate1.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event 2",
            StartAt = futureDate2,
            EndAt = futureDate2.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.GetAllEventsAsync(cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllEventsAsync_WithFromFilter_ReturnsFilteredEvents()
    {
        var futureDate1 = DateTime.UtcNow.AddDays(1);
        var futureDate2 = DateTime.UtcNow.AddDays(2);
        var filterDate = futureDate1.AddHours(1);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event 1",
            StartAt = futureDate1,
            EndAt = futureDate1.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event 2",
            StartAt = futureDate2,
            EndAt = futureDate2.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.GetFilteredEventsAsync(from: filterDate);

        Assert.Single(result.Events);
        Assert.Equal("Event 2", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithToFilter_ReturnsFilteredEvents()
    {
        var futureDate1 = DateTime.UtcNow.AddDays(1);
        var futureDate2 = DateTime.UtcNow.AddDays(2);
        var filterDate = futureDate1.AddHours(3);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event 1",
            StartAt = futureDate1,
            EndAt = futureDate1.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event 2",
            StartAt = futureDate2,
            EndAt = futureDate2.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.GetFilteredEventsAsync(to: filterDate);

        Assert.Single(result.Events);
        Assert.Equal("Event 1", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithTitleFilter_ReturnsFilteredEvents()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Conference 2024",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Meeting Q1",
            StartAt = futureDate.AddDays(1),
            EndAt = futureDate.AddDays(1).AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.GetFilteredEventsAsync(title: "Conference");

        Assert.Single(result.Events);
        Assert.Equal("Conference 2024", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithTitleFilter_IsCaseInsensitive()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Conference 2024",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.GetFilteredEventsAsync(title: "conference");

        Assert.Single(result.Events);
        Assert.Equal("Conference 2024", result.Events[0].Title);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithMultipleFilters_ReturnsFilteredEvents()
    {
        var baseDate = DateTime.UtcNow.AddDays(1);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Conference 2024",
            StartAt = baseDate,
            EndAt = baseDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Conference 2025",
            StartAt = baseDate.AddDays(5),
            EndAt = baseDate.AddDays(5).AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.GetFilteredEventsAsync(
            from: baseDate.AddDays(2),
            to: baseDate.AddDays(6),
            title: "Conference");

        Assert.Single(result.Events);
        Assert.Equal("Conference 2025", result.Events[0].Title);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetAllEventsAsync_WithDefaultPagination_ReturnsFirstPageWithDefaultPageSize()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        for (int i = 1; i <= 15; i++)
        {
            await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = $"Event {i}",
                StartAt = futureDate.AddHours(i),
                EndAt = futureDate.AddHours(i + 1),
                TotalSeats = 10,
            }, cancellationToken);
        }

        var result = await _eventService.GetFilteredEventsAsync();

        Assert.Equal(15, result.TotalEvents);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.ElementsOnPage);
        Assert.Equal(10, result.Events.Count());
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithCustomPageSize_ReturnsCorrectNumberOfEvents()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        for (int i = 1; i <= 25; i++)
        {
            await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = $"Event {i}",
                StartAt = futureDate.AddHours(i),
                EndAt = futureDate.AddHours(i + 1),
                TotalSeats = 10,
            }, cancellationToken);
        }

        var result = await _eventService.GetFilteredEventsAsync(page: 1, pageSize: 5);

        Assert.Equal(25, result.TotalEvents);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(5, result.ElementsOnPage);
        Assert.Equal(5, result.Events.Count());
        Assert.Equal(5, result.TotalPages);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithSecondPage_ReturnsCorrectEvents()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        for (int i = 1; i <= 25; i++)
        {
            await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = $"Event {i}",
                StartAt = futureDate.AddHours(i),
                EndAt = futureDate.AddHours(i + 1),
                TotalSeats = 10,
            }, cancellationToken);
        }

        var result = await _eventService.GetFilteredEventsAsync(page: 2, pageSize: 10);

        Assert.Equal(25, result.TotalEvents);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(10, result.ElementsOnPage);
        Assert.Equal(10, result.Events.Count());
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithLastPagePartialResults_ReturnsRemainingEvents()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        for (int i = 1; i <= 23; i++)
        {
            await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = $"Event {i}",
                StartAt = futureDate.AddHours(i),
                EndAt = futureDate.AddHours(i + 1),
                TotalSeats = 10,
            }, cancellationToken);
        }

        var result = await _eventService.GetFilteredEventsAsync(page: 3, pageSize: 10);

        Assert.Equal(23, result.TotalEvents);
        Assert.Equal(3, result.CurrentPage);
        Assert.Equal(10, result.ElementsOnPage);
        Assert.Equal(3, result.Events.Count());
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithPageBeyondTotal_ReturnsEmptyEvents()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        for (int i = 1; i <= 5; i++)
        {
            await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = $"Event {i}",
                StartAt = futureDate.AddHours(i),
                EndAt = futureDate.AddHours(i + 1),
                TotalSeats = 10,
            }, cancellationToken);
        }

        var result = await _eventService.GetFilteredEventsAsync(page: 10, pageSize: 10);

        Assert.Equal(5, result.TotalEvents);
        Assert.Equal(10, result.CurrentPage);
        Assert.Equal(10, result.ElementsOnPage);
        Assert.Empty(result.Events);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithPaginationAndFilters_ReturnsPaginatedFilteredResults()
    {
        var baseDate = DateTime.UtcNow.AddDays(1);
        for (int i = 1; i <= 30; i++)
        {
            await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = $"Conference {i}",
                StartAt = baseDate.AddDays(i),
                EndAt = baseDate.AddDays(i).AddHours(2),
                TotalSeats = 10,
            }, cancellationToken);
        }

        var result = await _eventService.GetFilteredEventsAsync(
            page: 2,
            pageSize: 5,
            title: "Conference");

        Assert.Equal(30, result.TotalEvents);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(5, result.ElementsOnPage);
        Assert.Equal(5, result.Events.Count());
        Assert.Equal(6, result.TotalPages);
    }

    [Fact]
    public async Task GetAllEventsAsync_WithPaginationPageSizeOne_ReturnsOneItemPerPage()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        for (int i = 1; i <= 3; i++)
        {
            await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = $"Event {i}",
                StartAt = futureDate.AddHours(i),
                EndAt = futureDate.AddHours(i + 1),
                TotalSeats = 10,
            }, cancellationToken);
        }

        var result = await _eventService.GetFilteredEventsAsync(page: 2, pageSize: 1);

        Assert.Equal(3, result.TotalEvents);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(1, result.ElementsOnPage);
        Assert.Single(result.Events);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetAllEventsAsync_TotalPagesCalculation_IsCorrect()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        for (int i = 1; i <= 37; i++)
        {
            await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = $"Event {i}",
                StartAt = futureDate.AddHours(i),
                EndAt = futureDate.AddHours(i + 1),
                TotalSeats = 10,
            }, cancellationToken);
        }

        var result = await _eventService.GetFilteredEventsAsync(pageSize: 10);

        Assert.Equal(4, result.TotalPages);
    }

    [Fact]
    public async Task GetAllEventsAsync_FirstPageIsOne_NotZero()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event 1",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(1),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.GetFilteredEventsAsync(page: 1);

        Assert.Equal(1, result.CurrentPage);
    }

    #endregion

    #region UpdateEventAsync Tests

    [Fact]
    public async Task UpdateEventAsync_WithValidData_UpdatesEvent()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createdEvent = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Original Event",
            Description = "Original Description",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var newFutureDate = DateTime.UtcNow.AddDays(2);
        var updateEvent = new UpdateEventDto
        {
            Title = "Updated Event",
            Description = "Updated Description",
            StartAt = newFutureDate,
            EndAt = newFutureDate.AddHours(3)
        };

        var result = await _eventService.UpdateEventAsync(createdEvent.Id, updateEvent, cancellationToken);

        Assert.Equal(createdEvent.Id, result.Id);
        Assert.Equal("Updated Event", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(newFutureDate, result.StartAt);
        Assert.Equal(newFutureDate.AddHours(3), result.EndAt);
    }

    [Fact]
    public async Task UpdateEventAsync_WithInvalidId_ThrowsNotFoundException()
    {
        var invalidId = Guid.NewGuid();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var updateEvent = new UpdateEventDto
        {
            Title = "Updated Event",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2)
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _eventService.UpdateEventAsync(invalidId, updateEvent, cancellationToken));
        Assert.Contains($"Событие {invalidId} не найдено", exception.Message);

    }

    [Fact]
    public async Task UpdateEventAsync_WithNullTitle_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createdEvent = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Original Event",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var updateEvent = new UpdateEventDto
        {
            Title = null,
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2)
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _eventService.UpdateEventAsync(createdEvent.Id, updateEvent, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);

    }

    [Fact]
    public async Task UpdateEventAsync_WithPastStartAt_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createdEvent = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Original Event",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var pastDate = DateTime.UtcNow.AddDays(-1);
        var updateEvent = new UpdateEventDto
        {
            Title = "Updated Event",
            StartAt = pastDate,
            EndAt = pastDate.AddHours(2)
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _eventService.UpdateEventAsync(createdEvent.Id, updateEvent, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);

    }

    [Fact]
    public async Task UpdateEventAsync_WithEndAtBeforeStartAt_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createdEvent = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Original Event",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var updateEvent = new UpdateEventDto
        {
            Title = "Updated Event",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(-1)
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _eventService.UpdateEventAsync(createdEvent.Id, updateEvent, cancellationToken));
        Assert.IsType(typeof(ValidationException), exception);
    }

    #endregion

    #region DeleteEventAsync Tests

    [Fact]
    public async Task DeleteEventAsync_WithValidId_ReturnsTrue()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createdEvent = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event to Delete",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        var result = await _eventService.DeleteEventAsync(createdEvent.Id, cancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteEventAsync_WithInvalidId_ReturnsFalse()
    {
        var invalidId = Guid.NewGuid();

        var result = await _eventService.DeleteEventAsync(invalidId, cancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteEventAsync_DeletedEventCannotBeRetrieved()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var createdEvent = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Event to Delete",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = 10,
        }, cancellationToken);

        await _eventService.DeleteEventAsync(createdEvent.Id, cancellationToken);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _eventService.GetEventByIdAsync(createdEvent.Id, cancellationToken));
    }

    #endregion
}
