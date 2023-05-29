using BlockadeLabs.Skyboxes;
using NUnit.Framework;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace BlockadeLabs.Tests
{
    public class TestFixture_01_Skyboxes
    {
        [Test]
        public async Task Test_01_GetSkyboxStyles()
        {
            var api = new BlockadeLabsClient();
            Assert.IsNotNull(api.SkyboxEndpoint);
            var result = await api.SkyboxEndpoint.GetSkyboxStylesAsync();
            Assert.IsNotNull(result);

            foreach (var skyboxStyle in result)
            {
                Debug.Log($"{skyboxStyle.Name}");
            }
        }

        [Test]
        public async Task Test_02_GenerateSkybox()
        {
            var api = new BlockadeLabsClient();
            Assert.IsNotNull(api.SkyboxEndpoint);

            var request = new SkyboxRequest("underwater"/*, depth: true*/);
            var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(request);
            Assert.IsNotNull(skyboxInfo);
            Debug.Log(skyboxInfo.MainTextureUrl);
            Assert.IsNotNull(skyboxInfo.MainTexture);
            Debug.Log(skyboxInfo.DepthTextureUrl);
            //Assert.IsNotNull(skyboxInfo.DepthTexture);

            var result = await api.SkyboxEndpoint.GetSkyboxInfoAsync(skyboxInfo.Id);
            Assert.IsNotNull(result);
            Debug.Log($"Skybox: {result.Id} | {result.MainTextureUrl}");
            Assert.IsTrue(skyboxInfo.Id == result.Id);
        }
    }
}
