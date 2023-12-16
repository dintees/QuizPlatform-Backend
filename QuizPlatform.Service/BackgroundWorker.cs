using Microsoft.Extensions.Hosting;

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
            Console.WriteLine("Service has been started");
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

                if (now.Hour == scheduledTime.Hour)
                {
                    Console.WriteLine("Starting executing tasks...");

                    await _databaseCleaner.CleanUserTokensEntityAsync();

                    Console.WriteLine("Completed all tasks successfully.");
                }


                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
