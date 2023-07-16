// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine;

namespace BlockadeLabs.Skyboxes
{
    [Serializable]
    public sealed class SkyboxStyle
    {
        [JsonConstructor]
        public SkyboxStyle(
            [JsonProperty("id")] int id,
            [JsonProperty("name")] string name,
            [JsonProperty("max-char")] string maxChar,
            [JsonProperty("negative-text-max-char")] int negativeTextMaxChar,
            [JsonProperty("image")] string image,
            [JsonProperty("sort_order")] int sortOrder)
        {
            this.id = id;
            this.name = name;
            MaxChar = maxChar;
            NegativeTextMaxChar = negativeTextMaxChar;
            Image = image;
            SortOrder = sortOrder;
        }

        [SerializeField]
        private string name;

        [JsonProperty("name")]
        public string Name => name;

        [SerializeField]
        private int id;

        [JsonProperty("id")]
        public int Id => id;

        [JsonProperty("max-char")]
        public string MaxChar { get; }

        [JsonProperty("negative-text-max-char")]
        public int NegativeTextMaxChar { get; }

        [JsonProperty("image")]
        public string Image { get; }

        [JsonProperty("sort_order")]
        public int SortOrder { get; }

        public static implicit operator int(SkyboxStyle style) => style.Id;
    }
}
