using Microsoft.Extensions.Hosting;
using QuizPlatform.Infrastructure.Interfaces;

namespace QuizPlatform.Service
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly IDatabaseCleaner _databaseCleaner;

        public BackgroundWorker(IDatabaseCleaner databaseCleaner)
        {
            _databaseCleaner = databaseCleaner;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 18, 0, 0);

                if (now.Hour == scheduledTime.Hour)
                {
                    Console.WriteLine("Service has been started.");

                    await _databaseCleaner.CleanUserTokensEntity();

                    Console.WriteLine("Completed all tasks successfully.");
                }


                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
