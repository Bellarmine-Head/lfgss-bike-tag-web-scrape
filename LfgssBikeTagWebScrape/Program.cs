//
// @Bellarmine-Head 2024
//

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

static class Program
{
    // Entry point.
    static async Task Main(String[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var httpClientBuilder = builder.Services.AddHttpClient<LfgssHttpClient>(
            configureClient: static client =>
            {
                client.BaseAddress = new Uri("https://www.lfgss.com/");
            });

        builder.Services.AddHostedService<WorkerService>();

        builder.Configuration.AddCommandLine(args);

        var host = builder.Build();

        await host.RunAsync();
    }
}
