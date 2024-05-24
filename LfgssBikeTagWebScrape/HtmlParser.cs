//
// @Bellarmine-Head 2024
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AngleSharp;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

/// <summary>
/// Parses HTML pages from LFGSS.
/// </summary>
static class HtmlParser
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="html">
    /// </param>
    /// <param name="ct">
    /// </param>
    /// <returns>
    /// </returns>
    public static async Task ParsePageAsync(String html, CancellationToken ct)
    {
        var context = BrowsingContext.New(Configuration.Default);

        var document = await context.OpenAsync(req => req.Content(html), ct);

        // each page SHOULD contain one <ul class="list-comments"> ... </ul> list with 25 <li> items - these are where the tags are,
        // although some "tags" just contain text - no photos
        var pageCommentsList = document.All
            .Where(elem => elem.LocalName == "ul" && elem.ClassList.Contains("list-comments", StringComparer.Ordinal))
            .FirstOrDefault(ulList => ulList.ChildElementCount == 25);

        if (pageCommentsList is null)
        {
            // throw exn
        }

        // got to here
    }
}
