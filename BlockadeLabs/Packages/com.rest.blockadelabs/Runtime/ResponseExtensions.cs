// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace BlockadeLabs
{
    internal static class ResponseExtensions
    {
        internal static void SetResponseData(this BaseResponse response, BlockadeLabsClient client)
        {
            if (response is IListResponse<BaseResponse> listResponse)
            {
                foreach (var item in listResponse.Items)
                {
                    SetResponseData(item, client);
                }
            }

            response.Client = client;
        }

        internal static void SetResponseData(this IReadOnlyList<BaseResponse> responseData, BlockadeLabsClient client)
        {
            foreach (var response in responseData)
            {
                response.SetResponseData(client);
            }
        }
    }
}
