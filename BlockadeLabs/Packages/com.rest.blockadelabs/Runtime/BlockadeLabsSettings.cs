using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace BlockadeLabs
{
    public class BlockadeLabsSettings : ISettings
    {
        public BlockadeLabsSettings()
        {
            if (cachedDefault != null) { return; }

            var config = Resources.LoadAll<BlockadeLabsConfiguration>(string.Empty)
                .FirstOrDefault(asset => asset != null);
            Default = config != null
                ? new BlockadeLabsSettings(new BlockadeLabsSettingsInfo(config.ProxyDomain))
                : new BlockadeLabsSettings(new BlockadeLabsSettingsInfo());
        }

        public BlockadeLabsSettings(BlockadeLabsSettingsInfo settingsInfo)
            => this.settingsInfo = settingsInfo;

        public BlockadeLabsSettings(string domain)
            => settingsInfo = new BlockadeLabsSettingsInfo(domain);

        private static BlockadeLabsSettings cachedDefault;

        public static BlockadeLabsSettings Default
        {
            get => cachedDefault ?? new BlockadeLabsSettings();
            internal set => cachedDefault = value;
        }

        private readonly BlockadeLabsSettingsInfo settingsInfo;

        public BlockadeLabsSettingsInfo Info => settingsInfo ?? Default.Info;

        public string BaseRequestUrlFormat => Info.BaseRequestUrlFormat;
    }
}
