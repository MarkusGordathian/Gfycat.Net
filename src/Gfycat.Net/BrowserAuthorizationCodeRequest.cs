﻿using Newtonsoft.Json;

namespace Gfycat
{
    internal class BrowserAuthorizationCodeRequest : ClientCredentialsAuthRequest
    {
        [JsonProperty("code")]
        internal string Code { get; set; }
        [JsonProperty("redirect_uri")]
        internal string RedirectUri { get; set; }
    }
}
