﻿using Newtonsoft.Json;

namespace Gfycat.API.Responses
{
    internal class ErrorResponse
    {
        [JsonProperty("errorMessage")]
        internal GfycatException Error { get; set; }

        [JsonProperty("message")]
        internal string Message
        {
            get => Error.ServerMessage;
            set { Error.ServerMessage = value; }
        }
    }
}