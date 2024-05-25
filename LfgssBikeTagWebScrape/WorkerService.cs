//
// @Bellarmine-Head 2024
//

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

// The meat of the application.
internal sealed class WorkerService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly Int32 _startPage;
    private readonly Int32 _numPages;

    // Constructor.
    public WorkerService(ILogger<WorkerService> logger, IHostApplicationLifetime appLifetime, IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _httpClientFactory = httpClientFactory;

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
        Console.Out.WriteLine($"Start page: {_startPage}, number of pages: {_numPages}");

        await ScrapePagesAsync(ct);
    }

    // Service stopped.
    Task IHostedService.StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    // This is what the app is for.
    private async Task ScrapePagesAsync(CancellationToken ct)
    {
        // Results list.
        List<BikeTagPage> resultsPages = [];


        // For all of the pages...
        String html;
        HttpClient httpClient;
        BikeTagPage bikeTagpage;
        for (var page = _startPage; page < (_startPage + _numPages); ++page)
        {
            if (ct.IsCancellationRequested)
                break;


            // Get the HTML for the first/next page.
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1.0), ct);    // a short delay between requests; don't overload LFGSS

                httpClient = _httpClientFactory.CreateClient("brightonBikeTagHttpClient");
                html = await LfgssAccess.GetBikeTagPageHtmlAsync(httpClient, page, ct);
            }
            catch (HttpRequestException)
            {
                // for the time being we'll regard this as: page not found (i.e. we've gone too high in the page numbers)
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get a page of HTML. {Error}", ex.Message);
                _appLifetime.StopApplication();
                return;
            }

            if (ct.IsCancellationRequested)
                break;


            // Parse into a BikeTagPage instance.
            try
            {
                bikeTagpage = await HtmlParser.ParsePageAsync(html, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get parse a page of HTML. {Error}", ex.Message);
                _appLifetime.StopApplication();
                return;
            }


            // Add this page to the results list.
            resultsPages.Add(bikeTagpage);

            if (ct.IsCancellationRequested)
                break;
        }


        // Save the results.
        if (ct.IsCancellationRequested is false)
        {
            // todo: write `resultsPages` to file as JSON
        }


        // Nothing more to do.
        _appLifetime.StopApplication();
    }
}
