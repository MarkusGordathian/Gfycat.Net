﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model = Gfycat.API.Models.Feed;

namespace Gfycat
{
    internal class SiteSearchFeed : GfyFeed
    {
        protected readonly string _searchText;

        internal SiteSearchFeed(GfycatClient client, string searchText, RequestOptions options) : base(client, options)
        {
            _searchText = searchText;
        }

        internal static SiteSearchFeed Create(GfycatClient client, Model model, string searchText, RequestOptions options)
        {
            return new SiteSearchFeed(client, searchText, options)
            {
                Content = model.Gfycats.Select(g => Gfy.Create(client, g)).ToReadOnlyCollection(),
                _cursor = model.Cursor
            };
        }
        /// <summary>
        /// Returns an enumerator to enumerate through this feed
        /// </summary>
        /// <returns></returns>
        public override IAsyncEnumerator<Gfy> GetEnumerator()
        {
            return new FeedEnumerator<Gfy>(_client, this, _options);
        }
        /// <summary>
        /// Returns the next page of this feed
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async override Task<IFeed<Gfy>> GetNextPageAsync(RequestOptions options = null)
        {
            return Create(_client, await _client.ApiClient.SearchSiteAsync(_searchText, _cursor, options).ConfigureAwait(false), _searchText, options);
        }
    }
}
