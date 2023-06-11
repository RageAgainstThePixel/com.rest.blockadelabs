// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utilities.WebRequestRest;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxEndpoint : BlockadeLabsBaseEndpoint
    {
        public SkyboxEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => "skybox";

        /// <summary>
        /// Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        public async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStylesAsync(CancellationToken cancellationToken = default)
        {
            var endpoint = GetUrl("/styles");
            var response = await Rest.GetAsync(endpoint, parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, client.JsonSerializationOptions);
        }

        /// <summary>
        /// Generate a skybox image
        /// </summary>
        /// <param name="skyboxRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SkyboxInfo> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonConvert.SerializeObject(skyboxRequest, client.JsonSerializationOptions);
            var response = await Rest.PostAsync(GetUrl("/generate"), jsonContent, parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfo>(response.Body, client.JsonSerializationOptions);

            var downloadTasks = new List<Task>(2)
            {
                Task.Run(async () =>
                {
                    skyboxInfo.MainTexture = await Rest.DownloadTextureAsync(skyboxInfo.MainTextureUrl, parameters: null, cancellationToken: cancellationToken);
                }, cancellationToken),
                Task.Run(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(skyboxInfo.DepthTextureUrl))
                    {
                        skyboxInfo.DepthTexture = await Rest.DownloadTextureAsync(skyboxInfo.DepthTextureUrl, parameters: null, cancellationToken: cancellationToken);
                    }
                }, cancellationToken)
            };

            await Task.WhenAll(downloadTasks);
            return skyboxInfo;
        }

        /// <summary>
        /// Returns the skybox metadata for the given skybox id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GetSkyboxInfoAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"/info/{id}"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            return JsonConvert.DeserializeObject<SkyboxInfo>(response.Body, client.JsonSerializationOptions);
        }
    }
}
