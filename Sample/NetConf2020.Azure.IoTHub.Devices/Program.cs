using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NetConf2020.Azure.IoTHub.Devices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            host.Services.GetRequiredService<App>().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<App>();
                });
    }
}
