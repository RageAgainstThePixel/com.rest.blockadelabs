// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using Utilities.Async;
using Utilities.WebRequestRest;
using Object = UnityEngine.Object;

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
            [JsonProperty("error_message")] string errorMessage = null,
            [JsonProperty("exports")] Dictionary<string, string> exports = null)
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
            Exports = exports ?? new Dictionary<string, string>();
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
        public string MainTextureUrl { get; private set; }

        [JsonIgnore]
        public Texture2D MainTexture { get; internal set; }

        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; }

        [JsonIgnore]
        public Texture2D Thumbnail { get; internal set; }

        [JsonProperty("depth_map_url")]
        public string DepthTextureUrl { get; private set; }

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
        public string ErrorMessage { get; }

        [JsonProperty("exports")]
        public Dictionary<string, string> Exports { get; private set; }

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public static implicit operator int(SkyboxInfo skyboxInfo) => skyboxInfo.Id;

        /// <summary>
        /// Loads the textures for this skybox.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        [Obsolete("Use LoadAssetsAsync")]
        public async Task LoadTexturesAsync(CancellationToken cancellationToken = default)
            => await LoadAssetsAsync(cancellationToken);

        /// <summary>
        /// Downloads and loads all of the assets associated with this skybox.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task LoadAssetsAsync(CancellationToken cancellationToken = default)
        {
            if (Exports.TryGetValue("equirectangular-png", out var imageUrl) ||
                Exports.TryGetValue("equirectangular-jpg", out imageUrl))
            {
                MainTextureUrl = imageUrl;
            }

            if (Exports.TryGetValue("depth-map-png", out var depthUrl) ||
                Exports.TryGetValue("depth-map-jpg", out depthUrl))
            {
                DepthTextureUrl = depthUrl;
            }

            var downloadTasks = new List<Task>
            {
                Task.Run(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(ThumbUrl))
                    {
                        await Awaiters.UnityMainThread;
                        Rest.TryGetFileNameFromUrl(ThumbUrl, out var filename);
                        Thumbnail = await Rest.DownloadTextureAsync(ThumbUrl, fileName: $"{ObfuscatedId}-thumb{Path.GetExtension(filename)}", cancellationToken: cancellationToken);
                    }
                }, cancellationToken),
                Task.Run(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(MainTextureUrl))
                    {
                        await Awaiters.UnityMainThread;
                        Rest.TryGetFileNameFromUrl(MainTextureUrl, out var filename);
                        MainTexture = await Rest.DownloadTextureAsync(MainTextureUrl, fileName: $"{ObfuscatedId}-albedo{Path.GetExtension(filename)}", cancellationToken: cancellationToken);
                    }
                }, cancellationToken),
                Task.Run(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(DepthTextureUrl))
                    {
                        await Awaiters.UnityMainThread;
                        Rest.TryGetFileNameFromUrl(DepthTextureUrl, out var filename);
                        DepthTexture = await Rest.DownloadTextureAsync(DepthTextureUrl, fileName: $"{ObfuscatedId}-depth{Path.GetExtension(filename)}", cancellationToken: cancellationToken);
                    }
                }, cancellationToken)
            };

            //downloadTasks.AddRange(Exports.Select(export => Task.Run(async () =>
            //{
            //    var exportUrl = export.Value;
            //    Debug.Log($"{export.Key} -> {exportUrl}");

            //    if (!string.IsNullOrWhiteSpace(exportUrl))
            //    {
            //        await Awaiters.UnityMainThread;
            //        //Rest.TryGetFileNameFromUrl(exportUrl, out var filename);

            //        switch (export.Key)
            //        {
            //            case "depth-map-png":
            //            case "equirectangular-png":
            //            case "equirectangular-jpg":
            //                // already handled.
            //                break;
            //            case "cube-map-png":
            //                // TODO download and extract zip
            //                break;
            //            case "hdri-hdr":
            //            case "hdri-exr":
            //            case "video-landscape-mp4":
            //            case "video-portrait-mp4":
            //            case "video-square-mp4":
            //                // await Rest.DownloadFileAsync(exportUrl, $"{ObfuscatedId}-{export.Key}{Path.GetExtension(filename)}", cancellationToken: cancellationToken);
            //                break;
            //            default:
            //                Debug.LogWarning($"No download task defined for {export.Key}!");
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        Debug.LogError($"No valid url for skybox {ObfuscatedId}.{export.Key}");
            //    }
            //}, cancellationToken)));

            await Task.WhenAll(downloadTasks).ConfigureAwait(true);
        }
    }
}
