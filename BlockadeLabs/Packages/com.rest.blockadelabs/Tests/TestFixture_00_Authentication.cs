// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Authentication;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabs.Tests
{
    internal class TestFixture_00_Authentication
    {
        [SetUp]
        public void Setup()
        {
            var authJson = new BlockadeLabsAuthInfo("key-test12");
            var authText = JsonUtility.ToJson(authJson, true);
            File.WriteAllText(BlockadeLabsAuthentication.CONFIG_FILE, authText);
        }

        [Test]
        public void Test_01_GetAuthFromEnv()
        {
            var auth = BlockadeLabsAuthentication.Default.LoadFromEnvironment();
            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.IsNotEmpty(auth.Info.ApiKey);
        }

        [Test]
        public void Test_02_GetAuthFromFile()
        {
            var auth = BlockadeLabsAuthentication.Default.LoadFromPath(Path.GetFullPath(BlockadeLabsAuthentication.CONFIG_FILE));
            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-test12", auth.Info.ApiKey);
        }

        [Test]
        public void Test_03_GetAuthFromNonExistentFile()
        {
            var auth = BlockadeLabsAuthentication.Default.LoadFromDirectory(filename: "bad.config");
            Assert.IsNull(auth);
        }

        [Test]
        public void Test_04_GetAuthFromConfiguration()
        {
            var configPath = $"Assets/Resources/{nameof(BlockadeLabsConfiguration)}.asset";
            var cleanup = false;

            if (!File.Exists(Path.GetFullPath(configPath)))
            {
                if (!Directory.Exists($"{Application.dataPath}/Resources"))
                {
                    Directory.CreateDirectory($"{Application.dataPath}/Resources");
                }

                var instance = ScriptableObject.CreateInstance<BlockadeLabsConfiguration>();
                instance.ApiKey = "key-test12";
                AssetDatabase.CreateAsset(instance, configPath);
                cleanup = true;
            }

            var config = AssetDatabase.LoadAssetAtPath<BlockadeLabsConfiguration>(configPath);
            var auth = BlockadeLabsAuthentication.Default.LoadFromAsset<BlockadeLabsConfiguration>();

            Assert.IsNotNull(auth);
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.IsNotEmpty(auth.Info.ApiKey);
            Assert.AreEqual(auth.Info.ApiKey, config.ApiKey);

            if (cleanup)
            {
                AssetDatabase.DeleteAsset(configPath);
                AssetDatabase.DeleteAsset("Assets/Resources");
            }
        }

        [Test]
        public void Test_05_Authentication()
        {
            var defaultAuth = BlockadeLabsAuthentication.Default;
            var manualAuth = new BlockadeLabsAuthentication("key-testAA");

            Assert.IsNotNull(defaultAuth);
            Assert.IsNotNull(defaultAuth.Info.ApiKey);
            Assert.AreEqual(defaultAuth.Info.ApiKey, BlockadeLabsAuthentication.Default.Info.ApiKey);

            BlockadeLabsAuthentication.Default = new BlockadeLabsAuthentication("key-testAA");
            Assert.IsNotNull(manualAuth);
            Assert.IsNotNull(manualAuth.Info.ApiKey);
            Assert.AreEqual(manualAuth.Info.ApiKey, BlockadeLabsAuthentication.Default.Info.ApiKey);

            BlockadeLabsAuthentication.Default = defaultAuth;
        }

        [Test]
        public void Test_06_GetKey()
        {
            var auth = new BlockadeLabsAuthentication("key-testAA");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-testAA", auth.Info.ApiKey);
        }

        [Test]
        public void Test_07_GetKeyFailed()
        {
            BlockadeLabsAuthentication auth = null;

            try
            {
                auth = new BlockadeLabsAuthentication("fail-key");
            }
            catch (InvalidCredentialException)
            {
                Assert.IsNull(auth);
            }
            catch (Exception e)
            {
                Assert.IsTrue(false, $"Expected exception {nameof(InvalidCredentialException)} but got {e.GetType().Name}");
            }
        }

        [Test]
        public void Test_08_ParseKey()
        {
            var auth = new BlockadeLabsAuthentication("key-testAA");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-testAA", auth.Info.ApiKey);
            auth = "key-testCC";
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-testCC", auth.Info.ApiKey);

            auth = new BlockadeLabsAuthentication("key-testBB");
            Assert.IsNotNull(auth.Info.ApiKey);
            Assert.AreEqual("key-testBB", auth.Info.ApiKey);
        }

        [Test]
        public void Test_12_CustomDomainConfigurationSettings()
        {
            var auth = new BlockadeLabsAuthentication("customIssuedToken");
            var settings = new BlockadeLabsSettings(domain: "api.your-custom-domain.com");
            var api = new BlockadeLabsClient(auth, settings);
            Debug.Log(api.Settings.Info.BaseRequestUrlFormat);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(BlockadeLabsAuthentication.CONFIG_FILE))
            {
                File.Delete(BlockadeLabsAuthentication.CONFIG_FILE);
            }
        }
    }
}
