using Microsoft.Extensions.Hosting;

namespace QuizPlatform.Service
{
    public class BackgroundWorker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0);

                if (now.Hour == scheduledTime.Hour)
                {
                    Console.WriteLine("Hello world!");
                }

                Console.WriteLine("aaaa");

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
