//
// @Bellarmine-Head 2024
//

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using static System.FormattableString;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

// Talks to the LFGSS website.
internal sealed class LfgssHttpClient
{
    private readonly HttpClient _httpClient;

    // Constructor.
    public LfgssHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the HTML text for the specified bike tag page.
    /// </summary>
    /// <param name="page">
    /// 1-based page number.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// HTML text.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the request wasn't successful.
    /// </exception>
    public async Task<String> GetBikeTagPageHtmlAsync(Int32 page, CancellationToken ct)
    {
        if (page < 1)
            page = 1;

        var offset = (page - 1) * 25;

        var response = await _httpClient.GetAsync(Invariant($"?offset={offset}"), ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}
