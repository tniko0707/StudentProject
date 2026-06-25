
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests
{
    public class BookingRepositoryTests : IAsyncLifetime
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
        public async Task CreateFindingBooking_ReturnBooking()
        {
            await ResetDatabaseAsync();

            //arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            EventRepository eventRepository = new EventRepository(context);
            await eventRepository.AddAsync(evente);
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

            await using var bookingContext = CreateContext();
            Booking booking = Booking.CreatePending(user.UserId, evente.Id);
            BookingRepository bookingRepository = new BookingRepository(bookingContext);
            await bookingRepository.AddAsync(booking);

            //assert
            await using var checkingContext = CreateContext();
            BookingRepository testRepo = new BookingRepository(bookingContext);
            Booking? bookingCheck = await testRepo.FindByIdAsync(booking.Id);
            Assert.NotNull(bookingCheck);
        }

        [Fact]
        public async Task LimitBookings()
        {
            await ResetDatabaseAsync();

            //arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            EventRepository eventRepository = new EventRepository(context);
            await eventRepository.AddAsync(evente);
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
            await using var bookingContext = CreateContext();
            BookingRepository bookingRepository = new BookingRepository(bookingContext);
            for (int i = 0; i < 9;  i++)
            {
                Booking booking = Booking.CreatePending(user.UserId, evente.Id);
                await bookingRepository.AddAsync(booking);
            }

            await

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
            await using var bookingContext = CreateContext();
            Booking booking = Booking.CreatePending(user.UserId, evente.Id);
            BookingRepository bookingRepository = new BookingRepository(bookingContext);
            await bookingRepository.AddAsync(booking);

            await bookingRepository.ConfirmBookingAsync(booking.Id);

            //assert
            await using var checkingCOntext = CreateContext();
            BookingRepository repository = new BookingRepository(checkingCOntext);
            Booking? bookingCheck = await repository.FindByIdAsync(booking.Id);
            Assert.True(bookingCheck?.Status == BookingStatus.Confirmed);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsBookings()
        {
            await ResetDatabaseAsync();

            //arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            EventRepository eventRepository = new EventRepository(context);
            await eventRepository.AddAsync(evente);
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
            await using var contextB = CreateContext();
            BookingRepository bookingRepository = new BookingRepository(contextB);
            Booking booking = Booking.CreatePending(user.UserId, evente.Id);
            Booking booking2 = Booking.CreatePending(user.UserId, evente.Id);

            await bookingRepository.AddAsync(booking);
            await bookingRepository.AddAsync(booking2);

            //assert
            await using var checkingCOntext = CreateContext();
            BookingRepository repository = new BookingRepository(checkingCOntext);
            var bookings = await repository.GetAllAsync();
            Assert.Equal(2, bookings.Count);
        }

        [Fact]
        public async Task GetAllPendingAsync_ReturnsSingleBooking()
        {
            await ResetDatabaseAsync();

            //arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            EventRepository eventRepository = new EventRepository(context);
            await eventRepository.AddAsync(evente);
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
            await using var contextB = CreateContext();
            BookingRepository bookingRepository = new BookingRepository(contextB);
            Booking booking = Booking.CreatePending(user.UserId, evente.Id);
            Booking booking2 = Booking.CreatePending(user.UserId, evente.Id);

            await bookingRepository.AddAsync(booking);
            await bookingRepository.AddAsync(booking2);
            await bookingRepository.ConfirmBookingAsync(booking2.Id);

            //assert
            await using var checkingCOntext = CreateContext();
            BookingRepository repository = new BookingRepository(checkingCOntext);
            var bookings = await repository.GetAllPendingAsync();
            Assert.Single(bookings);
        }
        [Fact]
        public async Task GetLastBookingAsync_ReturnsBooking()
        {
            await ResetDatabaseAsync();

            //arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            EventRepository eventRepository = new EventRepository(context);
            await eventRepository.AddAsync(evente);
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
            await using var contextB = CreateContext();
            BookingRepository bookingRepository = new BookingRepository(contextB);
            Booking booking = Booking.CreatePending(user.UserId, evente.Id);
            await Task.Delay(2000);
            Booking booking2 = Booking.CreatePending(user.UserId, evente.Id);

            await bookingRepository.AddAsync(booking);
            await bookingRepository.AddAsync(booking2);
            await bookingRepository.ConfirmBookingAsync(booking2.Id);

            //assert
            await using var checkingCOntext = CreateContext();
            BookingRepository repository = new BookingRepository(checkingCOntext);
            var bookingLast = await repository.GetLastBookingAsync();
            Assert.True(bookingLast.Id.Equals(booking2.Id));
        }
        [Fact]
        public async Task DeleteDataFromDb()
        {
            await ResetDatabaseAsync();

            //arrange
            await using var context = CreateContext();
            Event evente = new Event("Test", "Descr", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            EventRepository eventRepository = new EventRepository(context);
            await eventRepository.AddAsync(evente);
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
            await using var contextB = CreateContext();
            BookingRepository bookingRepository = new BookingRepository(contextB);
            Booking booking = Booking.CreatePending(user.UserId, evente.Id);
            Booking booking2 = Booking.CreatePending(user.UserId, evente.Id);

            await bookingRepository.AddAsync(booking);
            await bookingRepository.AddAsync(booking2);
            await bookingRepository.ConfirmBookingAsync(booking2.Id);

            //assert
            await using var checkingCOntext = CreateContext();
            BookingRepository repository = new BookingRepository(checkingCOntext);
            await repository.DeleteDataFromTable();
            var all = await repository.GetAllAsync();
            Assert.Empty(all);
        }

    }
}
