using System;
using Utilities.WebRequestRest.Interfaces;

namespace Rest.BlockadeLabs
{
    public class BlockadeLabsSettingsInfo : ISettingsInfo
    {
        internal const string DefaultDomain = "blockade.cloudshell.run";

        public BlockadeLabsSettingsInfo()
        {
            Domain = DefaultDomain;
            BaseRequestUrlFormat = $"https://{Domain}/{{0}}";
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
            BaseRequestUrlFormat = $"https://{Domain}/{{0}}";
        }

        public string Domain { get; }

        public string BaseRequestUrlFormat { get; }
    }
}
