using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Authentication;
using Utilities.WebRequestRest;

namespace BlockadeLabs
{
    public class BlockadeLabsClient : BaseClient<BlockadeLabsAuthentication, BlockadeLabsSettings>
    {
        public BlockadeLabsClient(BlockadeLabsAuthentication authentication = null, BlockadeLabsSettings settings = null, HttpClient httpClient = null)
            : base(authentication ?? BlockadeLabsAuthentication.Default, settings ?? BlockadeLabsSettings.Default, httpClient)
        {
            JsonSerializationOptions = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        protected override HttpClient SetupClient(HttpClient httpClient = null)
        {
            httpClient ??= new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "com.rest.blockadelabs");
            httpClient.DefaultRequestHeaders.Add("X-API-Key", Authentication.Info.ApiKey);
            return httpClient;
        }

        protected override void ValidateAuthentication()
        {
            if (!HasValidAuthentication)
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/com.rest.blockadelabs#authentication for details.");
            }
        }

        public override bool HasValidAuthentication => !string.IsNullOrWhiteSpace(Authentication.Info.ApiKey);

        internal JsonSerializerSettings JsonSerializationOptions { get; }
    }
}
