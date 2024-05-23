//
// @Bellarmine-Head 2024
//

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

internal sealed class LfgssHttpClient
{
    private readonly HttpClient _httpClient;

    public LfgssHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<String> GetHomePageHtmlAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "");

        request.Headers.Add("User-Agent", "Chrome/124.0.0.0");      // without User-Agent the response will be 403
        request.Headers.Add("Accept", "text/html");

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
