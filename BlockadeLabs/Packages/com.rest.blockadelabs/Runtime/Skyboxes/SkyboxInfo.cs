// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.WebRequestRest;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxInfo
    {
        [JsonConstructor]
        public SkyboxInfo(
            [JsonProperty("id")] int id,
            [JsonProperty("skybox_style_id")] int skyboxStyleId,
            [JsonProperty("skybox_style_name")] string skyboxStyleName,
            [JsonProperty("status")] Status status,
            [JsonProperty("queue_position")] int queuePosition,
            [JsonProperty("type")] string type,
            [JsonProperty("file_url")] string mainTextureUrl,
            [JsonProperty("thumb_url")] string thumbUrl,
            [JsonProperty("depth_map_url")] string depthTextureUrl,
            [JsonProperty("title")] string title,
            [JsonProperty("obfuscated_id")] string obfuscatedId,
            [JsonProperty("created_at")] DateTime createdAt,
            [JsonProperty("updated_at")] DateTime updatedAt,
            [JsonProperty("dispatched_at")] DateTime dispatchedAt,
            [JsonProperty("processing_at")] DateTime processingAt,
            [JsonProperty("completed_at")] DateTime completedAt,
            [JsonProperty("error_message")] string errorMessage = null)
        {
            Id = id;
            SkyboxStyleId = skyboxStyleId;
            SkyboxStyleName = skyboxStyleName;
            Status = status;
            QueuePosition = queuePosition;
            Type = type;
            MainTextureUrl = mainTextureUrl;
            ThumbUrl = thumbUrl;
            DepthTextureUrl = depthTextureUrl;
            Title = title;
            ObfuscatedId = obfuscatedId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            DispatchedAt = dispatchedAt;
            ProcessingAt = processingAt;
            CompletedAt = completedAt;
            ErrorMessage = errorMessage;
        }

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("skybox_style_id")]
        public int SkyboxStyleId { get; }

        [JsonProperty("skybox_style_name")]
        public string SkyboxStyleName { get; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; }

        [JsonProperty("queue_position")]
        public int QueuePosition { get; }

        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("file_url")]
        public string MainTextureUrl { get; }

        [JsonIgnore]
        public Texture2D MainTexture { get; internal set; }

        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; }

        [JsonIgnore]
        public Texture2D Thumbnail { get; internal set; }

        [JsonProperty("depth_map_url")]
        public string DepthTextureUrl { get; }

        [JsonIgnore]
        public Texture2D DepthTexture { get; internal set; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("obfuscated_id")]
        public string ObfuscatedId { get; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; }

        [JsonProperty("dispatched_at")]
        public DateTime DispatchedAt { get; }

        [JsonProperty("processing_at")]
        public DateTime ProcessingAt { get; }

        [JsonProperty("completed_at")]
        public DateTime CompletedAt { get; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static implicit operator int(SkyboxInfo skyboxInfo) => skyboxInfo.Id;

        /// <summary>
        /// Loads the textures for this skybox.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task LoadTexturesAsync(CancellationToken cancellationToken = default)
        {
            var downloadTasks = new List<Task>(2)
            {
                Task.Run(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(ThumbUrl))
                    {
                        Thumbnail = await Rest.DownloadTextureAsync(ThumbUrl, parameters:null, cancellationToken: cancellationToken);
                    }
                }, cancellationToken),
                Task.Run(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(MainTextureUrl))
                    {
                        MainTexture = await Rest.DownloadTextureAsync(MainTextureUrl, parameters: null, cancellationToken: cancellationToken);
                    }
                }, cancellationToken),
                Task.Run(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(DepthTextureUrl))
                    {
                        DepthTexture = await Rest.DownloadTextureAsync(DepthTextureUrl, parameters: null, cancellationToken: cancellationToken);
                    }
                }, cancellationToken)
            };

            await Task.WhenAll(downloadTasks).ConfigureAwait(true);
        }
    }
}
