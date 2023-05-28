using System;
using System.Security.Authentication;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace BlockadeLabs
{
    [Serializable]
    public class BlockadeLabsAuthInfo : IAuthInfo
    {
        public BlockadeLabsAuthInfo(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidCredentialException(nameof(apiKey));
            }

            this.apiKey = apiKey;
        }

        [SerializeField]
        private string apiKey;

        /// <summary>
        /// The API key, required to access the service.
        /// </summary>
        public string ApiKey => apiKey;
    }
}
