using Newtonsoft.Json;
using System;

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
            [JsonProperty("file_url")] string fileUrl,
            [JsonProperty("thumb_url")] string thumbUrl,
            [JsonProperty("depth_map_url")] string depthMapUrl,
            [JsonProperty("title")] string title,
            [JsonProperty("error_message")] object errorMessage,
            [JsonProperty("obfuscated_id")] string obfuscatedId,
            [JsonProperty("created_at")] DateTime createdAt,
            [JsonProperty("updated_at")] DateTime updatedAt)
        {
            Id = id;
            SkyboxStyleId = skyboxStyleId;
            SkyboxStyleName = skyboxStyleName;
            Status = status;
            Type = type;
            FileUrl = fileUrl;
            ThumbUrl = thumbUrl;
            DepthMapUrl = depthMapUrl;
            Title = title;
            ErrorMessage = errorMessage;
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
        public string FileUrl { get; }

        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; }

        [JsonProperty("depth_map_url")]
        public string DepthMapUrl { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("error_message")]
        public object ErrorMessage { get; }

        [JsonProperty("obfuscated_id")]
        public string ObfuscatedId { get; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; }
    }
}
