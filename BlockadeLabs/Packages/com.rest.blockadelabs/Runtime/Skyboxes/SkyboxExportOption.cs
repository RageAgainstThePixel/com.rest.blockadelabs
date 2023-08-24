// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine;

namespace BlockadeLabs.Skyboxes
{
    [Serializable]
    public sealed class SkyboxExportOption
    {
        [JsonConstructor]
        public SkyboxExportOption(
            [JsonProperty("id")] int id,
            [JsonProperty("name")] string name,
            [JsonProperty("key")] string key)
        {
            this.name = name;
            this.id = id;
            this.key = key;
        }

        [SerializeField]
        private string name;

        [JsonProperty("name")]
        public string Name => name;

        [SerializeField]
        private int id;

        [JsonProperty("id")]
        public int Id => id;

        [SerializeField]
        private string key;

        [JsonProperty("key")]
        public string Key => key;
    }
}
