using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Utilities.Rest.Extensions;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxEndpoint : BlockadeLabsBaseEndpoint
    {
        public SkyboxEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => string.Empty;

        public async Task<string> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonConvert.SerializeObject(skyboxRequest, client.JsonSerializationOptions).ToJsonStringContent();
            var response = await client.Client.PostAsync(GetUrl("generate-skybox"), jsonContent, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return responseAsString;
        }

        public async Task GetSkyboxCallbackAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        public async Task TokenizePromptAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }
    }
}
