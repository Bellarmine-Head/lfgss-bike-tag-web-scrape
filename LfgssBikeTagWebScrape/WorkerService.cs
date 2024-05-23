//
// @Bellarmine-Head 2024
//

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

// The meat of the application.
internal sealed class WorkerService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly LfgssHttpClient _lfgssHttpClient;

    private readonly Int32 _startPage;
    private readonly Int32 _numPages;

    // Constructor.
    public WorkerService(ILogger<WorkerService> logger, IHostApplicationLifetime appLifetime, IConfiguration config,
        LfgssHttpClient lfgssHttpClient)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _lfgssHttpClient = lfgssHttpClient;     // should we get this via the ctor, or should we query for it before each request?

        _startPage = 1;
        _numPages = 1;

        if (Int32.TryParse(config["startPage"], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 startPageInt))
        {
            if (startPageInt < 1)
                startPageInt = 1;

            _startPage = startPageInt;
        }

        if (Int32.TryParse(config["numPages"], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 numPagesInt))
        {
            if (numPagesInt < 1)
                numPagesInt = 1;

            _numPages = numPagesInt;
        }
    }

    // Service started.
    async Task IHostedService.StartAsync(CancellationToken ct)
    {
        for (var page = _startPage; page < (_startPage + _numPages); ++page)
        {
            if (ct.IsCancellationRequested)
                break;

            var html = await _lfgssHttpClient.GetBikeTagPageHtmlAsync(page, ct);

            if (ct.IsCancellationRequested)
                break;

            _logger.LogInformation("{Html}", html[..100]);

            // the 25 posts per page are <li> items within the first <ul class="list-comments"> ... </ul> on each page
        }

        _appLifetime.StopApplication();
    }

    // Service stopped.
    Task IHostedService.StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
