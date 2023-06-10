using Newtonsoft.Json;
using System;
using UnityEngine;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxInfo
    {
        [JsonConstructor]
        public SkyboxInfo(
            [JsonProperty("id")] int id,
            [JsonProperty("skybox_style_id")] int skyboxStyleId,
            [JsonProperty("skybox_style_name")] string skyboxStyleName,
            [JsonProperty("status")] string status,
            [JsonProperty("type")] string type,
            [JsonProperty("file_url")] string mainTextureUrl,
            [JsonProperty("thumb_url")] string thumbUrl,
            [JsonProperty("depth_map_url")] string depthTextureUrl,
            [JsonProperty("title")] string title,
            [JsonProperty("obfuscated_id")] string obfuscatedId,
            [JsonProperty("created_at")] DateTime createdAt,
            [JsonProperty("updated_at")] DateTime updatedAt)
        {
            Id = id;
            SkyboxStyleId = skyboxStyleId;
            SkyboxStyleName = skyboxStyleName;
            Status = status;
            Type = type;
            MainTextureUrl = mainTextureUrl;
            ThumbUrl = thumbUrl;
            DepthTextureUrl = depthTextureUrl;
            Title = title;
            ObfuscatedId = obfuscatedId;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("skybox_style_id")]
        public int SkyboxStyleId { get; }

        [JsonProperty("skybox_style_name")]
        public string SkyboxStyleName { get; }

        [JsonProperty("status")]
        public string Status { get; }

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
    }
}
