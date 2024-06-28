// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Utilities.WebRequestRest.Interfaces;

namespace BlockadeLabs
{
    public sealed class BlockadeLabsAuthentication : AbstractAuthentication<BlockadeLabsAuthentication, BlockadeLabsAuthInfo, BlockadeLabsConfiguration>
    {
        internal const string CONFIG_FILE = ".blockadelabs";
        private const string BLOCKADELABS_API_KEY = nameof(BLOCKADELABS_API_KEY);
        private const string BLOCKADE_LABS_API_KEY = nameof(BLOCKADE_LABS_API_KEY);

        public static implicit operator BlockadeLabsAuthentication(string apiKey) => new BlockadeLabsAuthentication(apiKey);

        /// <summary>
        /// Instantiates an empty Authentication object.
        /// </summary>
        public BlockadeLabsAuthentication() { }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="apiKey"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="apiKey">The API key, required to access the API endpoint.</param>
        public BlockadeLabsAuthentication(string apiKey)
        {
            Info = new BlockadeLabsAuthInfo(apiKey);
            cachedDefault = this;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="authInfo"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="authInfo"></param>
        public BlockadeLabsAuthentication(BlockadeLabsAuthInfo authInfo)
        {
            Info = authInfo;
            cachedDefault = this;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <see cref="configuration"/>.
        /// </summary>
        /// <param name="configuration"><see cref="BlockadeLabsConfiguration"/>.</param>
        public BlockadeLabsAuthentication(BlockadeLabsConfiguration configuration) : this(configuration.ApiKey) { }

        /// <inheritdoc />
        public override BlockadeLabsAuthInfo Info { get; }

        private static BlockadeLabsAuthentication cachedDefault;

        /// <summary>
        /// The default authentication to use when no other auth is specified.
        /// This can be set manually, or automatically loaded via environment variables or a config file.
        /// <seealso cref="LoadFromEnvironment"/><seealso cref="LoadFromDirectory"/>
        /// </summary>
        public static BlockadeLabsAuthentication Default
        {
            get => cachedDefault ??= new BlockadeLabsAuthentication().LoadDefault();
            set => cachedDefault = value;
        }

        public override BlockadeLabsAuthentication LoadFromAsset(BlockadeLabsConfiguration configuration = null)
        {
            if (configuration == null)
            {
                Debug.LogWarning($"This can be speed this up by passing a {nameof(BlockadeLabsConfiguration)} to the {nameof(BlockadeLabsAuthentication)}.ctr");
                configuration = Resources.LoadAll<BlockadeLabsConfiguration>(string.Empty).FirstOrDefault(o => o != null);
            }

            return configuration != null ? new BlockadeLabsAuthentication(configuration) : null;
        }

        /// <inheritdoc />
        public override BlockadeLabsAuthentication LoadFromEnvironment()
        {
            var apiKey = Environment.GetEnvironmentVariable(BLOCKADELABS_API_KEY);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable(BLOCKADE_LABS_API_KEY);
            }

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

            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = CONFIG_FILE;
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
                                case BLOCKADELABS_API_KEY:
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

            return string.IsNullOrEmpty(tempAuthInfo?.ApiKey) ? null : new BlockadeLabsAuthentication(tempAuthInfo);
        }
    }
}
