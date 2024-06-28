// Licensed under the MIT License. See LICENSE in the project root for license information.

using Utilities.WebRequestRest;

namespace BlockadeLabs
{
    internal static class ResponseExtensions
    {
        private const string RateLimit = "X-RateLimit-Limit";
        private const string RateLimitRemaining = "X-RateLimit-Remaining";

        internal static void SetResponseData(this BaseResponse response, Response restResponse, BlockadeLabsClient client)
        {
            if (response is IListResponse<BaseResponse> listResponse)
            {
                foreach (var item in listResponse.Items)
                {
                    SetResponseData(item, restResponse, client);
                }
            }

            response.Client = client;

            if (restResponse is not { Headers: not null }) { return; }

            if (restResponse.Headers.TryGetValue(RateLimit, out var rateLimit) &&
                int.TryParse(rateLimit, out var rateLimitValue))
            {
                response.RateLimit = rateLimitValue;
            }

            if (restResponse.Headers.TryGetValue(RateLimitRemaining, out var rateLimitRemaining) &&
                int.TryParse(rateLimitRemaining, out var rateLimitRemainingValue))
            {
                response.RateLimitRemaining = rateLimitRemainingValue;
            }
        }
    }
}
