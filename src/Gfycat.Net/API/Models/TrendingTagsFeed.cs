﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Gfycat.API.Models
{
    internal class TrendingTagsFeed
    {
        [JsonProperty("tags")]
        internal IEnumerable<TrendingFeed> Tags { get; set; }
        [JsonProperty("cursor")]
        internal string Cursor { get; set; }
    }
}
