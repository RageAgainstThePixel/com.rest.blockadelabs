// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace BlockadeLabs.Tests
{
    internal abstract class AbstractTestFixture
    {
        protected readonly BlockadeLabsClient BlockadeLabsClient;

        protected AbstractTestFixture()
        {
            var auth = new BlockadeLabsAuthentication().LoadDefaultsReversed();
            var settings = new BlockadeLabsSettings();
            BlockadeLabsClient = new BlockadeLabsClient(auth, settings);
            //BlockadeLabsClient.EnableDebug = true;
        }
    }
}
