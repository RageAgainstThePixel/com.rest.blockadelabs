// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Async;
using Utilities.WebRequestRest;
using Debug = UnityEngine.Debug;
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
            exports ??= new Dictionary<string, string>();
            exports.TryAdd("equirectangular-png", mainTextureUrl);
            exports.TryAdd("depth-map-png", depthTextureUrl);
            Exports = exports;
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
        [Obsolete("Get texture from ExportedAssets")]
        public Texture2D MainTexture { get; internal set; }

        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; }

        [JsonIgnore]
        public Texture2D Thumbnail { get; internal set; }

        [JsonProperty("depth_map_url")]
        public string DepthTextureUrl { get; private set; }

        [JsonIgnore]
        [Obsolete("Get depth from ExportedAssets")]
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
        public IReadOnlyDictionary<string, string> Exports { get; }

        // ReSharper disable once InconsistentNaming
        internal readonly Dictionary<string, Object> exportedAssets = new Dictionary<string, Object>();

        [JsonIgnore]
        public IReadOnlyDictionary<string, Object> ExportedAssets => exportedAssets;

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented, BlockadeLabsClient.JsonSerializationOptions);

        public static implicit operator int(SkyboxInfo skyboxInfo) => skyboxInfo.Id;

        /// <summary>
        /// Loads the textures for this skybox.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        [Obsolete("Use LoadAssetsAsync")]
        public async Task LoadTexturesAsync(CancellationToken cancellationToken = default)
            => await LoadAssetsAsync(false, cancellationToken);

        /// <summary>
        /// Downloads and loads all of the assets associated with this skybox.
        /// </summary>
        /// <param name="debug">Optional, debug downloads.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task LoadAssetsAsync(bool debug = false, CancellationToken cancellationToken = default)
        {
            async Task DownloadThumbnail()
            {
                if (!string.IsNullOrWhiteSpace(ThumbUrl))
                {
                    await Awaiters.UnityMainThread;
                    Rest.TryGetFileNameFromUrl(ThumbUrl, out var filename);
                    Thumbnail = await Rest.DownloadTextureAsync(ThumbUrl, fileName: $"{ObfuscatedId}-thumb{Path.GetExtension(filename)}", null, debug, cancellationToken);
                }
            }

            async Task DownloadExport(KeyValuePair<string, string> export)
            {
                try
                {
                    await Awaiters.UnityMainThread;
                    var exportUrl = export.Value;

                    if (!string.IsNullOrWhiteSpace(exportUrl))
                    {
                        Rest.TryGetFileNameFromUrl(exportUrl, out var filename);
                        var path = $"{ObfuscatedId}-{export.Key}{Path.GetExtension(filename)}";

                        switch (export.Key)
                        {
                            case "depth-map-png":
                            case "equirectangular-png":
                            case "equirectangular-jpg":
                                var texture = await Rest.DownloadTextureAsync(exportUrl, path, null, debug, cancellationToken);
                                exportedAssets[export.Key] = texture;
                                break;
                            case "cube-map-default-png":
                            case "cube-map-roblox-png":
                                var zipPath = await Rest.DownloadFileAsync(exportUrl, path, null, debug, cancellationToken);
                                var files = await ExportUtilities.UnZipAsync(zipPath, cancellationToken);
                                var textures = new List<Texture2D>();

                                foreach (var file in files)
                                {
                                    var face = await Rest.DownloadTextureAsync($"file://{file}", null, null, debug, cancellationToken);
                                    textures.Add(face);
                                }

                                var cubemap = ExportUtilities.BuildCubemap(textures);
                                exportedAssets[export.Key] = cubemap;
                                break;
                            case "hdri-hdr":
                            case "hdri-exr":
                            case "video-landscape-mp4":
                            case "video-portrait-mp4":
                            case "video-square-mp4":
                                await Rest.DownloadFileAsync(exportUrl, path, null, debug, cancellationToken);
                                break;
                            default:
                                Debug.LogWarning($"No download task defined for {export.Key}!");
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"No valid url for skybox {ObfuscatedId}.{export.Key}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            var downloadTasks = new List<Task> { DownloadThumbnail() };
            downloadTasks.AddRange(Exports.Select(DownloadExport));
            await Task.WhenAll(downloadTasks).ConfigureAwait(true);
        }

        public bool TryGetAssetCachePath(string key, out string localCachedPath)
        {
            if (Exports.TryGetValue(key, out var exportUrl) &&
                Rest.TryGetFileNameFromUrl(exportUrl, out var filename))
            {
                var cachePath = Path.Combine(Rest.DownloadCacheDirectory, $"{ObfuscatedId}-{key}{Path.GetExtension(filename)}");
                return Rest.TryGetDownloadCacheItem($"file://{cachePath}", out localCachedPath);
            }

            localCachedPath = string.Empty;
            return false;
        }

        public bool TryGetAsset<T>(string key, out T asset) where T : Object
        {
            if (ExportedAssets.TryGetValue(key, out var obj))
            {
                asset = (T)obj;
                return true;
            }

            if (Exports.ContainsKey(key))
            {
                Debug.LogWarning($"{key} exists, but has not been loaded. Have you called {nameof(LoadAssetsAsync)}?");
            }

            asset = default;
            return false;
        }
    }
}
