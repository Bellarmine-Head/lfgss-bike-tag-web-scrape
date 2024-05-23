//
// @Bellarmine-Head 2024
//

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

internal sealed class WorkerService : IHostedService //, IHostedLifecycleService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly LfgssHttpClient _lfgssHttpClient;

    public WorkerService(ILogger<WorkerService> logger, IHostApplicationLifetime appLifetime, LfgssHttpClient lfgssHttpClient)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _lfgssHttpClient = lfgssHttpClient;
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("2. StartAsync has been called.");

        var html = await _lfgssHttpClient.GetHomePageHtmlAsync();

        _logger.LogInformation("{Html}", html);

        _appLifetime.StopApplication();
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
