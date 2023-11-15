// Licensed under the MIT License. See LICENSE in the project root for license information.

using BlockadeLabs.Skyboxes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Authentication;
using Utilities.WebRequestRest;

namespace BlockadeLabs
{
    public sealed class BlockadeLabsClient : BaseClient<BlockadeLabsAuthentication, BlockadeLabsSettings>
    {
        public BlockadeLabsClient(BlockadeLabsAuthentication authentication = null, BlockadeLabsSettings settings = null)
            : base(authentication ?? BlockadeLabsAuthentication.Default, settings ?? BlockadeLabsSettings.Default)
        {
            SkyboxEndpoint = new SkyboxEndpoint(this);
        }

        protected override void ValidateAuthentication()
        {
            if (!HasValidAuthentication)
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/com.rest.blockadelabs#authentication for details.");
            }
        }

        protected override void SetupDefaultRequestHeaders()
        {
            DefaultRequestHeaders = new Dictionary<string, string>
            {
#if !UNITY_WEBGL
                {"User-Agent", "com.rest.blockadelabs" },
#endif
                {"x-api-key", Authentication.Info.ApiKey }
            };
        }

        public override bool HasValidAuthentication => !string.IsNullOrWhiteSpace(Authentication?.Info?.ApiKey);

        internal static JsonSerializerSettings JsonSerializationOptions { get; } = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public SkyboxEndpoint SkyboxEndpoint { get; }
    }
}
