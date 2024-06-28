// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine.Scripting;

namespace BlockadeLabs.Skyboxes
{
    [Preserve]
    public sealed class SkyboxExportRequest : BaseResponse
    {
        [Preserve]
        [JsonConstructor]
        internal SkyboxExportRequest(
            [JsonProperty("id")] string id,
            [JsonProperty("file_url")] string fileUrl,
            [JsonProperty("skybox_obfuscated_id")] string skyboxObfuscatedId,
            [JsonProperty("type")] string type,
            [JsonProperty("type_id")] int typeId,
            [JsonProperty("status")] Status status,
            [JsonProperty("queue_position")] int queuePosition,
            [JsonProperty("error_message")] string errorMessage,
            [JsonProperty("pusher_channel")] string pusherChannel,
            [JsonProperty("pusher_event")] string pusherEvent,
            [JsonProperty("webhook_url")] string webhookUrl,
            [JsonProperty("created_at")] DateTime createdAt)
        {
            Id = id;
            FileUrl = fileUrl;
            SkyboxObfuscatedId = skyboxObfuscatedId;
            Type = type;
            TypeId = typeId;
            Status = status;
            QueuePosition = queuePosition;
            ErrorMessage = errorMessage;
            PusherChannel = pusherChannel;
            PusherEvent = pusherEvent;
            WebhookUrl = webhookUrl;
            CreatedAt = createdAt;
        }

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("file_url")]
        public string FileUrl { get; }

        [JsonProperty("skybox_obfuscated_id")]
        public string SkyboxObfuscatedId { get; }

        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("type_id")]
        public int TypeId { get; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; }

        [JsonProperty("queue_position")]
        public int QueuePosition { get; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; }

        [JsonProperty("pusher_channel")]
        public string PusherChannel { get; }

        [JsonProperty("pusher_event")]
        public string PusherEvent { get; }

        [JsonProperty("webhook_url")]
        public string WebhookUrl { get; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; }
    }
}
