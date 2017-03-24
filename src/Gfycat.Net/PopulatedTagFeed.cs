﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Model = Gfycat.API.Models.TrendingTagsFeed;

namespace Gfycat
{
    [DebuggerDisplay("Tag count: {Content.Count}")]
    public class PopulatedTagFeed : IFeed<TaggedGfyFeed>
    {
        readonly RequestOptions _defaultOptions;
        readonly GfycatClient _client;
        
        internal PopulatedTagFeed(GfycatClient client,  RequestOptions defaultOptions)
        {
            _defaultOptions = defaultOptions;
            _client = client;
        }

        public IReadOnlyCollection<TaggedGfyFeed> Content { get; private set; }

        public string Cursor { get; private set; }

        internal static PopulatedTagFeed Create(GfycatClient client, RequestOptions options, Model model)
        {
            return new PopulatedTagFeed(client, options)
            {
                Content = model.Tags.Select(t => TaggedGfyFeed.Create(client, t, options)).ToReadOnlyCollection(),
                Cursor = model.Cursor
            };
        }

        public IAsyncEnumerator<TaggedGfyFeed> GetEnumerator()
        {
            return new PopulatedTagFeedEnumerator(_client, this, _defaultOptions);
        }
    }
}
