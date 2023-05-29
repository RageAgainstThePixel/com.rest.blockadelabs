using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Rest.Extensions;
using Utilities.WebRequestRest;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxEndpoint : BlockadeLabsBaseEndpoint
    {
        private class SkyboxMetadata
        {
            public SkyboxMetadata([JsonProperty("request")] SkyboxInfo request)
            {
                Request = request;
            }

            [JsonProperty("request")]
            public SkyboxInfo Request { get; }
        }

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
            var response = await client.Client.GetAsync(endpoint, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(responseAsString, client.JsonSerializationOptions);
        }

        /// <summary>
        /// Generate a skybox image
        /// </summary>
        /// <param name="skyboxRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SkyboxInfo> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonConvert.SerializeObject(skyboxRequest, client.JsonSerializationOptions).ToJsonStringContent();
            var response = await client.Client.PostAsync(GetUrl("/generate"), jsonContent, cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfo>(responseAsString, client.JsonSerializationOptions);

            var downloadTasks = new List<Task>(2)
            {
                Task.Run(async () =>
                {
                    skyboxInfo.MainTexture = await Rest.DownloadTextureAsync(skyboxInfo.MainTextureUrl, cancellationToken: cancellationToken);
                }, cancellationToken),
                Task.Run(async () =>
                {
                    if (!string.IsNullOrWhiteSpace(skyboxInfo.DepthTextureUrl))
                    {
                        skyboxInfo.DepthTexture = await Rest.DownloadTextureAsync(skyboxInfo.DepthTextureUrl, cancellationToken: cancellationToken);
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
            var response = await client.Client.GetAsync(GetUrl($"/info/{id}"), cancellationToken);
            var responseAsString = await response.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SkyboxMetadata>(responseAsString, client.JsonSerializationOptions).Request;
        }
    }
}
