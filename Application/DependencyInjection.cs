using Application.Repositories;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IUserService, UserService>();

            services.AddHostedService<BookingBackgroundService>();
            return services;
        }
    }
}
