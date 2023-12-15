// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace BlockadeLabs.Skyboxes
{
    [Preserve]
    [Serializable]
    public sealed class SkyboxExportOption
    {
        public const string Equirectangular_JPG = "equirectangular-jpg";
        public const string Equirectangular_PNG = "equirectangular-png";
        public const string CubeMap_Roblox_PNG = "cube-map-roblox-png";
        public const string HDRI_HDR = "hdri-hdr";
        public const string HDRI_EXR = "hdri-exr";
        public const string DepthMap_PNG = "depth-map-png";
        public const string Video_LandScape_MP4 = "video-landscape-mp4";
        public const string Video_Portrait_MP4 = "video-portrait-mp4";
        public const string Video_Square_MP4 = "video-square-mp4";
        public const string CubeMap_PNG = "cube-map-default-png";

        [Preserve]
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

        [Preserve]
        [SerializeField]
        private string name;

        [Preserve]
        [JsonProperty("name")]
        public string Name => name;

        [Preserve]
        [SerializeField]
        private int id;

        [Preserve]
        [JsonProperty("id")]
        public int Id => id;

        [Preserve]
        [SerializeField]
        private string key;

        [Preserve]
        [JsonProperty("key")]
        public string Key => key;
    }
}
