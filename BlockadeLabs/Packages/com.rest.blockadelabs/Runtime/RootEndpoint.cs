using System.Threading.Tasks;

namespace BlockadeLabs
{
    public sealed class RootEndpoint : BlockadeLabsBaseEndpoint
    {
        public RootEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => string.Empty;

        public async Task GetInFlightAsync()
        {
            await Task.CompletedTask;
        }

        public async Task GenerateSkyboxAsync()
        {
            await Task.CompletedTask;
        }

        public async Task GetSkyboxCallbackAsync()
        {
            await Task.CompletedTask;
        }

        public async Task TokenizePromptAsync()
        {
            await Task.CompletedTask;
        }
    }
}
