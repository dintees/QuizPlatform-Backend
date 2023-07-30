namespace QuizPlatform.API.Extensions
{
    internal static class ServicesExtensions
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddCors(opts =>
            {
                opts.AddPolicy("AllowAnyonePolicy",
                    policyOpts => { policyOpts.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin(); });
            });
        }
    }
}
