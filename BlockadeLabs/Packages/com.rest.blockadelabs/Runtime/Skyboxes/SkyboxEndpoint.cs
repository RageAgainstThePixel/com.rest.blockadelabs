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

        [Preserve]
        private class SkyboxOperation
        {
            [Preserve]
            [JsonConstructor]
            public SkyboxOperation(
                [JsonProperty("success")] string success,
                [JsonProperty("error")] string error)
            {
                Success = success;
                Error = error;
            }

            [Preserve]
            [JsonProperty("success")]
            public string Success { get; }

            [Preserve]
            [JsonProperty("Error")]
            public string Error { get; }
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
            var response = await Rest.GetAsync(GetUrl("skybox/styles"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
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
                await Task.Delay(pollingInterval ?? 3 * 1000, CancellationToken.None)
                    .ConfigureAwait(true); // Configure await to make sure we're still in Unity context
                skyboxInfo = await GetSkyboxInfoAsync(skyboxInfo, CancellationToken.None);

                if (skyboxInfo.Status is Status.Pending or Status.Processing or Status.Dispatched)
                {
                    continue;
                }

                break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                var cancelResult = await CancelSkyboxGenerationAsync(skyboxInfo, CancellationToken.None);

                if (!cancelResult)
                {
                    throw new Exception($"Failed to cancel generation for {skyboxInfo.Id}");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (skyboxInfo.Status != Status.Complete)
            {
                throw new Exception($"Failed to generate skybox! {skyboxInfo.Id} -> {skyboxInfo.Status}\nError: {skyboxInfo.ErrorMessage}\n{skyboxInfo}");
            }

            await skyboxInfo.LoadTexturesAsync(cancellationToken);
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

        /// <summary>
        /// Deletes a skybox by id.
        /// </summary>
        /// <param name="id">The id of the skybox.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if skybox was successfully deleted.</returns>
        public async Task<bool> DeleteSkyboxAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"imagine/deleteImagine/{id}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, client.JsonSerializationOptions);

            const string successStatus = "Item deleted successfully";

            if (skyboxOp is not { Success: successStatus })
            {
                throw new Exception($"Failed to cancel generation for skybox {id}!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals(successStatus);
        }

        /// <summary>
        /// Gets the previously generated skyboxes.
        /// </summary>
        /// <param name="parameters">Optional, <see cref="SkyboxHistoryParameters"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxHistory"/>.</returns>
        public async Task<SkyboxHistory> GetSkyboxHistoryAsync(SkyboxHistoryParameters parameters = null, CancellationToken cancellationToken = default)
        {
            var historyRequest = parameters ?? new SkyboxHistoryParameters();
            var response = await Rest.GetAsync(GetUrl($"imagine/myRequests{historyRequest}"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            return JsonConvert.DeserializeObject<SkyboxHistory>(response.Body, client.JsonSerializationOptions);
        }

        /// <summary>
        /// Cancels a pending skybox generation request by id.
        /// </summary>
        /// <param name="id">The id of the skybox.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if generation was cancelled.</returns>
        public async Task<bool> CancelSkyboxGenerationAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"imagine/requests/{id}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, client.JsonSerializationOptions);

            if (skyboxOp is not { Success: "true" })
            {
                throw new Exception($"Failed to cancel generation for skybox {id}!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals("true");
        }

        /// <summary>
        /// Cancels ALL pending skybox generation requests.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        public async Task<bool> CancelAllPendingSkyboxGenerationsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl("imagine/requests/pending"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate();
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, client.JsonSerializationOptions);

            if (skyboxOp is not { Success: "true" })
            {
                if (skyboxOp != null &&
                    skyboxOp.Error.Contains("You don't have any pending"))
                {
                    return false;
                }

                throw new Exception($"Failed to cancel all pending skybox generations!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals("true");
        }
    }
}
