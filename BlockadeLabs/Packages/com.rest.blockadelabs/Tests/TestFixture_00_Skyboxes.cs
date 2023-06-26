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
            var api = new BlockadeLabsClient();
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
    }
}
