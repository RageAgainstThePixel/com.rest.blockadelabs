using BlockadeLabs.Skyboxes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Utilities.Async;
using Utilities.WebRequestRest;
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

        private static bool hasFetchedSkyboxStyles;

        private static IReadOnlyList<SkyboxStyle> skyboxStyles = new List<SkyboxStyle>();

        private static GUIContent[] skyboxOptions = Array.Empty<GUIContent>();

        private static SkyboxStyle currentSkyboxStyleSelection;

        private static bool hasFetchedHistory;

        private static SkyboxHistory history;

        #endregion static content

        [SerializeField]
        private int tab;

        [SerializeField]
        private int currentSkyboxStyleId;

        private Vector2 scrollPosition = Vector2.zero;

        private string promptText;

        private string negativeText;

        private int seed;

        private bool depth;

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
            seed = EditorGUILayout.IntField("Seed", seed);
            depth = EditorGUILayout.Toggle("Depth", depth);
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

        private static bool isFetchingSkyboxStyles;

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

        private static bool isGeneratingSkybox;

        private async void GenerateSkybox()
        {
            if (isGeneratingSkybox) { return; }
            isGeneratingSkybox = true;

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
                        skyboxStyleId: currentSkyboxStyleSelection?.Id,
                        seed: seed,
                    depth: depth)
                    : new SkyboxRequest(
                        prompt: promptText,
                        negativeText: negativeText,
                        controlImage: controlImage,
                        skyboxStyleId: currentSkyboxStyleSelection?.Id,
                        seed: seed,
                        depth: depth);
                promptText = string.Empty;
                negativeText = string.Empty;
                controlImage = null;
                var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(skyboxRequest);
                await SaveSkyboxAssetAsync(skyboxInfo);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isGeneratingSkybox = false;
            }
        }

        private static ConcurrentDictionary<int, SkyboxInfo> loadingSkyboxes = new ConcurrentDictionary<int, SkyboxInfo>();

        private static async void SaveSkyboxAsset(SkyboxInfo skyboxInfo)
        {
            loadingSkyboxes.TryAdd(skyboxInfo.Id, skyboxInfo);
            await skyboxInfo.LoadTexturesAsync();
            await SaveSkyboxAssetAsync(skyboxInfo);
            loadingSkyboxes.TryRemove(skyboxInfo.Id, out _);
        }

        private static async Task SaveSkyboxAssetAsync(SkyboxInfo skyboxInfo)
        {
            if (skyboxInfo.MainTexture == null)
            {
                Debug.LogError("No main texture found!");
                return;
            }

            await Awaiters.UnityMainThread;
            var directory = editorDownloadDirectory.Replace(Application.dataPath, "Assets");

            var mainTexturePath = string.Empty;
            var depthTexturePath = string.Empty;
            var mainTextureBytes = skyboxInfo.MainTexture != null ? skyboxInfo.MainTexture.EncodeToPNG() : Array.Empty<byte>();

            if (mainTextureBytes.Length > 0)
            {
                mainTexturePath = $"{directory}/{skyboxInfo.MainTexture!.name}.png";
                Debug.Log(mainTexturePath);
            }

            var depthTextureBytes = skyboxInfo.DepthTexture != null ? skyboxInfo.DepthTexture.EncodeToPNG() : Array.Empty<byte>();

            if (depthTextureBytes.Length > 0)
            {
                depthTexturePath = $"{directory}/{skyboxInfo.DepthTexture!.name}.depth.png";
                Debug.Log(depthTexturePath);
            }

            var importTasks = new List<Task>(2)
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

            await Task.WhenAll(importTasks);
            await Awaiters.UnityMainThread;

            if (AssetImporter.GetAtPath(mainTexturePath) is TextureImporter mainTextureImporter)
            {
                mainTextureImporter.alphaIsTransparency = false;
                mainTextureImporter.alphaSource = TextureImporterAlphaSource.None;
            }

            if (AssetImporter.GetAtPath(depthTexturePath) is TextureImporter depthTextureImporter)
            {
                depthTextureImporter.alphaIsTransparency = false;
                depthTextureImporter.alphaSource = TextureImporterAlphaSource.None;
            }

            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                var mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(mainTexturePath);
                EditorGUIUtility.PingObject(mainTexture);
            };
        }

        private static async Task SaveTextureAsync(string path, byte[] pngBytes)
        {
            var fileStream = File.OpenWrite(path);

            try
            {
                await fileStream.WriteAsync(pngBytes, 0, pngBytes.Length);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write texture to disk!\n{e}");
            }
            finally
            {
                await fileStream.DisposeAsync();
            }
        }

        private void RenderHistory()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TabWidth);
            { //Header
                EditorGUILayout.LabelField("History", EditorStyles.boldLabel, wideColumnWidthOption);

                //if (history != null && GUILayout.Button("Prev Page"))
                //{
                //    page--;
                //}

                //if (history is { HasMore: true } && GUILayout.Button("Next Page"))
                //{
                //    page++;
                //}

                limit = EditorGUILayout.IntField("page items", limit);

                GUILayout.FlexibleSpace();

                GUI.enabled = !isFetchingSkyboxHistory;

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
                if (Rest.TryGetFileNameFromUrl(skybox.MainTextureUrl, out var mainTextureFilePath))
                {
                    mainTextureFilePath = Path.Combine(editorDownloadDirectory, mainTextureFilePath).Replace("\\", "/").Replace(".jpg", ".png");
                }

                if (Rest.TryGetFileNameFromUrl(skybox.DepthTextureUrl, out var depthTextureFilePath))
                {
                    depthTextureFilePath = Path.Combine(editorDownloadDirectory, $"{depthTextureFilePath}").Replace("\\", "/").Replace(".jpg", ".depth.png");
                }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"{skybox.Title} {skybox.Status} {skybox.CreatedAt}");

                if (!File.Exists(mainTextureFilePath))
                {
                    GUI.enabled = !loadingSkyboxes.TryGetValue(skybox.Id, out _);

                    if (GUILayout.Button("Download", defaultColumnWidthOption))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            SaveSkyboxAsset(skybox);
                        };
                    }

                    GUI.enabled = true;
                }
                else
                {
                    var mainLocalPath = mainTextureFilePath.Replace(Application.dataPath, "Assets");
                    var mainImageAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(mainLocalPath);

                    if (mainImageAsset != null)
                    {
                        EditorGUILayout.ObjectField(mainImageAsset, typeof(Texture2D), false);
                    }

                    if (File.Exists(depthTextureFilePath))
                    {
                        var depthLocalPath = depthTextureFilePath.Replace(Application.dataPath, "Assets");
                        var depthImageAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(depthLocalPath);

                        if (depthImageAsset != null)
                        {
                            EditorGUILayout.ObjectField(depthImageAsset, typeof(Texture2D), false);
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(EndWidth);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private static bool isFetchingSkyboxHistory;

        // private static int page;

        private static int limit = 9999;

        private static async void FetchSkyboxHistory()
        {
            if (isFetchingSkyboxHistory) { return; }
            isFetchingSkyboxHistory = true;

            try
            {
                history = await api.SkyboxEndpoint.GetSkyboxHistoryAsync(new SkyboxHistoryParameters { Limit = limit });
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
