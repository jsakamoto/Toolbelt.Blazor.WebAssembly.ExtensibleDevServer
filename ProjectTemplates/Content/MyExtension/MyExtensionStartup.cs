using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MyExtension;

[assembly: HostingStartup(typeof(MyExtensionStartup))]

namespace MyExtension;

public class MyExtensionStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Register the startup filter
            services.AddSingleton<IStartupFilter, MyExtensionStartupFilter>();

            // You can register other services here as needed
        });
    }
}
