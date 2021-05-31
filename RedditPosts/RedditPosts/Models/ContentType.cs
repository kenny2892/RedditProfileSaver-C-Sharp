using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.Models
{
    public enum ContentType
    {
        Mp4,
        Twitter,
        Youtube,
        Image,
        [Description("Imgur Image")]
        ImgurImage,
        Gif,
        Gifv,
        [Description("Imgur Gallery")]
        ImgurGallery,
        GfyCat,
        [Description("RedGif Watch Link")]
        RedGifWatch,
        [Description("Rule 34 Video")]
        R34Video,
        Gallery,
        Vreddit,
        [Description("Vreddit Post")]
        VredditPostOnly,
        [Description("Url")]
        UrlPreview,
        Blank
    }
}
