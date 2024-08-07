// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace BlockadeLabs.Skyboxes
{
    [Preserve]
    public class SkyboxStyleFamily
    {
        [Preserve]
        [JsonConstructor]
        internal SkyboxStyleFamily(
            [JsonProperty("id")] int id,
            [JsonProperty("name")] string name)
        {
            Id = id;
            Name = name;
        }

        [Preserve]
        [JsonProperty("id")]
        public int Id { get; }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        public static implicit operator int(SkyboxStyleFamily family) => family.Id;
    }
}
