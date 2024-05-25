//
// @Bellarmine-Head 2024
//

using System;
using System.Collections.Generic;

namespace BellarmineHead.Lfgss.BikeTag.WebScrape;

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
    /// Author name - e.g. Squash.
    /// </summary>
    public String AuthorName { get; set; }

    /// <summary>
    /// The zero or more images in the tag.
    /// </summary>
    public BikeTagPostImage[] Images { get; set; } = [];
}

class BikeTagPostImage
{
    public Uri ImageUri { get; set; }
    public String AltText { get; set; }
    public String Ttitle { get; set; }
}

