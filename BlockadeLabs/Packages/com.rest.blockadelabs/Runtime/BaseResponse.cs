// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace BlockadeLabs
{
    public abstract class BaseResponse
    {
        [JsonIgnore]
        public BlockadeLabsClient Client { get; internal set; }

        [JsonIgnore]
        public int RateLimit { get; internal set; }

        [JsonIgnore]
        public int RateLimitRemaining { get; internal set; }
    }
}
