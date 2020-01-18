using Google.ReCaptcha;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReCaptchaExtensions
    {
        public static void AddReCaptcha(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var configuration = (IConfiguration)scope.ServiceProvider.GetService(typeof(IConfiguration));

                AddReCaptcha(services, options => 
                {
                    options.Secret = configuration["ReCaptcha:Secret"];
                });
            }
        }

        public static void AddReCaptcha(this IServiceCollection services, Action<ReCaptchaOptions> options)
        {
            services.AddScoped<ReCaptchaOptions>(factory =>
            {
                var opt = new ReCaptchaOptions();

                options(opt);

                return opt;
            });

            services.AddHttpClient(GoogleHttpClientName.Name, (serviceProvider, client) =>
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var recaptchaOptions = (ReCaptchaOptions)scope.ServiceProvider.GetService(typeof(ReCaptchaOptions));

                    client.BaseAddress = new Uri("https://www.google.com/recaptcha/api/siteverify");
                }
            });

            services.AddScoped<IReCaptchaService, ReCaptchaService>();
        }
    }
}
