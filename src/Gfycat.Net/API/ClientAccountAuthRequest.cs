﻿using Newtonsoft.Json;

namespace Gfycat.API
{
    internal class ClientAccountAuthRequest : ClientCredentialsAuthRequest
    {
        [JsonProperty("username")]
        internal string Username { get; set; }
        [JsonProperty("password")]
        internal string Password { get; set; }
    }
}
