﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gfycat
{
    internal class ExtendedHttpClient : HttpClient
    {
        internal AuthenticationContainer Auth { get; set; }

        internal ExtendedHttpClient() : base() { }

        internal async Task CheckAuthorizationAsync(string endpoint)
        {
            if (await SendRequestForStatusAsync("HEAD", endpoint) == HttpStatusCode.Unauthorized)
            {
                await Auth.AttemptRefreshTokenAsync();
                if (await SendRequestForStatusAsync("HEAD", endpoint) == HttpStatusCode.Unauthorized)
                    throw new GfycatException()
                    {
                        HttpCode = (HttpStatusCode)401,
                        Code = "Unauthorized",
                        Description = "A valid access token is required to access this resource"
                    };
            }
        }

        internal async Task<T> SendJsonAsync<T>(string method, string endpoint, object json, bool useAccessToken = true)
        {
            Debug.WriteLine($"Sending json to {endpoint}");
            HttpRequestMessage message = await CreateMessageAsync(new HttpMethod(method), endpoint, useAccessToken);
            AddJsonContent(message, json);
            HttpResponseMessage result = await SendAsync(message);

            if (!result.IsSuccessStatusCode)
                throw await GetExceptionFromResponseAsync(result);

            return await GetJsonFromResponseAsync<T>(result);
        }

        internal async Task SendJsonAsync(string method, string endpoint, object json, bool useAccessToken = true)
        {
            Debug.WriteLine($"Sending json to {endpoint}");
            HttpRequestMessage message = await CreateMessageAsync(new HttpMethod(method), endpoint, useAccessToken);
            AddJsonContent(message, json);
            HttpResponseMessage result = await SendAsync(message);

            if (!result.IsSuccessStatusCode)
                throw await GetExceptionFromResponseAsync(result);
        }

        internal async Task<HttpStatusCode> SendStreamAsync(string method, string endpoint, Stream stream, string fileName, bool useAccessToken = true, bool throwIf401 = false, CancellationToken? cancelToken = null)
        {
            Debug.WriteLine($"Sending stream to {endpoint}");
            HttpRequestMessage message = await CreateMessageAsync(new HttpMethod(method), endpoint, useAccessToken);
            AddStreamContent(message, stream, fileName);
             
            HttpResponseMessage result = (cancelToken.HasValue) ? await SendAsync(message) : await SendAsync(message, cancelToken.Value);

            if (throwIf401 && result.StatusCode == HttpStatusCode.Unauthorized)
                throw await GetExceptionFromResponseAsync(result);

            return result.StatusCode;
        }

        internal async Task<T> SendRequestAsync<T>(string method, string endpoint, bool useAccessToken = true)
        {
            Debug.WriteLine($"Sending request to {endpoint}");
            HttpRequestMessage message = await CreateMessageAsync(new HttpMethod(method), endpoint, useAccessToken);
            HttpResponseMessage result = await SendAsync(message);

            if (!result.IsSuccessStatusCode)
                throw await GetExceptionFromResponseAsync(result);

            return await GetJsonFromResponseAsync<T>(result);
        }

        internal async Task SendRequestAsync(string method, string endpoint, bool useAccessToken = true)
        {
            Debug.WriteLine($"Sending request to {endpoint}");
            HttpRequestMessage message = await CreateMessageAsync(new HttpMethod(method), endpoint, useAccessToken);
            HttpResponseMessage result = await SendAsync(message);

            if (!result.IsSuccessStatusCode)
                throw await GetExceptionFromResponseAsync(result);
        }

        internal async Task<HttpStatusCode> SendRequestForStatusAsync(string method, string endpoint, bool useAccessToken = true, bool throwIf401 = false)
        {
            Debug.WriteLine($"Sending request for status code to {endpoint}");
            HttpRequestMessage message = await CreateMessageAsync(new HttpMethod(method), endpoint, useAccessToken);
            HttpResponseMessage result = await SendAsync(message);

            if (throwIf401 && result.StatusCode == HttpStatusCode.Unauthorized)
                throw await GetExceptionFromResponseAsync(result);

            return result.StatusCode;
        }

        internal async Task<string> SendRequestForStringAsync(string method, string endpoint, bool useAccessToken = true, bool throwIf401 = false)
        {
            Debug.WriteLine($"Sending request for status code to {endpoint}");
            HttpRequestMessage message = await CreateMessageAsync(new HttpMethod(method), endpoint, useAccessToken);
            HttpResponseMessage result = await SendAsync(message);

            if (throwIf401 && result.StatusCode == HttpStatusCode.Unauthorized)
                throw await GetExceptionFromResponseAsync(result);

            return await result.Content.ReadAsStringAsync();
        }

        internal async Task<HttpStatusCode> SendJsonForStatusAsync(string method, string endpoint, object json, bool useAccessToken = true, bool throwIf401 = false)
        {
            Debug.WriteLine($"Sending json for status code to {endpoint}");
            HttpRequestMessage message = await CreateMessageAsync(new HttpMethod(method), endpoint, useAccessToken);
            AddJsonContent(message, json);
            HttpResponseMessage result = await SendAsync(message);

            if (throwIf401 && result.StatusCode == HttpStatusCode.Unauthorized)
                throw await GetExceptionFromResponseAsync(result);

            return result.StatusCode;
        }
#pragma warning disable IDE0019 // https://github.com/dotnet/roslyn/issues/16195
        private async Task<T> GetJsonFromResponseAsync<T>(HttpResponseMessage message)
        {
            string result = await message.Content.ReadAsStringAsync();
            #if SUPER_DEBUG_MODE
            Debug.WriteLine(result);
            #endif

            T deserialized = JsonConvert.DeserializeObject<T>(result);

            ConnectedEntity webEntity = deserialized as ConnectedEntity;
            if (webEntity != null)
                webEntity.Web = this;

            return deserialized;
        }
#pragma warning restore IDE0019
        private async Task<GfycatException> GetExceptionFromResponseAsync(HttpResponseMessage message)
        {
            GfycatException exception = await GetJsonFromResponseAsync<GfycatException>(message);
            exception.HttpCode = message.StatusCode;
            return exception;
        }

        private static void AddJsonContent(HttpRequestMessage message, object json)
        {
            message.Content = new StringContent(JsonConvert.SerializeObject(json), System.Text.Encoding.UTF8, "application/json");
        }

        private static void AddStreamContent(HttpRequestMessage message, Stream stream, string fileName)
        {
            message.Content = new StreamContent(stream);
            message.Content.Headers.ContentDisposition.FileName = fileName;
        }

        private async Task<HttpRequestMessage> CreateMessageAsync(HttpMethod method, string endpoint, bool useAcccessToken = true)
        {
            if (useAcccessToken)
                await CheckAuthorizationAsync(endpoint);
            HttpRequestMessage message = new HttpRequestMessage(method, endpoint);
            if (useAcccessToken)
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Auth.AccessToken);
            return message;
        }

        internal static string CreateQueryString(IDictionary<string, object> nameValueCollection)
        {
            foreach (var pairWhereNullValue in nameValueCollection.Where(v => v.Value == null))
                nameValueCollection.Remove(pairWhereNullValue);

            if (nameValueCollection.Count == 0)
                return "";

            StringBuilder builder = new StringBuilder();
            builder.Append($"?{WebUtility.UrlEncode(nameValueCollection.Keys.First())}={WebUtility.UrlEncode(nameValueCollection.Values.First().ToString())}");

            for (int i = 1; i < nameValueCollection.Count; i++)
            {
                var element = nameValueCollection.ElementAt(i);
                builder.Append($"&{WebUtility.UrlEncode(element.Key)}={WebUtility.UrlEncode(element.Value.ToString())}");
            }

            return builder.ToString();
        }
    }
}
