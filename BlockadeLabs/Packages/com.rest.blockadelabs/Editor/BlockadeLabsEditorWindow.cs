// Licensed under the MIT License. See LICENSE in the project root for license information.

using BlockadeLabs.Skyboxes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using Utilities.Async;
using Utilities.WebRequestRest;
using Object = UnityEngine.Object;
using Progress = UnityEditor.Progress;
using Task = System.Threading.Tasks.Task;

namespace BlockadeLabs.Editor
{
    public sealed class BlockadeLabsEditorWindow : EditorWindow
    {
        private const int TabWidth = 18;
        private const int EndWidth = 10;
        private const int InnerLabelIndentLevel = 13;
        private const int MaxCharacterLength = 5000;

        private const float InnerLabelWidth = 1.9f;
        private const float DefaultColumnWidth = 96f;
        private const float WideColumnWidth = 128f;
        private const float SettingsLabelWidth = 1.56f;

        private static readonly GUIContent guiTitleContent = new GUIContent("BlockadeLabs Dashboard");
        private static readonly GUIContent saveDirectoryContent = new GUIContent("Save Directory");
        private static readonly GUIContent downloadContent = new GUIContent("Download");
        private static readonly GUIContent deleteContent = new GUIContent("Delete");
        private static readonly GUIContent refreshContent = new GUIContent("Refresh");

        private static readonly string[] tabTitles = { "Skybox Generation", "History" };

        private static GUIStyle boldCenteredHeaderStyle;

        private static GUIStyle BoldCenteredHeaderStyle
        {
            get
            {
                if (boldCenteredHeaderStyle == null)
                {
                    var editorStyle = EditorGUIUtility.isProSkin ? EditorStyles.whiteLargeLabel : EditorStyles.largeLabel;

                    if (editorStyle != null)
                    {
                        boldCenteredHeaderStyle = new GUIStyle(editorStyle)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18,
                            padding = new RectOffset(0, 0, -8, -8)
                        };
                    }
                }

                return boldCenteredHeaderStyle;
            }
        }

        private static string DefaultSaveDirectoryKey => $"{Application.productName}_BlockadeLabs_EditorDownloadDirectory";

        private static string DefaultSaveDirectory => Application.dataPath;

        private static readonly GUILayoutOption[] defaultColumnWidthOption =
        {
            GUILayout.Width(DefaultColumnWidth)
        };

        private static readonly GUILayoutOption[] wideColumnWidthOption =
        {
            GUILayout.Width(WideColumnWidth)
        };

        private static readonly GUILayoutOption[] expandWidthOption =
        {
            GUILayout.ExpandWidth(true)
        };

        #region static content

        private static BlockadeLabsClient api;

        private static string editorDownloadDirectory = string.Empty;

        private static bool isGeneratingSkybox;

        private static IReadOnlyList<SkyboxStyle> skyboxStyles = new List<SkyboxStyle>();

        private static GUIContent[] skyboxOptions = Array.Empty<GUIContent>();

        private static SkyboxStyle currentSkyboxStyleSelection;

        private static bool isFetchingSkyboxStyles;
        private static bool hasFetchedSkyboxStyles;

        private static IReadOnlyList<SkyboxExportOption> skyboxExportOptions = new List<SkyboxExportOption>();

        private static bool isFetchingSkyboxExportOptions;
        private static bool hasFetchedSkyboxExportOptions;

        private static bool hasFetchedHistory;
        private static bool isFetchingSkyboxHistory;

        private static SkyboxHistory history;

        private static int page;

        private static int limit = 100;

        #endregion static content

        [SerializeField]
        private int tab;

        [SerializeField]
        private int currentSkyboxStyleId;

        private Vector2 scrollPosition = Vector2.zero;

        private string promptText;

        private string negativeText;

        private int seed;

        private bool enhancePrompt;

        private Texture2D controlImage;

        [MenuItem("BlockadeLabs/Dashboard")]
        private static void OpenWindow()
        {
            // Dock it next to the Scene View.
            var instance = GetWindow<BlockadeLabsEditorWindow>(typeof(SceneView));
            instance.Show();
            instance.titleContent = guiTitleContent;
        }

        private void OnEnable()
        {
            titleContent = guiTitleContent;
            minSize = new Vector2(WideColumnWidth * 5, WideColumnWidth * 4);
        }

        private void OnFocus()
        {
            api ??= new BlockadeLabsClient();

            if (!hasFetchedSkyboxStyles)
            {
                hasFetchedSkyboxStyles = true;
                FetchSkyboxStyles();
            }

            if (!hasFetchedSkyboxExportOptions)
            {
                hasFetchedSkyboxExportOptions = true;
                FetchSkyboxExportOptions();
            }

            if (!hasFetchedHistory)
            {
                hasFetchedHistory = true;
                FetchSkyboxHistory();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, expandWidthOption);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TabWidth);
            EditorGUILayout.BeginVertical();
            { // Begin Header
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("BlockadeLabs Dashboard", BoldCenteredHeaderStyle);
                EditorGUILayout.Space();

                if (api is not { HasValidAuthentication: true })
                {
                    EditorGUILayout.HelpBox($"No valid {nameof(BlockadeLabsConfiguration)} was found. This tool requires that you set your API key.", MessageType.Error);
                    return;
                }

                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                tab = GUILayout.Toolbar(tab, tabTitles, expandWidthOption);

                if (EditorGUI.EndChangeCheck())
                {
                    GUI.FocusControl(null);
                }

                EditorGUILayout.LabelField(saveDirectoryContent);

                if (string.IsNullOrWhiteSpace(editorDownloadDirectory))
                {
                    editorDownloadDirectory = EditorPrefs.GetString(DefaultSaveDirectoryKey, DefaultSaveDirectory);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.TextField(editorDownloadDirectory, expandWidthOption);

                    if (GUILayout.Button("Reset", wideColumnWidthOption))
                    {
                        editorDownloadDirectory = DefaultSaveDirectory;
                        EditorPrefs.SetString(DefaultSaveDirectoryKey, editorDownloadDirectory);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Change Save Directory", expandWidthOption))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            var result = EditorUtility.OpenFolderPanel("Save Directory", editorDownloadDirectory, string.Empty);

                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                editorDownloadDirectory = result;
                                EditorPrefs.SetString(DefaultSaveDirectoryKey, editorDownloadDirectory);
                            }
                        };
                    }
                }
                EditorGUILayout.EndHorizontal();
            } // End Header
            EditorGUILayout.EndVertical();
            GUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            switch (tab)
            {
                case 0:
                    SkyboxGeneration();
                    break;
                case 1:
                    RenderHistory();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void SkyboxGeneration()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TabWidth);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Describe your world");
            promptText = EditorGUILayout.TextArea(promptText);
            EditorGUILayout.LabelField("Negative text");
            negativeText = EditorGUILayout.TextArea(negativeText);
            EditorGUILayout.Space();
            enhancePrompt = EditorGUILayout.Toggle("Enhance Prompt", enhancePrompt);
            seed = EditorGUILayout.IntField("Seed", seed);
            controlImage = EditorGUILayout.ObjectField(new GUIContent("Control Image"), controlImage, typeof(Texture2D), false) as Texture2D;

            EditorGUILayout.BeginHorizontal();
            { // skybox style dropdown
                var skyboxIndex = -1;

                currentSkyboxStyleSelection ??= skyboxStyles?.FirstOrDefault(skyboxStyle => skyboxStyle.Id == currentSkyboxStyleId);

                if (currentSkyboxStyleSelection != null)
                {
                    for (var i = 0; i < skyboxOptions.Length; i++)
                    {
                        if (skyboxOptions[i].text.Contains(currentSkyboxStyleSelection.Name))
                        {
                            skyboxIndex = i;
                            break;
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                skyboxIndex = EditorGUILayout.Popup(new GUIContent("Style"), skyboxIndex, skyboxOptions);

                if (EditorGUI.EndChangeCheck())
                {
                    currentSkyboxStyleSelection = skyboxStyles?.FirstOrDefault(voice => skyboxOptions[skyboxIndex].text.Contains(voice.Name));
                    currentSkyboxStyleId = currentSkyboxStyleSelection!.Id;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            GUI.enabled = !isGeneratingSkybox;

            if (GUILayout.Button("Generate"))
            {
                GenerateSkybox();
            }

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            GUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
        }

        private static async void FetchSkyboxStyles()
        {
            if (isFetchingSkyboxStyles) { return; }
            isFetchingSkyboxStyles = true;

            try
            {
                skyboxStyles = await api.SkyboxEndpoint.GetSkyboxStylesAsync();
                skyboxOptions = skyboxStyles.Select(skyboxStyle => new GUIContent(skyboxStyle.Name)).ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingSkyboxStyles = false;
            }
        }

        private static async void FetchSkyboxExportOptions()
        {
            if (isFetchingSkyboxExportOptions) { return; }
            isFetchingSkyboxExportOptions = true;

            try
            {
                skyboxExportOptions = await api.SkyboxEndpoint.GetExportOptionsAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingSkyboxExportOptions = false;
            }
        }

        private async void GenerateSkybox()
        {
            if (isGeneratingSkybox) { return; }
            isGeneratingSkybox = true;
            int? progressId = null;

            try
            {
                if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(controlImage)) is TextureImporter textureImporter)
                {
                    if (!textureImporter.isReadable)
                    {
                        throw new Exception($"Enable Read/Write in Texture asset import settings for {controlImage.name}");
                    }

                    if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        throw new Exception($"Disable compression in Texture asset import settings for {controlImage.name}");
                    }
                }

                using var skyboxRequest = controlImage == null
                    ? new SkyboxRequest(
                        prompt: promptText,
                        negativeText: negativeText,
                        enhancePrompt: enhancePrompt,
                        skyboxStyleId: currentSkyboxStyleSelection?.Id,
                        seed: seed)
                    : new SkyboxRequest(
                        prompt: promptText,
                        negativeText: negativeText,
                        enhancePrompt: enhancePrompt,
                        controlImage: controlImage,
                        skyboxStyleId: currentSkyboxStyleSelection?.Id,
                        seed: seed);
                promptText = string.Empty;
                negativeText = string.Empty;
                controlImage = null;
                progressId = Progress.Start("Generating Skybox", promptText, options: Progress.Options.Indefinite);
                var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(skyboxRequest);
                await SaveSkyboxAssetAsync(skyboxInfo);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                if (progressId.HasValue)
                {
                    Progress.Finish(progressId.Value);
                }

                isGeneratingSkybox = false;
                FetchSkyboxHistory();
            }
        }

        private static readonly ConcurrentDictionary<string, SkyboxInfo> loadingSkyboxes = new ConcurrentDictionary<string, SkyboxInfo>();

        private static async void SaveAllSkyboxAssets(SkyboxInfo skyboxInfo)
        {
            if (loadingSkyboxes.TryAdd(skyboxInfo.ObfuscatedId, skyboxInfo))
            {
                await skyboxInfo.LoadAssetsAsync();
                await SaveSkyboxAssetAsync(skyboxInfo);
                loadingSkyboxes.TryRemove(skyboxInfo.ObfuscatedId, out _);
            }
            else
            {
                Debug.LogWarning($"Skybox {skyboxInfo.ObfuscatedId} is already loading assets. Try again later.");
            }
        }

        private static async Task SaveSkyboxAssetAsync(SkyboxInfo skyboxInfo)
        {
            await Awaiters.UnityMainThread;

            if (skyboxInfo.MainTexture == null)
            {
                Debug.LogError("No main texture found!");
                return;
            }

            var directory = GetLocalPath(editorDownloadDirectory);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var mainTexturePath = string.Empty;
            var depthTexturePath = string.Empty;
            var mainTextureBytes = skyboxInfo.MainTexture != null ? skyboxInfo.MainTexture.EncodeToPNG() : Array.Empty<byte>();

            if (mainTextureBytes.Length > 0)
            {
                mainTexturePath = $"{directory}/{skyboxInfo.ObfuscatedId}-albedo.png";
                Debug.Log(mainTexturePath);
            }

            var depthTextureBytes = skyboxInfo.DepthTexture != null ? skyboxInfo.DepthTexture.EncodeToPNG() : Array.Empty<byte>();

            if (depthTextureBytes.Length > 0)
            {
                depthTexturePath = $"{directory}/{skyboxInfo.ObfuscatedId}-depth.png";
                Debug.Log(depthTexturePath);
            }

            var importTasks = new List<Task>
            {
                Task.Run(async () =>
                {
                    if (mainTextureBytes.Length > 0)
                    {
                        await SaveTextureAsync(mainTexturePath, mainTextureBytes);
                    }
                }),
                Task.Run(async () =>
                {
                    if (depthTextureBytes.Length > 0)
                    {
                        await SaveTextureAsync(depthTexturePath, depthTextureBytes);
                    }
                })
            };

            await Task.WhenAll(importTasks).ConfigureAwait(true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            EditorApplication.delayCall += () =>
            {
                SetSkyboxTextureImportSettings(mainTexturePath);

                if (!string.IsNullOrWhiteSpace(depthTexturePath))
                {
                    SetSkyboxTextureImportSettings(depthTexturePath);
                }

                EditorApplication.delayCall += () =>
                {
                    var mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(mainTexturePath);
                    EditorGUIUtility.PingObject(mainTexture);
                    EditorApplication.delayCall += AssetDatabase.SaveAssets;
                };
            };
        }

        private static void SetSkyboxTextureImportSettings(string path)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter textureImporter)
            {
                textureImporter.isReadable = true;
                textureImporter.alphaIsTransparency = false;
                textureImporter.mipmapEnabled = false;
                textureImporter.maxTextureSize = 6144;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                textureImporter.alphaSource = TextureImporterAlphaSource.None;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.SaveAndReimport();
            }
        }

        private static async Task SaveTextureAsync(string path, byte[] pngBytes)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                File.Delete($"{path}.meta");
            }

            var fileStream = File.OpenWrite(path);

            try
            {
                await fileStream.WriteAsync(pngBytes, 0, pngBytes.Length);
                await fileStream.FlushAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write texture to disk!\n{e}");
            }
            finally
            {
                fileStream.Close();
                await fileStream.DisposeAsync();
            }
        }

        private void RenderHistory()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TabWidth);
            { //Header
                EditorGUILayout.LabelField("History", EditorStyles.boldLabel, wideColumnWidthOption);

                GUI.enabled = !isFetchingSkyboxHistory;
                if (history != null && page > 0 && GUILayout.Button("Prev Page"))
                {
                    page--;
                    EditorApplication.delayCall += FetchSkyboxHistory;
                }

                if (history is { HasMore: true } && GUILayout.Button("Next Page"))
                {
                    page++;
                    EditorApplication.delayCall += FetchSkyboxHistory;
                }

                EditorGUI.BeginChangeCheck();
                limit = EditorGUILayout.IntField("page items", limit);

                if (EditorGUI.EndChangeCheck())
                {
                    if (limit > 100)
                    {
                        limit = 100;
                    }

                    if (limit < 1)
                    {
                        limit = 1;
                    }
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(refreshContent, defaultColumnWidthOption))
                {
                    EditorApplication.delayCall += FetchSkyboxHistory;
                }

                GUI.enabled = true;
            }
            GUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (history == null) { return; }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TabWidth);
            EditorGUILayout.BeginVertical();

            foreach (var skybox in history.Skyboxes)
            {
                GUILayout.Space(TabWidth);
                Utilities.Extensions.Editor.EditorGUILayoutExtensions.Divider();
                var albedoPath = GetFullLocalPath(skybox, "albedo", ".png");
                var depthPath = GetFullLocalPath(skybox, "depth", ".png");

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                { // skybox title
                    EditorGUILayout.LabelField($"{skybox.Title} {skybox.Status} {skybox.CreatedAt}");

                    if (GUILayout.Button("Delete", defaultColumnWidthOption))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            DeleteSkybox(skybox, albedoPath, depthPath);
                        };
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                { // Image and Depth textures
                    if (!File.Exists(albedoPath))
                    {
                        GUI.enabled = !loadingSkyboxes.TryGetValue(skybox.ObfuscatedId, out _);

                        if (GUILayout.Button("Download", defaultColumnWidthOption))
                        {
                            EditorApplication.delayCall += () =>
                            {
                                SaveAllSkyboxAssets(skybox);
                            };
                        }

                        GUI.enabled = true;
                    }
                    else
                    {
                        var mainImageAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(GetLocalPath(albedoPath));

                        if (mainImageAsset != null)
                        {
                            EditorGUILayout.ObjectField(mainImageAsset, typeof(Texture2D), false, GUILayout.Height(128f), GUILayout.Width(256f));
                        }
                        else
                        {
                            if (!loadingSkyboxes.TryGetValue(skybox.ObfuscatedId, out _))
                            {
                                EditorGUILayout.HelpBox($"Failed to load texture at {albedoPath}!", MessageType.Error);
                            }
                        }

                        if (File.Exists(depthPath))
                        {
                            var depthImageAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(GetLocalPath(depthPath));

                            if (depthImageAsset != null)
                            {
                                EditorGUILayout.ObjectField(depthImageAsset, typeof(Texture2D), false, GUILayout.Height(128f), GUILayout.Width(256f));
                            }
                            else
                            {
                                if (!loadingSkyboxes.TryGetValue(skybox.ObfuscatedId, out _))
                                {
                                    EditorGUILayout.HelpBox($"Failed to load texture at {depthPath}!", MessageType.Error);
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                foreach (var export in skyboxExportOptions)
                {
                    if (export.Key.Contains("jpg") ||
                        export.Key.Contains("depth-map-png") ||
                        export.Key.Contains("equirectangular-png"))
                    {
                        continue;
                    }

                    if (skybox.Exports.TryGetValue(export.Key, out var exportUrl))
                    {
                        Rest.TryGetFileNameFromUrl(exportUrl, out var filename);
                        var exportPath = GetFullLocalPath(skybox, export.Key, Path.GetExtension(filename));

                        if (!File.Exists(exportPath))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(export.Key);

                            if (GUILayout.Button("Download", defaultColumnWidthOption))
                            {
                                EditorApplication.delayCall += async () =>
                                {
                                    try
                                    {
                                        Rest.TryDeleteCacheItem(exportUrl);
                                        var downloadedPath = await Rest.DownloadFileAsync(exportUrl, exportPath);
                                        Debug.Log(downloadedPath);
                                        Debug.Log(exportPath);
                                        File.Copy(downloadedPath, exportPath);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError(e);
                                    }
                                };
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(export.Key, expandWidthOption);
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(GetLocalPath(exportPath));

                            if (asset != null)
                            {
                                switch (export.Key)
                                {
                                    case "equirectangular-png":
                                    case "depth-map-png":
                                    case "equirectangular-jpg":
                                    case "depth-map-jpg":
                                        // already handled.
                                        break;
                                    case "cube-map-png":
                                        EditorGUILayout.ObjectField(asset, typeof(Cubemap), false, GUILayout.Height(128f), GUILayout.Width(128f));
                                        break;
                                    case "hdri-hdr":
                                    case "hdri-exr":
                                        EditorGUILayout.ObjectField(asset, typeof(Texture2D), false, GUILayout.Height(128f), GUILayout.Width(256f));
                                        break;
                                    case "video-landscape-mp4":
                                    case "video-portrait-mp4":
                                    case "video-square-mp4":
                                        EditorGUILayout.ObjectField(asset, typeof(VideoClip), false);
                                        break;
                                    default:
                                        EditorGUILayout.ObjectField(asset, typeof(Object), false);
                                        break;
                                }
                            }
                            else
                            {
                                EditorGUILayout.HelpBox($"Failed to load {export.Key} asset!", MessageType.Error);
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(export.Key);

                        if (GUILayout.Button("Export", defaultColumnWidthOption))
                        {
                            EditorApplication.delayCall += () =>
                            {
                                // TODO export it
                            };
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(EndWidth);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private static string GetFullLocalPath(SkyboxInfo skybox, string assetType, string extension)
            => Path.Combine(editorDownloadDirectory, $"{skybox.ObfuscatedId}-{assetType}{extension}").Replace("\\", "/").Replace(".zip", ".cubemap");

        private static string GetLocalPath(string path)
            => path.Replace(Application.dataPath, "Assets");

        private static async void DeleteSkybox(SkyboxInfo skyboxInfo, string albedoPath, string depthPath)
        {
            if (!EditorUtility.DisplayDialog(
                    "Attention!",
                    $"Are you sure you want to delete skybox {skyboxInfo.Id}?\n\n({skyboxInfo.ObfuscatedId})",
                    "Delete",
                    "Cancel"))
            {
                return;
            }

            try
            {
                var success = await api.SkyboxEndpoint.DeleteSkyboxAsync(skyboxInfo.Id);

                if (!success)
                {
                    Debug.LogError($"Failed to delete skybox {skyboxInfo.Id} ({skyboxInfo.ObfuscatedId})!");
                }
                else
                {
                    if (!string.IsNullOrEmpty(albedoPath) &&
                        File.Exists(albedoPath))
                    {
                        AssetDatabase.DeleteAsset(GetLocalPath(albedoPath));
                    }

                    if (!string.IsNullOrEmpty(depthPath) &&
                        File.Exists(depthPath))
                    {
                        AssetDatabase.DeleteAsset(GetLocalPath(depthPath));
                    }

                    // TODO delete any local exported items from asset database
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                };

                FetchSkyboxHistory();
            }
        }

        private static async void FetchSkyboxHistory()
        {
            if (isFetchingSkyboxHistory) { return; }
            isFetchingSkyboxHistory = true;

            try
            {
                history = await api.SkyboxEndpoint.GetSkyboxHistoryAsync(new SkyboxHistoryParameters { Limit = limit, Offset = page });
                //Debug.Log($"history item count: {history.TotalCount} | hasMore? {history.HasMore}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingSkyboxHistory = false;
            }
        }
    }
}
