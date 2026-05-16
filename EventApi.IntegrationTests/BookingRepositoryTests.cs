
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Project.DataAccess;
using Project.Models;
using Project.Repositories;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests
{
    public class BookingRepositoryTests: IAsyncLifetime
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
        public async Task CreateFindingBooking_ReturnBooking()
        {
            await ResetDatabaseAsync();

            //arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            EventRepository eventRepository = new EventRepository(context);
            await eventRepository.AddAsync(evente);
            
            //act
            await using var bookingContext = CreateContext();
            Booking booking = Booking.CreatePending(evente.Id);
            BookingRepository bookingRepository = new BookingRepository(bookingContext);
            await bookingRepository.AddAsync(booking);

            //assert
            await using var checkingContext = CreateContext();
            BookingRepository testRepo = new BookingRepository(bookingContext);
            Booking? bookingCheck = await testRepo.FindByIdAsync(booking.Id);
            Assert.NotNull(bookingCheck);
        }

        [Fact]
        public async Task ConfirmBooking()
        {
            await ResetDatabaseAsync();

            //arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            EventRepository eventRepository = new EventRepository(context);
            await eventRepository.AddAsync(evente);

            //act
            await using var bookingContext = CreateContext();
            Booking booking = Booking.CreatePending(evente.Id);
            BookingRepository bookingRepository = new BookingRepository(bookingContext);
            await bookingRepository.AddAsync(booking);

            await bookingRepository.ConfirmBookingAsync(booking.Id);

            //assert
            await using var checkingCOntext = CreateContext();
            BookingRepository repository = new BookingRepository(checkingCOntext);
            Booking? bookingCheck = await repository.FindByIdAsync(booking.Id);
            Assert.True(bookingCheck?.Status == BookingStatus.Confirmed);


        }
    }
}
