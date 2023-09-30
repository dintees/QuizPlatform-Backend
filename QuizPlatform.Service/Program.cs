using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizPlatform.Infrastructure;
using QuizPlatform.Infrastructure.Entities;

namespace QuizPlatform.Service
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<IDatabaseCleaner, DatabaseCleaner>();
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QuizPlatform;Trusted_Connection=True;");
                    });
                    services.AddInfrastructure();

                    services.AddHostedService<BackgroundWorker>();
                });

            await builder.RunConsoleAsync();
        }
    }
}