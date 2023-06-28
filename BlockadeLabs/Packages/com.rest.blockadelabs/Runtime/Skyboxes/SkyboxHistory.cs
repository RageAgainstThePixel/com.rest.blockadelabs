// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxHistory
    {
        [JsonConstructor]
        public SkyboxHistory(
            [JsonProperty("data")] List<SkyboxInfo> skyboxes,
            [JsonProperty("totalCount")] int totalCount,
            [JsonProperty("has_more")] bool hasMore)
        {
            Skyboxes = skyboxes;
            TotalCount = totalCount;
            HasMore = hasMore;
        }

        [JsonProperty("data")]
        public IReadOnlyList<SkyboxInfo> Skyboxes { get; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; }

        [JsonProperty("has_more")]
        public bool HasMore { get; }
    }
}
