using System;
using Utilities.WebRequestRest.Interfaces;

namespace BlockadeLabs
{
    public sealed class BlockadeLabsSettingsInfo : ISettingsInfo
    {
        internal const string DefaultDomain = "backend.blockadelabs.com";
        internal const string DefaultVersion = "v1";

        public BlockadeLabsSettingsInfo()
        {
            Domain = DefaultDomain;
            BaseRequestUrlFormat = $"https://{Domain}/api/{DefaultVersion}/{{0}}";
        }

        public BlockadeLabsSettingsInfo(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = DefaultDomain;
            }

            if (!domain.Contains('.') &&
                !domain.Contains(':'))
            {
                throw new ArgumentException($"Invalid parameter \"{nameof(domain)}\"");
            }

            Domain = domain;
            BaseRequestUrlFormat = $"https://{Domain}/api/{DefaultVersion}/{{0}}";
        }

        public string Domain { get; }

        public string BaseRequestUrlFormat { get; }
    }
}
