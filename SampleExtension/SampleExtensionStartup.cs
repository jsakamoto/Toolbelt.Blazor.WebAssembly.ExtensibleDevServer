using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.WebAssembly.ExtensibleDevServer.SampleExtension;

[assembly: HostingStartup(typeof(SampleExtensionStartup))]

namespace Toolbelt.Blazor.WebAssembly.ExtensibleDevServer.SampleExtension;

public class SampleExtensionStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Register the startup filter
            services.AddSingleton<IStartupFilter, SampleExtensionStartupFilter>();

            // You can register other services here as needed
        });
    }
}
