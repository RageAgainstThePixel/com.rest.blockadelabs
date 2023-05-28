using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Rest.Extensions;
using Utilities.WebRequestRest;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxEndpoint : BlockadeLabsBaseEndpoint
    {
        public SkyboxEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => string.Empty;

        public async Task<Texture2D> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonConvert.SerializeObject(skyboxRequest, client.JsonSerializationOptions).ToJsonStringContent();
            var response = await client.Client.PostAsync(GetUrl("generate-skybox"), jsonContent, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync(true);
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfo>(responseAsString, client.JsonSerializationOptions);
            var texture = await Rest.DownloadTextureAsync(skyboxInfo.FileUrl, cancellationToken: cancellationToken);
            return texture;
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
