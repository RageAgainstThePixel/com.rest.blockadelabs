using Newtonsoft.Json;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxStyle
    {
        [JsonConstructor]
        public SkyboxStyle(
            [JsonProperty("id")] int id,
            [JsonProperty("name")] string name,
            [JsonProperty("max-char")] string maxChar,
            [JsonProperty("negative-text-max-char")] int negativeTextMaxChar,
            [JsonProperty("image")] string image,
            [JsonProperty("sort_order")] int sortOrder
        )
        {
            Id = id;
            Name = name;
            MaxChar = maxChar;
            NegativeTextMaxChar = negativeTextMaxChar;
            Image = image;
            SortOrder = sortOrder;
        }

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("max-char")]
        public string MaxChar { get; }

        [JsonProperty("negative-text-max-char")]
        public int NegativeTextMaxChar { get; }

        [JsonProperty("image")]
        public string Image { get; }

        [JsonProperty("sort_order")]
        public int SortOrder { get; }
    }
}
