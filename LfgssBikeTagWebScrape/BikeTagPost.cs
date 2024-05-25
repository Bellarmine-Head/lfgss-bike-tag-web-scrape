//
// @Bellarmine-Head 2024
//

using System;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

/// <summary>
/// Represents a bike tag page.
/// </summary>
class BikeTagPage
{
    /// <summary>
    /// Page number: 1, 2, 3...
    /// </summary>
    public required Int32 PageNumber { get; init; }

    /// <summary>
    /// Page URL.
    /// </summary>
    public required Uri PageUrl { get; init; }

    /// <summary>
    /// 1 - 25 posts on the page.
    /// </summary>
    public required BikeTagPost[] Posts { get; init; }
}

/// <summary>
/// Represents a bike tag post (aka a comment).
/// </summary>
/// <remarks>
/// <para>
/// Only the posts that contain one or more images are (potentially) genuine "tags".  To determine what are tags and what aren't,
/// human judgement is called for.
/// </para>
/// </remarks>
class BikeTagPost
{
    /// <summary>
    /// Overall post number, across all pages.  1, 2, 3...
    /// </summary>
    public required Int32 PostNumber { get; init; }

    /// <summary>
    /// URL to this particular post.
    /// </summary>
    public required Uri PostUrl { get; init; }

    /// <summary>
    /// Author name - e.g. Squash.
    /// </summary>
    public required String AuthorName { get; init; }

    /// <summary>
    /// The zero or more images in the tag.
    /// </summary>
    public BikeTagPostImage[] Images { get; init; } = [];
}

class BikeTagPostImage
{
    public required Uri ImageUri { get; init; }
    public required String AltText { get; init; }
    public required String Ttitle { get; init; }
}
