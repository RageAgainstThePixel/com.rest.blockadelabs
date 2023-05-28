using System;
using Newtonsoft.Json;

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
            [JsonProperty("queue_position")] int queuePosition,
            [JsonProperty("file_url")] string fileUrl,
            [JsonProperty("thumb_url")] string thumbUrl,
            [JsonProperty("title")] string title,
            [JsonProperty("user_id")] int userId,
            [JsonProperty("username")] string username,
            [JsonProperty("error_message")] object errorMessage,
            [JsonProperty("obfuscated_id")] string obfuscatedId,
            [JsonProperty("pusher_channel")] string pusherChannel,
            [JsonProperty("pusher_event")] string pusherEvent,
            [JsonProperty("created_at")] DateTime createdAt,
            [JsonProperty("updated_at")] DateTime updatedAt
        )
        {
            Id = id;
            SkyboxStyleId = skyboxStyleId;
            SkyboxStyleName = skyboxStyleName;
            Status = status;
            Type = type;
            QueuePosition = queuePosition;
            FileUrl = fileUrl;
            ThumbUrl = thumbUrl;
            Title = title;
            UserId = userId;
            Username = username;
            ErrorMessage = errorMessage;
            ObfuscatedId = obfuscatedId;
            PusherChannel = pusherChannel;
            PusherEvent = pusherEvent;
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

        [JsonProperty("queue_position")]
        public int QueuePosition { get; }

        [JsonProperty("file_url")]
        public string FileUrl { get; }

        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("user_id")]
        public int UserId { get; }

        [JsonProperty("username")]
        public string Username { get; }

        [JsonProperty("error_message")]
        public object ErrorMessage { get; }

        [JsonProperty("obfuscated_id")]
        public string ObfuscatedId { get; }

        [JsonProperty("pusher_channel")]
        public string PusherChannel { get; }

        [JsonProperty("pusher_event")]
        public string PusherEvent { get; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; }
    }
}
