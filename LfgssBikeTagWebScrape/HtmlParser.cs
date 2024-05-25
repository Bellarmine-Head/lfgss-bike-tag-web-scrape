//
// @Bellarmine-Head 2024
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Text;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

/// <summary>
/// Parses HTML pages from LFGSS.
/// </summary>
static class HtmlParser
{
    private const String LfgssBaseAddress = "https://www.lfgss.com";

    /// <summary>
    /// Parses the HTML of the specified LFGSS bike tag page, returning an instance of <see cref="BikeTagPage"/> which will contain
    /// an array of up to 25 posts.
    /// </summary>
    /// <param name="html">
    /// The HTML text of the bike tag page.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A fully-populated instance of <see cref="BikeTagPage"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="html"/> doesn't contain the HTML for an LFGSS bike tag page.
    /// </exception>
    public static async Task<BikeTagPage> ParsePageAsync(String html, CancellationToken ct)
    {
        // Parse into AngleSharp.
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


        // Get the page's number and URL.
        Uri pageUrl;
        Int32 pageNumber;
        try
        {
            (pageNumber, pageUrl) = getPageNumberAndUrl();
        }
        catch (Exception)
        {
            throw new ArgumentException("The specified HTML doesn't represent an LFGSS bike tag page.", nameof(html));
        }


        // Get the posts.
        BikeTagPost[] posts;
        try
        {
            posts = pageCommentsList.Children.Where(elem => elem.LocalName == "li").Select(ParseListComment).ToArray();
        }
        catch (Exception)
        {
            throw new ArgumentException("The specified HTML doesn't represent an LFGSS bike tag page.", nameof(html));
        }


        // Return.
        return new BikeTagPage
        {
            PageNumber = pageNumber,
            PageUrl = pageUrl,
            Posts = posts
        };


        // local function; can throw exception
        (Int32, Uri) getPageNumberAndUrl()
        {
            var aTag = document.QuerySelector("ul.pagination").QuerySelector("li.active").QuerySelector("a");

            var link = LfgssBaseAddress + aTag.Attributes["href"].Value;    // the hrefs all start with "/"

            var uri = new Uri(link);

            var pageNumber = Int32.Parse(aTag.Text(), NumberStyles.Integer, CultureInfo.InvariantCulture);

            return (pageNumber, uri);
        }
    }

    /// <summary>
    /// Parses a list element that contains one of up to 25 comments on a bike tag page.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can throw one of a number of exceptions.
    /// </para>
    /// </remarks>
    /// <param name="elem">
    /// The list ("li") element.
    /// </param>
    /// <returns>
    /// A fully-populated <see cref="BikeTagPost"/>.
    /// </returns>
    private static BikeTagPost ParseListComment(IElement liElem)
    {
        var commentBody = liElem.QuerySelector("div.comment-item-body");

        var imageAttachments = commentBody.QuerySelectorAll("li.attachment-image");

        (var postNumber, var postUrl) = getPostNumberAndUrl();

        var authorName = getAuthorName();

        var images = imageAttachments.Select(getImage).ToArray();

        return new BikeTagPost
        {
            PostNumber = postNumber,
            PostUrl = postUrl,
            AuthorName = authorName,
            Images = images
        };


        // local functions
        (Int32, Uri) getPostNumberAndUrl()
        {
            var permaLinkElem = liElem.QuerySelector("div.comment-item-permalink");

            var aTag = permaLinkElem.QuerySelectorAll("a").First();

            var link = LfgssBaseAddress + aTag.Attributes["href"].Value;    // the hrefs all start with "/"

            var uri = new Uri(link);

            var postNumber = Int32.Parse(aTag.Text()[1..],                  // e.g. "#2989" -> "2989"
                NumberStyles.Integer, CultureInfo.InvariantCulture);

            return (postNumber, uri);
        }

        String getAuthorName() => liElem.QuerySelector("strong.comment-item-author-name").Text().NormalizeWhitespace();

        static BikeTagPostImage getImage(IElement liAttachElem)
        {
            var imgElemAttrs = liAttachElem.QuerySelector("img").Attributes;

            return new BikeTagPostImage
            {
                ImageUri = new Uri(imgElemAttrs["src"].Value),
                AltText = imgElemAttrs["alt"]?.Value ?? String.Empty,
                Ttitle = imgElemAttrs["title"]?.Value ?? String.Empty
            };
        }
    }

    /// <summary>
    /// Returns a version of the specified text in which all whitespace characters are normalized to Unicode 32 (normal space).
    /// </summary>
    private static String NormalizeWhitespace(this String text)
    {
        if (text is null)
            return null;

        return new String(text.Select(normalize).ToArray());

        static Char normalize(Char ch) => ch.IsWhiteSpaceCharacter() ? ' ' : ch;
    }
}
