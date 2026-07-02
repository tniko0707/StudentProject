
using Application.Services;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests
{
    public class EventRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("test")
            .Build();
        public async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
            // Применяем миграции один раз при старте контейнера
            await using var context = CreateContext();
            await context.Database.MigrateAsync();
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            var context = new AppDbContext(options);
            //context.Database.EnsureCreated();
            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            NpgsqlConnection.ClearAllPools();
            await using var context = CreateContext();
            //await context.Database.EnsureDeletedAsync();
            //await context.Database.EnsureCreatedAsync();
            // Очищаем данные, но НЕ удаляем схему (иначе потеряем миграции)
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE bookings, events RESTART IDENTITY CASCADE;");
        }

        [Fact]
        public async Task Migrations_Should_Be_Applied()
        {
            await using var context = CreateContext();

            var applied = await context.Database.GetAppliedMigrationsAsync();
            var pending = await context.Database.GetPendingMigrationsAsync();

            Assert.NotEmpty(applied);
            Assert.Empty(pending);
        }


        [Fact]
        public async Task CreateEvent()
        {
            await ResetDatabaseAsync();

            // arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);

            var repository = new EventRepository(context);
            User user = new User()
            {
                UserId = Guid.NewGuid(),
                Login = "log",
                PasswordHash = PasswordHasher.HashPassword("abracadabra"),
                Role = Role.Admin,
                Bookings = new List<Booking>()
            };
            UserRepository userRepository = new UserRepository(context);
            await userRepository.AddAsync(user);

            //act
            await repository.AddAsync(evente);

            //assert
            await using var testContext = CreateContext();
            var savedEvent = testContext.Events.SingleOrDefault();
            Assert.NotNull(savedEvent);
            Assert.Equal("Test", savedEvent.Title);
        }

        [Fact]
        public async Task FindById_ReturnsEvent()
        {
            await ResetDatabaseAsync();

            // arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);

            var repository = new EventRepository(context);

            //act
            await repository.AddAsync(evente);

            //assert
            await using var testContext = CreateContext();
            var testRepository = new EventRepository(context);

            Event savedEvent = await testRepository.FindByIdAsync(evente.Id);
            Assert.NotNull(savedEvent);
            Assert.Equal("Test", savedEvent.Title);

        }

        [Fact]
        public async Task RemoveEvent_DeleteEvent()
        {
            await ResetDatabaseAsync();

            // arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);

            var repository = new EventRepository(context);

            //act
            await repository.AddAsync(evente);

            //assert
            await using var testContext = CreateContext();
            var testRepository = new EventRepository(context);

            await testRepository.RemoveEvent(evente);
            Event savedEvent = await testRepository.FindByIdAsync(evente.Id);
            Assert.Null(savedEvent);

        }

        [Fact]
        public async Task GetAll_ReturnsEvents()
        {
            await ResetDatabaseAsync();

            // arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            Event evente2 = new Event("Test2", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);

            var repository = new EventRepository(context);

            //act
            await repository.AddAsync(evente);
            await repository.AddAsync(evente2);

            //assert
            await using var testContext = CreateContext();
            var testRepository = new EventRepository(context);

            var events = await testRepository.GetAll();
            Assert.Equal(2, events.Count());

        }

        [Fact]
        public async Task GetLast()
        {
            await ResetDatabaseAsync();

            // arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            Event evente2 = new Event("Test2", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);

            var repository = new EventRepository(context);

            //act
            await repository.AddAsync(evente);
            await repository.AddAsync(evente2);

            //assert
            await using var testContext = CreateContext();
            var repositoryTest = new EventRepository(context);

            var savedEvent = await repositoryTest.GetLast();
            Assert.NotNull(savedEvent);
            Assert.Equal("Test2", savedEvent.Title);
        }

        [Fact]
        public async Task SaveChangesTest()
        {
            await ResetDatabaseAsync();

            // arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            Event evente2 = new Event("Test2", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);

            var repository = new EventRepository(context);

            //act
            await repository.AddAsync(evente);
            await repository.SaveChangesAsync();
            var counterAll = await repository.GetAll();
            await repository.AddAsync(evente2);

            //assert
            await using var testContext = CreateContext();
            var repositoryTest = new EventRepository(context);

            var savedEvents = await repositoryTest.GetAll();
            Assert.NotNull(savedEvents);
            Assert.NotEqual(counterAll, savedEvents);
        }
        [Fact]
        public async Task FilterTest()
        {
            await ResetDatabaseAsync();

            // arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10);
            Event evente2 = new Event("Test2", "Descr", DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(4), 10);

            var repository = new EventRepository(context);

            //act
            await repository.AddAsync(evente);
            await repository.AddAsync(evente2);

            //assert
            await using var testContext = CreateContext();
            var repositoryTest = new EventRepository(context);
            var eventService = new EventService(repositoryTest);
            var testEvent = await eventService.GetFilteredEventsAsync(
                title: "Test",
                from: DateTime.UtcNow.AddDays(1.5),
                to: DateTime.UtcNow.AddDays(5),
                page: 1,
                pageSize: 10);
            Assert.Single(testEvent.Events.ToList());
        }

    }
}
