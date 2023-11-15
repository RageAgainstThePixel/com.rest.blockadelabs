// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxExportRequest
    {
        public SkyboxExportRequest(
            [JsonProperty("id")] string id,
            [JsonProperty("type")] string type,
            [JsonProperty("type_id")] int typeId,
            [JsonProperty("status")] Status status,
            [JsonProperty("queue_position")] int queuePosition,
            [JsonProperty("created_at")] DateTime createdAt,
            [JsonProperty("error_message")] string errorMessage = null)
        {
            Id = id;
            Type = type;
            TypeId = typeId;
            Status = status;
            QueuePosition = queuePosition;
            CreatedAt = createdAt;
            ErrorMessage = errorMessage;
        }

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("type_id")]
        public int TypeId { get; }

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; }

        [JsonProperty("queue_position")]
        public int QueuePosition { get; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; }
    }
}
