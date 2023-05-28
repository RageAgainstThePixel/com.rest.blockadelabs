using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace BlockadeLabs
{
    public sealed class BlockadeLabsAuthentication : AbstractAuthentication<BlockadeLabsAuthentication, BlockadeLabsAuthInfo>
    {
        internal const string CONFIG_FILE = ".blockadelabs";
        private const string BLOCKADE_LABS_API_KEY = nameof(BLOCKADE_LABS_API_KEY);

        public static implicit operator BlockadeLabsAuthentication(string apiKey) => new BlockadeLabsAuthentication(apiKey);

        /// <summary>
        /// Instantiates a new Authentication object that will load the default config.
        /// </summary>
        public BlockadeLabsAuthentication()
            => cachedDefault ??= (LoadFromAsset<BlockadeLabsConfiguration>() ??
                                  LoadFromDirectory()) ??
                                  LoadFromDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) ??
                                  LoadFromEnvironment();

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="apiKey"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="apiKey">The API key, required to access the API endpoint.</param>
        public BlockadeLabsAuthentication(string apiKey) => authInfo = new BlockadeLabsAuthInfo(apiKey);

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="authInfo"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="authInfo"></param>
        public BlockadeLabsAuthentication(BlockadeLabsAuthInfo authInfo) => this.authInfo = authInfo;

        public readonly BlockadeLabsAuthInfo authInfo;

        /// <inheritdoc />
        public override BlockadeLabsAuthInfo Info => authInfo ?? Default.Info;

        private static BlockadeLabsAuthentication cachedDefault;

        /// <summary>
        /// The default authentication to use when no other auth is specified.
        /// This can be set manually, or automatically loaded via environment variables or a config file.
        /// <seealso cref="LoadFromEnvironment"/><seealso cref="LoadFromDirectory"/>
        /// </summary>
        public static BlockadeLabsAuthentication Default
        {
            get => cachedDefault ?? new BlockadeLabsAuthentication();
            internal set => cachedDefault = value;
        }

        /// <inheritdoc />
        public override BlockadeLabsAuthentication LoadFromAsset<T>()
            => Resources.LoadAll<T>(string.Empty)
                .Where(asset => asset != null)
                .Where(asset => asset is BlockadeLabsConfiguration config &&
                                !string.IsNullOrWhiteSpace(config.ApiKey))
                .Select(asset => asset is BlockadeLabsConfiguration config
                    ? new BlockadeLabsAuthentication(config.ApiKey)
                    : null)
                .FirstOrDefault();

        /// <inheritdoc />
        public override BlockadeLabsAuthentication LoadFromEnvironment()
        {
            var apiKey = Environment.GetEnvironmentVariable(BLOCKADE_LABS_API_KEY);
            return string.IsNullOrEmpty(apiKey) ? null : new BlockadeLabsAuthentication(apiKey);
        }

        /// <inheritdoc />
        /// ReSharper disable once OptionalParameterHierarchyMismatch
        public override BlockadeLabsAuthentication LoadFromDirectory(string directory = null, string filename = CONFIG_FILE, bool searchUp = true)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Environment.CurrentDirectory;
            }

            BlockadeLabsAuthInfo tempAuthInfo = null;

            var currentDirectory = new DirectoryInfo(directory);

            while (tempAuthInfo == null && currentDirectory.Parent != null)
            {
                var filePath = Path.Combine(currentDirectory.FullName, filename);

                if (File.Exists(filePath))
                {
                    try
                    {
                        tempAuthInfo = JsonUtility.FromJson<BlockadeLabsAuthInfo>(File.ReadAllText(filePath));
                        break;
                    }
                    catch (Exception)
                    {
                        // try to parse the old way for backwards support.
                    }

                    var lines = File.ReadAllLines(filePath);
                    string apiKey = null;

                    foreach (var line in lines)
                    {
                        var parts = line.Split('=', ':');

                        for (var i = 0; i < parts.Length - 1; i++)
                        {
                            var part = parts[i];
                            var nextPart = parts[i + 1];

                            switch (part)
                            {
                                case BLOCKADE_LABS_API_KEY:
                                    apiKey = nextPart.Trim();
                                    break;
                            }
                        }
                    }

                    tempAuthInfo = new BlockadeLabsAuthInfo(apiKey);
                }

                if (searchUp)
                {
                    currentDirectory = currentDirectory.Parent;
                }
                else
                {
                    break;
                }
            }

            if (tempAuthInfo == null ||
                string.IsNullOrEmpty(tempAuthInfo.ApiKey))
            {
                return null;
            }

            return new BlockadeLabsAuthentication(tempAuthInfo);
        }
    }
}
