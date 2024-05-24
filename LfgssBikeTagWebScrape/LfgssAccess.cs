//
// @Bellarmine-Head 2024
//

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using static System.FormattableString;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

/// <summary>
/// Talks to the LFGSS website.
/// </summary>
static class LfgssAccess
{
    /// <summary>
    /// Gets the HTML text for the specified bike tag page.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the page number is too high (i.e. the page doesn't exist), LFGSS will return a status of 500.  But it will also do so for
    /// other internal errors, so we can't rely on 500 as meaning "page doesn't exist" to the exclusion of all other errors.
    /// </para>
    /// </remarks>
    /// <param name="httpClient">
    /// The HTTP client to use.
    /// </param>
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
    public static async Task<String> GetBikeTagPageHtmlAsync(HttpClient httpClient, Int32 page, CancellationToken ct)
    {
        if (page < 1)
            page = 1;

        var offset = (page - 1) * 25;

        var response = await httpClient.GetAsync(Invariant($"?offset={offset}"), ct);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}
