// Licensed under the MIT License. See LICENSE in the project root for license information.

using BlockadeLabs.Skyboxes;
using NUnit.Framework;
using System;
using System.Threading;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace BlockadeLabs.Tests
{
    public class TestFixture_01_Skyboxes
    {
        [Test]
        public async Task Test_01_GetSkyboxStyles()
        {
            var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromEnvironment());
            Assert.IsNotNull(api.SkyboxEndpoint);
            var skyboxStyles = await api.SkyboxEndpoint.GetSkyboxStylesAsync();
            Assert.IsNotNull(skyboxStyles);

            foreach (var skyboxStyle in skyboxStyles)
            {
                Debug.Log($"{skyboxStyle.Name}");
            }
        }

        [Test]
        public async Task Test_02_GenerateSkybox()
        {
            var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromEnvironment());
            Assert.IsNotNull(api.SkyboxEndpoint);

            var request = new SkyboxRequest("mars", depth: true);
            var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(request);
            Assert.IsNotNull(skyboxInfo);
            Debug.Log($"Successfully created skybox: {skyboxInfo.Id}");
            Debug.Log(skyboxInfo.MainTextureUrl);
            Assert.IsNotNull(skyboxInfo.MainTexture);
            Debug.Log(skyboxInfo.DepthTextureUrl);
            Assert.IsNotNull(skyboxInfo.DepthTexture);
            Debug.Log(skyboxInfo.ToString());
        }

        [Test]
        public async Task Test_03_GetSkyboxInfo()
        {
            var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromEnvironment());
            Assert.IsNotNull(api.SkyboxEndpoint);
            var skyboxId = 6602899;
            var skyboxInfo = await api.SkyboxEndpoint.GetSkyboxInfoAsync(skyboxId);
            Assert.IsNotNull(skyboxInfo);
            await skyboxInfo.LoadTexturesAsync();
            Assert.IsNotNull(skyboxInfo.MainTexture);
            Assert.IsNotNull(skyboxInfo.DepthTexture);
        }

        [Test]
        public async Task Test_04_GetHistory()
        {
            var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromEnvironment());
            Assert.IsNotNull(api.SkyboxEndpoint);
            var history = await api.SkyboxEndpoint.GetSkyboxHistoryAsync();
            Assert.IsNotNull(history);
            Assert.IsNotEmpty(history.Skyboxes);
            Debug.Log($"Found {history.TotalCount} skyboxes");

            foreach (var skybox in history.Skyboxes)
            {
                Debug.Log($"{skybox.Id} {skybox.Title} status: {skybox.Status}");
            }
        }

        [Test]
        public async Task Test_05_CancelPendingGeneration()
        {
            var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromEnvironment());
            Assert.IsNotNull(api.SkyboxEndpoint);
            var request = new SkyboxRequest("mars", depth: true);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));

            try
            {
                await api.SkyboxEndpoint.GenerateSkyboxAsync(request, 1, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Operation successfully cancelled");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        [Test]
        public async Task Test_06_CancelAllPendingGenerations()
        {
            var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromEnvironment());
            Assert.IsNotNull(api.SkyboxEndpoint);
            var result = await api.SkyboxEndpoint.CancelAllPendingSkyboxGenerationsAsync();
            Debug.Log(result ? "All pending generations successfully cancelled" : "No pending generations");
        }

        [Test]
        public async Task Test_07_DeleteSkybox()
        {
            var api = new BlockadeLabsClient(BlockadeLabsAuthentication.Default.LoadFromEnvironment());
            Assert.IsNotNull(api.SkyboxEndpoint);
            var history = await api.SkyboxEndpoint.GetSkyboxHistoryAsync(new SkyboxHistoryParameters { StatusFilter = Status.Abort });
            Assert.IsNotNull(history);

            foreach (var skybox in history.Skyboxes)
            {
                Debug.Log($"Deleting {skybox.Id} {skybox.Title}");
                var result = await api.SkyboxEndpoint.DeleteSkyboxAsync(skybox);
                Assert.IsTrue(result);
            }
        }
    }
}
