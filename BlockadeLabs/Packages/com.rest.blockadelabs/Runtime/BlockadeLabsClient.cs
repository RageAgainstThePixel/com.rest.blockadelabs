// Licensed under the MIT License. See LICENSE in the project root for license information.

using BlockadeLabs.Skyboxes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Security.Authentication;
using Utilities.WebRequestRest;

namespace BlockadeLabs
{
    public sealed class BlockadeLabsClient : BaseClient<BlockadeLabsAuthentication, BlockadeLabsSettings>
    {
        /// <inheritdoc />
        public BlockadeLabsClient(BlockadeLabsConfiguration configuration)
            : this(
                configuration != null ? new BlockadeLabsAuthentication(configuration) : BlockadeLabsAuthentication.Default,
                configuration != null ? new BlockadeLabsSettings(configuration) : BlockadeLabsSettings.Default)
        {
        }

        /// <summary>
        /// Creates a new client for the BlockadeLabs API, handling auth and allowing for access to various API endpoints.
        /// </summary>
        /// <param name="authentication">The API authentication information to use for API calls,
        /// or <see langword="null"/> to attempt to use the <see cref="BlockadeLabsAuthentication.Default"/>,
        /// potentially loading from environment vars or from a config file.</param>
        /// <param name="settings">Optional, <see cref="BlockadeLabsSettings"/> for specifying a proxy domain.</param>
        /// <exception cref="AuthenticationException">Raised when authentication details are missing or invalid.</exception>
        public BlockadeLabsClient(BlockadeLabsAuthentication authentication = null, BlockadeLabsSettings settings = null)
            : base(authentication ?? BlockadeLabsAuthentication.Default, settings ?? BlockadeLabsSettings.Default)
        {
            SkyboxEndpoint = new SkyboxEndpoint(this);
        }

        protected override void ValidateAuthentication()
        {
            if (Authentication?.Info == null)
            {
                throw new InvalidCredentialException($"Invalid {nameof(BlockadeLabsAuthentication)}");
            }

            if (!HasValidAuthentication)
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/com.rest.blockadelabs#authentication for details.");
            }
        }

        /// <inheritdoc />
        public override bool HasValidAuthentication => !string.IsNullOrWhiteSpace(Authentication?.Info?.ApiKey);

        protected override void SetupDefaultRequestHeaders()
        {
            DefaultRequestHeaders = new Dictionary<string, string>
            {
#if !UNITY_WEBGL
                { "User-Agent", "com.rest.blockadelabs" },
#endif
                { "x-api-key", Authentication.Info.ApiKey }
            };
        }

        internal static JsonSerializerSettings JsonSerializationOptions { get; } = new()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter()
            }
        };

        public SkyboxEndpoint SkyboxEndpoint { get; }
    }
}
