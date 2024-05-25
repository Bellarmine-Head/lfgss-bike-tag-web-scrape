//
// @Bellarmine-Head 2024
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Text;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

/// <summary>
/// Parses HTML pages from LFGSS.
/// </summary>
static class HtmlParser
{
    /// <summary>
    /// Parses the HTML of the specified LFGSS bike tag page, returning an array of up to 25 posts.
    /// </summary>
    /// <param name="html">
    /// The HTML text of the bike tag page.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// An array of 1 to 25 bike tag posts.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="html"/> doesn't contain the HTML for an LFGSS bike tag page.
    /// </exception>
    public static async Task<BikeTagPost[]> ParsePageAsync(String html, CancellationToken ct)
    {
        var context = BrowsingContext.New(Configuration.Default);

        var document = await context.OpenAsync(req => req.Content(html), ct);

        var cs = StringComparer.Ordinal;

        // each page SHOULD contain one <ul class="list-comments"> ... </ul> list with up to 25 <li> items in the content body -
        // these are where the posts are, some of which contain images (and some of those images are genuine tags)
        var pageCommentsList = document.All
            .Where(elem => elem.LocalName == "div" && elem.ClassList.Contains("content-body", cs))
            .SelectMany(elem => elem.Children)
            .Where(elem => elem.LocalName == "ul" && elem.ClassList.Contains("list-comments", cs))
            .FirstOrDefault();

        if (pageCommentsList is null)
            throw new ArgumentException("The specified HTML doesn't represent an LFGSS bike tag page.", nameof(html));


        return pageCommentsList.Children.Where(elem => elem.LocalName == "li").Select(ParseListComment).ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elem">
    /// </param>
    /// <returns>
    /// </returns>
    private static BikeTagPost ParseListComment(IElement liElem)
    {
        var commentBody = liElem.QuerySelector("div.comment-item-body");

        var imageAttachments = commentBody.QuerySelectorAll("li.attachment-image");

        return new BikeTagPost
        {
            AuthorName = getAuthorName(),
            Images = imageAttachments.Select(getImage).ToArray()
        };


        String getAuthorName() => liElem.QuerySelector("strong.comment-item-author-name").Text().NormalizeWhitespace();

        BikeTagPostImage getImage(IElement liAttachElem)
        {
            var imgElemAttrs = liAttachElem.QuerySelector("img").Attributes;

            return new BikeTagPostImage
            {
                ImageUri = new Uri(imgElemAttrs["src"].Value),
                AltText = imgElemAttrs["alt"].Value,
                Ttitle = imgElemAttrs["title"].Value
            };
        }
    }

    private static String NormalizeWhitespace(this String text)
    {
        if (text is null)
            return null;

        return new String(text.Select(normalize).ToArray());

        static Char normalize(Char ch) => ch.IsWhiteSpaceCharacter() ? ' ' : ch;
    }
}
