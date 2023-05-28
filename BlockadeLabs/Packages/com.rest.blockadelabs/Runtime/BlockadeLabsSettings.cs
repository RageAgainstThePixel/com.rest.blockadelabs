using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace BlockadeLabs
{
    public sealed class BlockadeLabsSettings : ISettings
    {
        public BlockadeLabsSettings()
        {
            if (cachedDefault != null) { return; }

            var config = Resources.LoadAll<BlockadeLabsConfiguration>(string.Empty)
                .FirstOrDefault(asset => asset != null);

            if (config != null)
            {
                Info = new BlockadeLabsSettingsInfo(config.ProxyDomain);
                Default = new BlockadeLabsSettings(Info);
            }
            else
            {
                Info = new BlockadeLabsSettingsInfo();
                Default = new BlockadeLabsSettings(Info);
            }
        }

        public BlockadeLabsSettings(BlockadeLabsSettingsInfo settingsInfo)
            => Info = settingsInfo;

        public BlockadeLabsSettings(string domain)
            => Info = new BlockadeLabsSettingsInfo(domain);

        private static BlockadeLabsSettings cachedDefault;

        public static BlockadeLabsSettings Default
        {
            get => cachedDefault ??= new BlockadeLabsSettings();
            internal set => cachedDefault = value;
        }

        public BlockadeLabsSettingsInfo Info { get; }

        public string BaseRequestUrlFormat => Info.BaseRequestUrlFormat;
    }
}
