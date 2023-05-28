using BlockadeLabs.Skyboxes;
using NUnit.Framework;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace BlockadeLabs.Tests
{
    public class TestFixture_01_Skyboxes
    {
        [Test]
        public async Task GenerateSkybox()
        {
            var api = new BlockadeLabsClient();
            Assert.IsNotNull(api.SkyboxEndpoint);

            var request = new SkyboxRequest("underwater");
            var result = await api.SkyboxEndpoint.GenerateSkyboxAsync(request);
            Debug.Log(result);
        }
    }
}
