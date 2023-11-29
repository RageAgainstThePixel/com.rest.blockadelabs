// Licensed under the MIT License. See LICENSE in the project root for license information.

using BlockadeLabs.Skyboxes;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BlockadeLabs.Tests
{
    internal class TestFixture_01_Skyboxes : AbstractTestFixture
    {
        private SkyboxInfo testSkyboxInfo;

        [Test]
        public async Task Test_01_GetSkyboxStyles()
        {
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            var skyboxStyles = await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxStylesAsync();
            Assert.IsNotNull(skyboxStyles);

            foreach (var skyboxStyle in skyboxStyles)
            {
                Debug.Log($"{skyboxStyle.Name}");
            }
        }

        [Test]
        public async Task Test_02_GenerateSkybox()
        {
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            var request = new SkyboxRequest("mars", enhancePrompt: true);
            var skyboxInfo = await BlockadeLabsClient.SkyboxEndpoint.GenerateSkyboxAsync(request);
            Assert.IsNotNull(skyboxInfo);
            Debug.Log($"Successfully created skybox: {skyboxInfo.Id}");

            Assert.IsNotEmpty(skyboxInfo.Exports);
            Assert.IsNotEmpty(skyboxInfo.ExportedAssets);

            foreach (var exportInfo in skyboxInfo.Exports)
            {
                Debug.Log($"{exportInfo.Key} -> {exportInfo.Value}");
            }

            foreach (var exportedAsset in skyboxInfo.ExportedAssets)
            {
                Debug.Log(exportedAsset.Key);
                Assert.IsNotNull(exportedAsset.Value);
            }

            Debug.Log(skyboxInfo.ToString());
            testSkyboxInfo = skyboxInfo;
        }

        [Test]
        public async Task Test_03_GetSkyboxInfo()
        {
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            Assert.IsNotNull(testSkyboxInfo);
            var skyboxId = testSkyboxInfo.Id;
            var skyboxInfo = await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxInfoAsync(skyboxId);
            Assert.IsNotNull(skyboxInfo);
            await skyboxInfo.LoadAssetsAsync();
            Assert.IsNotEmpty(skyboxInfo.Exports);
            Assert.IsNotEmpty(skyboxInfo.ExportedAssets);

            foreach (var exportInfo in skyboxInfo.Exports)
            {
                Debug.Log($"{exportInfo.Key} -> {exportInfo.Value}");
            }

            foreach (var exportedAsset in skyboxInfo.ExportedAssets)
            {
                Debug.Log(exportedAsset.Key);
                Assert.IsNotNull(exportedAsset.Value);
            }
        }

        [Test]
        public async Task Test_04_GetHistory()
        {
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            var history = await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxHistoryAsync();
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
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            var request = new SkyboxRequest("mars", enhancePrompt: true);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));

            try
            {
                await BlockadeLabsClient.SkyboxEndpoint.GenerateSkyboxAsync(request, pollingInterval: 1, cancellationToken: cts.Token);
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
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            var result = await BlockadeLabsClient.SkyboxEndpoint.CancelAllPendingSkyboxGenerationsAsync();
            Debug.Log(result ? "All pending generations successfully cancelled" : "No pending generations");
        }

        [Test]
        public async Task Test_07_DeleteSkybox()
        {
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            var history = await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxHistoryAsync(new SkyboxHistoryParameters { StatusFilter = Status.Abort });
            Assert.IsNotNull(history);

            foreach (var skybox in history.Skyboxes)
            {
                Debug.Log($"Deleting {skybox.Id} {skybox.Title}");
                var result = await BlockadeLabsClient.SkyboxEndpoint.DeleteSkyboxAsync(skybox);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public async Task Test_08_GetSkyboxExportOptions()
        {
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            var exportOptions = await BlockadeLabsClient.SkyboxEndpoint.GetAllSkyboxExportOptionsAsync();
            Assert.IsNotNull(exportOptions);
            Assert.IsNotEmpty(exportOptions);

            foreach (var exportOption in exportOptions)
            {
                Debug.Log($"{exportOption.Id}: {exportOption.Name} | {exportOption.Key}");
            }
        }

        [Test]
        public async Task Test_09_ExportSkybox_All_Options()
        {
            Assert.IsNotNull(BlockadeLabsClient.SkyboxEndpoint);
            Assert.IsNotNull(testSkyboxInfo);
            var skyboxId = testSkyboxInfo.Id;
            var skyboxInfo = await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxInfoAsync(skyboxId);
            Assert.IsNotNull(skyboxInfo);
            var exportOptions = await BlockadeLabsClient.SkyboxEndpoint.GetAllSkyboxExportOptionsAsync();
            Assert.IsNotNull(exportOptions);
            Assert.IsNotEmpty(exportOptions);

            foreach (var exportOption in exportOptions)
            {
                Debug.Log(exportOption.Key);
                Assert.IsNotNull(exportOption);
                skyboxInfo = await BlockadeLabsClient.SkyboxEndpoint.ExportSkyboxAsync(skyboxInfo, exportOption);
                Assert.IsNotNull(skyboxInfo);
                Assert.IsTrue(skyboxInfo.Exports.ContainsKey(exportOption.Key));
                skyboxInfo.Exports.TryGetValue(exportOption.Key, out var exportUrl);
                Debug.Log(exportUrl);
            }

            if (skyboxInfo.Exports.Count > 0)
            {
                foreach (var exportInfo in skyboxInfo.Exports)
                {
                    Debug.Log($"{exportInfo.Key} -> {exportInfo.Value}");
                }
            }
        }
    }
}
