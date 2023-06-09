// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace BlockadeLabs
{
    [CreateAssetMenu(fileName = nameof(BlockadeLabsConfiguration), menuName = nameof(BlockadeLabs) + "/" + nameof(BlockadeLabsConfiguration), order = 0)]
    public sealed class BlockadeLabsConfiguration : ScriptableObject, IConfiguration
    {
        [SerializeField]
        [Tooltip("The api key.")]
        private string apiKey;

        public string ApiKey
        {
            get => apiKey;
            internal set => apiKey = value;
        }

        [SerializeField]
        [Tooltip("Optional proxy domain to make requests though.")]
        private string proxyDomain;

        public string ProxyDomain => proxyDomain;
    }
}
