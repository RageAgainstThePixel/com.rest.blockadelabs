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

        public string Prompt { get; }

        public string NegativeText { get; }

        public int? Seed { get; }

        public int? SkyboxStyleId { get; }

        public int? RemixImagineId { get; }

        public bool Depth { get; }
    }
}
