// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine.Networking;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxHistoryParameters
    {
        /// <summary>
        /// Filter by status.<br/>
        /// Options: all, pending, dispatched, processing, complete, abort, error (default: all)
        /// </summary>
        public Status? StatusFilter { get; set; }

        /// <summary>
        /// Number of items to be returned per page (default: 18)
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Page number (default: 0)
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// Sort order. Options: ASC, DESC (default: DESC)
        /// </summary>
        public SortOrder? Order { get; set; }

        /// <summary>
        /// Filter by id
        /// </summary>
        public int? ImagineId { get; set; }

        /// <summary>
        /// Filter by title or prompt
        /// </summary>
        public string QueryFilter { get; set; }

        /// <summary>
        /// Filter by generator
        /// </summary>
        public string GeneratorFilter { get; set; }

        public override string ToString()
        {
            var @params = new List<string>();

            if (StatusFilter.HasValue)
            {
                @params.Add($"status={StatusFilter.ToString().ToLower()}");
            }

            if (Limit.HasValue)
            {
                @params.Add($"limit={Limit}");
            }

            if (Offset.HasValue)
            {
                @params.Add($"offset={Offset}");
            }

            if (Order.HasValue)
            {
                @params.Add($"order={Order.ToString().ToUpper()}");
            }

            if (ImagineId.HasValue)
            {
                @params.Add($"imagine_id={ImagineId}");
            }

            if (!string.IsNullOrWhiteSpace(QueryFilter))
            {
                @params.Add($"query={UnityWebRequest.EscapeURL(QueryFilter)}");
            }

            if (!string.IsNullOrWhiteSpace(GeneratorFilter))
            {
                @params.Add($"generator={UnityWebRequest.EscapeURL(GeneratorFilter)}");
            }

            return @params.Count == 0 ? string.Empty : $"?{string.Join('&', @params)}";
        }
    }
}
