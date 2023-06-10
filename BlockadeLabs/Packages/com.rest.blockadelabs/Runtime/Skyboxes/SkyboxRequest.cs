using Newtonsoft.Json;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxRequest
    {
        public SkyboxRequest(
            string prompt,
            string negativeText = null,
            int? seed = null,
            int? skyboxStyleId = null,
            int? remixImagineId = null,
            bool depth = false)
        {
            Prompt = prompt;
            NegativeText = negativeText;
            Seed = seed;
            SkyboxStyleId = skyboxStyleId;
            RemixImagineId = remixImagineId;
            Depth = depth;
        }

        [JsonProperty("prompt")]
        public string Prompt { get; }

        [JsonProperty("negative_text")]
        public string NegativeText { get; }

        [JsonProperty("seed")]
        public int? Seed { get; }

        [JsonProperty("skybox_style_id")]
        public int? SkyboxStyleId { get; }

        [JsonProperty("remix_imagine_id")]
        public int? RemixImagineId { get; }

        [JsonProperty("return_depth")]
        public bool Depth { get; }
    }
}
