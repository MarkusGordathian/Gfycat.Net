﻿using System;
using System.Threading.Tasks;
using Model = Gfycat.API.Models.Status;

namespace Gfycat
{
    /// <summary>
    /// Represents a Gfy's upload status
    /// </summary>
    public class GfyStatus : IUpdatable
    {
        readonly GfycatClient _client;
        private string _errorResponse;

        /// <summary>
        /// The current upload task of the gfy
        /// </summary>
        public UploadTask Task { get; private set; }
        /// <summary>
        /// The current time remaining in seconds
        /// </summary>
        public int Time { get; private set; }
        /// <summary>
        /// The current gfy name for getting the current status
        /// </summary>
        public string GfyName { get; private set; }

        internal GfyStatus(GfycatClient client, Model model)
        {
            _client = client;
            Update(model);
        }

        internal void Update(Model model)
        {
            Task = model.Task;
            Time = model.Time;
            if (model.GfyName != null)
                GfyName = model.GfyName;
            _errorResponse = model.ErrorDescription;
        }
        
        /// <summary>
        /// Updates gfy's upload status
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task UpdateAsync(RequestOptions options = null)
            => Update(await _client.ApiClient.GetGfyStatusAsync(GfyName, options).ConfigureAwait(false));

        /// <summary>
        /// Gets the <see cref="Gfy"/> this status is checking if it's upload task is complete
        /// </summary>
        /// <param name="options"></param>
        /// <returns>An awaitable <see cref="Gfy"/></returns>
        /// <exception cref="InvalidOperationException">If the gfy's upload task isn't set to complete, running this method will be an invalid operation</exception>
        public async Task<Gfy> GetGfyAsync(RequestOptions options = null)
        {
            if (Task != UploadTask.Complete)
                throw new InvalidOperationException("The Gfy's upload isn't complete!");

            return await _client.GetGfyAsync(GfyName, false, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the gfy to finish uploading and retrieves it when it's done. This task is long running.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<Gfy> GetGfyWhenCompleteAsync(RequestOptions options = null)
        {
            while(Task == UploadTask.Encoding)
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(Time)).ConfigureAwait(false);
                await UpdateAsync(options).ConfigureAwait(false);
            }

            if (Task == UploadTask.Complete)
                return await GetGfyAsync(options).ConfigureAwait(false);
            else
                return null;
            
        }
    }
}
