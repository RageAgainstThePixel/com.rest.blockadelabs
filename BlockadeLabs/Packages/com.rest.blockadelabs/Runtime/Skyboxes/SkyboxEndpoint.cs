// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.WebRequestRest;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxEndpoint : BlockadeLabsBaseEndpoint
    {
        [Preserve]
        private class SkyboxInfoRequest
        {
            [Preserve]
            [JsonConstructor]
            public SkyboxInfoRequest([JsonProperty("request")] SkyboxInfo skyboxInfo)
            {
                SkyboxInfo = skyboxInfo;
            }

            [Preserve]
            [JsonProperty("request")]
            public SkyboxInfo SkyboxInfo { get; }
        }

        public SkyboxEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => string.Empty;

        /// <summary>
        /// Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        public async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStylesAsync(CancellationToken cancellationToken = default)
        {
            var endpoint = GetUrl("skybox/styles");
            var response = await Rest.GetAsync(endpoint, parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, client.JsonSerializationOptions);
        }

        /// <summary>
        /// Generate a skybox image.
        /// </summary>
        /// <param name="skyboxRequest"><see cref="SkyboxRequest"/>.</param>
        /// <param name="pollingInterval">Optional, polling interval in seconds.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, int? pollingInterval = null, CancellationToken cancellationToken = default)
        {
            var formData = new WWWForm();
            formData.AddField("prompt", skyboxRequest.Prompt);

            if (!string.IsNullOrWhiteSpace(skyboxRequest.NegativeText))
            {
                formData.AddField("negative_text", skyboxRequest.NegativeText);
            }

            if (skyboxRequest.Seed.HasValue)
            {
                formData.AddField("seed", skyboxRequest.Seed.Value);
            }

            if (skyboxRequest.SkyboxStyleId.HasValue)
            {
                formData.AddField("skybox_style_id", skyboxRequest.SkyboxStyleId.Value);
            }

            if (skyboxRequest.RemixImagineId.HasValue)
            {
                formData.AddField("remix_imagine_id", skyboxRequest.RemixImagineId.Value);
            }

            if (skyboxRequest.Depth)
            {
                formData.AddField("return_depth", skyboxRequest.Depth.ToString());
            }

            if (skyboxRequest.ControlImage != null)
            {
                if (!string.IsNullOrWhiteSpace(skyboxRequest.ControlModel))
                {
                    formData.AddField("control_model", skyboxRequest.ControlModel);
                }

                using var imageData = new MemoryStream();
                await skyboxRequest.ControlImage.CopyToAsync(imageData, cancellationToken);
                formData.AddBinaryData("control_image", imageData.ToArray(), skyboxRequest.ControlImageFileName);
                skyboxRequest.Dispose();
            }

            var response = await Rest.PostAsync(GetUrl("skybox"), formData, parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfo>(response.Body, client.JsonSerializationOptions);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(pollingInterval ?? 3 * 1000, cancellationToken)
                    .ConfigureAwait(true); // Configure await to make sure we're still in Unity context
                skyboxInfo = await GetSkyboxInfoAsync(skyboxInfo, cancellationToken);

                if (skyboxInfo.Status is "pending" or "processing")
                {
                    continue;
                }

                break;
            }

            if (skyboxInfo.Status != "complete")
            {
                throw new Exception($"Failed to generate skybox! {skyboxInfo.Id} -> {skyboxInfo.Status}\nError: {skyboxInfo.ErrorMessage}\n{skyboxInfo}");
            }

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
        /// <param name="id">Skybox Id.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GetSkyboxInfoAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"imagine/requests/{id}"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            return JsonConvert.DeserializeObject<SkyboxInfoRequest>(response.Body, client.JsonSerializationOptions).SkyboxInfo;
        }
    }
}
