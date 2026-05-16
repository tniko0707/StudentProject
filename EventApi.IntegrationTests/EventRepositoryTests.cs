
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Project.DataAccess;
using Project.Models;
using Project.Repositories;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests
{
    public class EventRepositoryTests: IAsyncLifetime
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
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            NpgsqlConnection.ClearAllPools();
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        [Fact]
        public async Task CreateEvent()
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
    }
}
