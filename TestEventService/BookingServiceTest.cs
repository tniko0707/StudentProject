namespace TestEventService;

using Application.DTO;
using Application.Repositories;
using Application.Services;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class BookingServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;
    private readonly CancellationToken cancellationToken;
    private readonly User _user;
    public BookingServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
        _userService = _scope.ServiceProvider.GetRequiredService<IUserService>();

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

    private async Task<Guid> CreateTestEventAsync(int totalSeats = 10)
    {
        var futureDate = DateTime.UtcNow.AddDays(1);
        var created = await _eventService.CreateEventAsync(new CreateEventDto
        {
            Title = "Test Event",
            StartAt = futureDate,
            EndAt = futureDate.AddHours(2),
            TotalSeats = totalSeats
        }, cancellationToken);
        return created.Id;
    }

    #region CreateBookingAsync Tests

    [Fact]
    public async Task CreateBookingAsync_WithValidEventId_ReturnsBookingInfoWithPendingStatus()
    {


        var eventId = await CreateTestEventAsync();
        var result = await _bookingService.CreateBookingAsync(_user, eventId, cancellationToken);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Null(result.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_WithValidEventId_SetsCreatedAt()
    {
        var eventId = await CreateTestEventAsync();
        var before = DateTime.UtcNow;

        var result = await _bookingService.CreateBookingAsync(_user, eventId, cancellationToken);

        var after = DateTime.UtcNow;
        Assert.InRange(result.CreatedAt, before, after);
    }

    [Fact]
    public async Task CreateBookingAsync_WithNonExistentEvent_ThrowsNotFoundException()
    {
        var invalidEventId = Guid.NewGuid();
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _bookingService.CreateBookingAsync(_user, invalidEventId, cancellationToken));
        Assert.Equal($"Событие {invalidEventId} не найдено", exception.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_MultipleBookingsForSameEvent_AllCreatedWithUniqueIds()
    {
        var eventId = await CreateTestEventAsync(totalSeats: 5);

        var results = new List<Booking>();
        for (int i = 0; i < 5; i++)
            results.Add(await _bookingService.CreateBookingAsync(_user, eventId, cancellationToken));

        var uniqueIds = results.Select(r => r.Id).Distinct();
        Assert.Equal(5, uniqueIds.Count());
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNoSeatsAvailable_ThrowsNoAvailableSeatsException()
    {
        var eventId = await CreateTestEventAsync(totalSeats: 1);
        await _bookingService.CreateBookingAsync(_user, eventId, cancellationToken);

        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => _bookingService.CreateBookingAsync(_user, eventId, cancellationToken));
    }

    [Fact]
    public async Task CreateBookingAsync_DecrementsAvailableSeats()
    {
        var eventId = await CreateTestEventAsync(totalSeats: 3);

        await _bookingService.CreateBookingAsync(_user, eventId, cancellationToken);
        await _bookingService.CreateBookingAsync(_user, eventId, cancellationToken);

        var eventInfo = await _eventService.GetEventByIdAsync(eventId, cancellationToken);
        Assert.Equal(1, eventInfo.AvailableSeats);
    }

    #endregion

    #region GetBookingByIdAsync Tests

    [Fact]
    public async Task GetBookingByIdAsync_WithValidId_ReturnsCorrectBookingInfo()
    {
        var eventId = await CreateTestEventAsync();
        var created = await _bookingService.CreateBookingAsync(_user, eventId, cancellationToken);

        var result = await _bookingService.GetBookingByIdAsync(_user, created.Id, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.EventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        var invalidId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _bookingService.GetBookingByIdAsync(_user, invalidId, cancellationToken));
        Assert.Equal($"Событие {invalidId} не найдено", exception.Message);
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_DoesNotOverbookEvent()
    {
        const int totalSeats = 5;
        const int concurrentRequests = 20;
        var eventId = await CreateTestEventAsync(totalSeats: totalSeats);

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                try
                {
                    await bookingService.CreateBookingAsync(_user, eventId, cancellationToken);
                    return true;
                }
                catch (NoAvailableSeatsException)
                {
                    return false;
                }
            }));

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r);
        Assert.Equal(totalSeats, successCount);
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_AllSuccessfulBookingsHaveUniqueIds()
    {
        const int totalSeats = 10;
        const int concurrentRequests = 10;
        var eventId = await CreateTestEventAsync(totalSeats: totalSeats);
        var bookingIds = new System.Collections.Concurrent.ConcurrentBag<Guid>();

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                var booking = await bookingService.CreateBookingAsync(_user, eventId, cancellationToken);
                bookingIds.Add(booking.Id);
            }));

        await Task.WhenAll(tasks);

        Assert.Equal(totalSeats, bookingIds.Distinct().Count());
    }

    #endregion
}
