// Licensed under the MIT License. See LICENSE in the project root for license information.

using BlockadeLabs.Skyboxes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using Utilities.WebRequestRest;
using Object = UnityEngine.Object;
using Progress = UnityEditor.Progress;

namespace BlockadeLabs.Editor
{
    public sealed class BlockadeLabsDashboard : EditorWindow
    {
        private const int TabWidth = 18;
        private const int EndWidth = 10;
        private const int InnerLabelIndentLevel = 13;
        private const int MaxCharacterLength = 5000;

        private const float InnerLabelWidth = 1.9f;
        private const float DefaultColumnWidth = 96f;
        private const float WideColumnWidth = 128f;
        private const float SettingsLabelWidth = 1.56f;

        private static readonly GUIContent guiTitleContent = new($"{nameof(BlockadeLabs)} Dashboard");
        private static readonly GUIContent saveDirectoryContent = new("Save Directory");
        private static readonly GUIContent downloadContent = new("Download");
        private static readonly GUIContent deleteContent = new("Delete");
        private static readonly GUIContent refreshContent = new("Refresh");

        private static readonly string[] tabTitles = { "Skybox Generation", "History" };

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
                        boldCenteredHeaderStyle = new(editorStyle)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18,
                            padding = new(0, 0, -8, -8)
                        };
                    }
                }

                return boldCenteredHeaderStyle;
            }
        }

        private static string DefaultSaveDirectoryKey => $"{Application.productName}_{nameof(BlockadeLabs)}_EditorDownloadDirectory";

        private static string DefaultSaveDirectory => Application.dataPath;

        #region static content

        private static BlockadeLabsClient api;

        private static string editorDownloadDirectory = string.Empty;

        private static bool isGeneratingSkybox;

        private static SkyboxModel model = SkyboxModel.Model3;

        private static IReadOnlyList<SkyboxStyle> skyboxStyles = new List<SkyboxStyle>();

        private static GUIContent[] skyboxOptions = Array.Empty<GUIContent>();

        private static SkyboxStyle currentSkyboxStyleSelection;

        private static GUIContent[] exportOptions = Array.Empty<GUIContent>();

        private static SkyboxExportOption currentSkyboxExportOption;

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
        private BlockadeLabsConfiguration configuration;

        [SerializeField]
        private int tab;

        [SerializeField]
        private int currentSkyboxStyleId;

        [SerializeField]
        private int currentSkyboxExportId = 1;

        private BlockadeLabsAuthentication auth;

        private BlockadeLabsSettings settings;

        private Vector2 scrollPosition = Vector2.zero;

        private string promptText;

        private string negativeText;

        private int seed;

        private bool enhancePrompt;

        private Texture2D controlImage;

        [MenuItem("Window/Dashboards/" + nameof(BlockadeLabs), false, priority: 999)]
        private static void OpenWindow()
        {
            // Dock it next to the Scene View.
            var instance = GetWindow<BlockadeLabsDashboard>(typeof(SceneView));
            instance.Show();
            instance.titleContent = guiTitleContent;
        }

        private void OnEnable()
        {
            titleContent = guiTitleContent;
            minSize = new(WideColumnWidth * 4.375F, WideColumnWidth * 4);
        }

        private void OnFocus()
        {
            if (configuration == null)
            {
                configuration = Resources.Load<BlockadeLabsConfiguration>($"{nameof(BlockadeLabsConfiguration)}.asset");
            }

            auth ??= configuration == null
                ? new BlockadeLabsAuthentication().LoadDefaultsReversed()
                : new(configuration);
            settings ??= configuration == null
                ? new()
                : new BlockadeLabsSettings(configuration);

            api ??= new(auth, settings);

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
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(TabWidth);
            EditorGUILayout.BeginVertical();
            { // Begin Header
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(guiTitleContent, BoldCenteredHeaderStyle);
                EditorGUILayout.Space();

                if (api is not { HasValidAuthentication: true })
                {
                    EditorGUILayout.HelpBox($"No valid {nameof(BlockadeLabsConfiguration)} was found. This tool requires that you set your API key.", MessageType.Error);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
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
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, expandWidthOption);

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
            controlImage = EditorGUILayout.ObjectField(new GUIContent("Remix Image"), controlImage, typeof(Texture2D), false) as Texture2D;
            { // model popup
                EditorGUI.BeginChangeCheck();
                model = (SkyboxModel)EditorGUILayout.EnumPopup(new GUIContent("Model"), model);

                if (EditorGUI.EndChangeCheck())
                {
                    FetchSkyboxStyles();
                }
            }
            { // skybox style dropdown
                var skyboxIndex = -1;
                currentSkyboxStyleSelection ??= skyboxStyles?.FirstOrDefault(skyboxStyle => skyboxStyle.Id == currentSkyboxStyleId);

                if (currentSkyboxStyleSelection != null)
                {
                    for (var i = 0; i < skyboxOptions.Length; i++)
                    {
                        if (skyboxOptions[i].text.Contains(currentSkyboxStyleSelection.Name.Replace("/", " ")))
                        {
                            skyboxIndex = i;
                            break;
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                skyboxIndex = EditorGUILayout.Popup(new("Style"), skyboxIndex, skyboxOptions);

                if (EditorGUI.EndChangeCheck() && skyboxStyles is { Count: > 0 })
                {
                    SkyboxStyle selection = null;

                    foreach (var skyboxStyle in skyboxStyles)
                    {
                        if (!skyboxOptions[skyboxIndex].text.Contains(skyboxStyle.Name.Replace("/", " ")))
                        {
                            continue;
                        }

                        if (skyboxStyle.FamilyStyles != null)
                        {
                            foreach (var familyStyle in skyboxStyle.FamilyStyles)
                            {
                                if (skyboxOptions[skyboxIndex].text.Contains(familyStyle.Name.Replace("/", " ")))
                                {
                                    selection = familyStyle;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            selection = skyboxStyle;
                            break;
                        }
                    }

                    currentSkyboxStyleSelection = selection;
                    currentSkyboxStyleId = currentSkyboxStyleSelection!.Id;
                }
            }
            { // export option dropdown
                var exportIndex = -1;
                currentSkyboxExportOption ??= skyboxExportOptions?.FirstOrDefault(exportOption => exportOption.Id == currentSkyboxExportId);

                if (currentSkyboxExportOption != null)
                {
                    for (int i = 0; i < exportOptions.Length; i++)
                    {
                        if (exportOptions[i].text.Contains(currentSkyboxExportOption.Name))
                        {
                            exportIndex = i;
                            break;
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                exportIndex = EditorGUILayout.Popup(new("Export"), exportIndex, exportOptions);

                if (EditorGUI.EndChangeCheck())
                {
                    currentSkyboxExportOption = skyboxExportOptions?.FirstOrDefault(option => exportOptions[exportIndex].text.Contains(option.Name));
                    currentSkyboxExportId = currentSkyboxExportOption!.Id;
                }
            }
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
                var skyboxFamilies = await api.SkyboxEndpoint.GetSkyboxStyleFamiliesAsync(model);
                var tempOptions = new List<GUIContent>();
                var tempStyles = new List<SkyboxStyle>();

                foreach (var skyboxStyle in skyboxFamilies)
                {
                    if (skyboxStyle.FamilyStyles != null)
                    {
                        tempOptions.AddRange(skyboxStyle.FamilyStyles.Where(style => style.Model == model).Select(style =>
                        {
                            tempStyles.Add(skyboxStyle);
                            return new GUIContent($"{skyboxStyle.Name.Replace("/", " ")}/{style.Name.Replace("/", " ")}");
                        }));
                    }
                    else if (skyboxStyle.Model == model)
                    {
                        tempStyles.Add(skyboxStyle);
                        tempOptions.Add(new GUIContent(skyboxStyle.Name.Replace("/", " ")));
                    }
                }

                skyboxStyles = tempStyles;
                skyboxOptions = tempOptions.ToArray();
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
                skyboxExportOptions = (await api.SkyboxEndpoint.GetAllSkyboxExportOptionsAsync()).OrderBy(option => option.Key).ToList();
                exportOptions = skyboxExportOptions.Select(option => new GUIContent(option.Name)).ToArray();
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
                        throw new($"Enable Read/Write in Texture asset import settings for {controlImage.name}");
                    }

                    if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        throw new($"Disable compression in Texture asset import settings for {controlImage.name}");
                    }
                }

                using var skyboxRequest = controlImage == null
                    ? new(
                        style: currentSkyboxStyleSelection,
                        prompt: promptText,
                        negativeText: negativeText,
                        enhancePrompt: enhancePrompt,
                        seed: seed)
                    : new SkyboxRequest(
                        style: currentSkyboxStyleSelection,
                        prompt: promptText,
                        negativeText: negativeText,
                        enhancePrompt: enhancePrompt,
                        controlImage: controlImage,
                        seed: seed);
                promptText = string.Empty;
                negativeText = string.Empty;
                controlImage = null;
                progressId = Progress.Start("Generating Skybox", promptText, options: Progress.Options.Indefinite);
                var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(skyboxRequest, currentSkyboxExportOption);
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
                EditorApplication.delayCall += FetchSkyboxHistory;
            }
        }

        private static readonly ConcurrentDictionary<string, SkyboxInfo> loadingSkyboxes = new();

        private static async void SaveAllSkyboxAssets(SkyboxInfo skyboxInfo)
        {
            if (loadingSkyboxes.TryAdd(skyboxInfo.ObfuscatedId, skyboxInfo))
            {
                try
                {
                    await skyboxInfo.LoadAssetsAsync();
                    await SaveSkyboxAssetAsync(skyboxInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    loadingSkyboxes.TryRemove(skyboxInfo.ObfuscatedId, out _);
                }
            }
            else
            {
                Debug.LogWarning($"Skybox {skyboxInfo.ObfuscatedId} is already loading assets. Try again later.");
            }
        }

        private static async Task SaveSkyboxAssetAsync(SkyboxInfo skyboxInfo)
        {
            var importTasks = new List<Task>();

            foreach (var export in skyboxInfo.Exports)
            {
                if (skyboxInfo.TryGetAssetCachePath(export.Key, out var cachedPath))
                {
                    importTasks.Add(CopyFileAsync(editorDownloadDirectory, cachedPath, skyboxInfo, export.Key));
                }
            }

            AssetDatabase.DisallowAutoRefresh();
            try
            {
                await Task.WhenAll(importTasks).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                AssetDatabase.AllowAutoRefresh();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static async Task CopyFileAsync(string directory, string cachedPath, SkyboxInfo skybox, string exportKey)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            cachedPath = cachedPath.Replace("file://", string.Empty);
            var shallowPath = cachedPath
                .Replace(Rest.DownloadCacheDirectory, string.Empty)
                .Replace("/", "\\");
            var importPath = $"{directory.Replace("/", "\\")}{shallowPath}";

            if (File.Exists(importPath)) { return; }
            File.Copy(cachedPath, importPath);
            importPath = GetLocalPath(importPath);
            AssetDatabase.ImportAsset(importPath);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(importPath);

            if (asset == null)
            {
                Debug.LogError($"Failed to import asset at {importPath}");
                return;
            }

            Debug.Log($"{asset.GetType().Name}::{asset.name}");

            switch (asset)
            {
                case DefaultAsset:
                    if (importPath.EndsWith(".zip"))
                    {
                        var files = await ExportUtilities.UnZipAsync(importPath);
                        var textures = new List<Texture2D>();

                        foreach (var file in files)
                        {
                            AssetDatabase.ImportAsset(file);

                            if (AssetImporter.GetAtPath(file) is TextureImporter textureImporter)
                            {
                                textureImporter.isReadable = true;
                                textureImporter.mipmapEnabled = false;
                                textureImporter.alphaIsTransparency = false;
                                textureImporter.wrapMode = TextureWrapMode.Clamp;
                                textureImporter.alphaSource = TextureImporterAlphaSource.None;
                                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                                textureImporter.SaveAndReimport();
                            }

                            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                            textures.Add(texture);
                        }

#if UNITY_2022_1_OR_NEWER
                        var cubemap = new Cubemap(textures.First().width, TextureFormat.RGB24, false, true);
#else
                        var cubemap = new Cubemap(textures.First().width, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8_UInt, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
#endif

                        try
                        {
                            foreach (var texture in textures)
                            {
                                switch (texture.name)
                                {
                                    case "cube_up":
                                        var rotation = exportKey.Contains("roblox")
                                            ? ExportUtilities.TextureRotation.Counterclockwise90
                                            : ExportUtilities.TextureRotation.Rotate180;
                                        cubemap.SetCubemapTexture(texture.Rotate(rotation), CubemapFace.PositiveY);
                                        break;
                                    case "cube_down":
                                        rotation = exportKey.Contains("roblox")
                                            ? ExportUtilities.TextureRotation.Clockwise90
                                            : ExportUtilities.TextureRotation.Rotate180;
                                        cubemap.SetCubemapTexture(texture.Rotate(rotation), CubemapFace.NegativeY);
                                        break;
                                    case "cube_front":
                                        cubemap.SetCubemapTexture(texture, CubemapFace.PositiveZ);
                                        break;
                                    case "cube_back":
                                        cubemap.SetCubemapTexture(texture, CubemapFace.NegativeZ);
                                        break;
                                    case "cube_right":
                                        cubemap.SetCubemapTexture(texture, CubemapFace.PositiveX);
                                        break;
                                    case "cube_left":
                                        cubemap.SetCubemapTexture(texture, CubemapFace.NegativeX);
                                        break;
                                }
                            }

                            var assetPath = Path.ChangeExtension(importPath, "cubemap");
                            AssetDatabase.CreateAsset(cubemap, assetPath);
                            AssetDatabase.DeleteAsset(importPath);
                            skybox.exportedAssets[exportKey] = cubemap;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to import cubemap at {importPath}!\n{e}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Unhandled asset at {importPath}");
                    }
                    break;
                case Texture2D texture:
                    SetSkyboxTextureImportSettings(importPath);
                    skybox.exportedAssets[exportKey] = texture;
                    break;
                case VideoClip videoClip:
                    skybox.exportedAssets[exportKey] = videoClip;
                    break;
            }
        }

        private static void SetSkyboxTextureImportSettings(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter textureImporter) { return; }

            textureImporter.isReadable = true;
            textureImporter.mipmapEnabled = false;
            textureImporter.alphaIsTransparency = false;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.alphaSource = TextureImporterAlphaSource.None;

            if (path.Contains("depth-map"))
            {
                textureImporter.wrapModeU = TextureWrapMode.Repeat;
                textureImporter.wrapModeV = TextureWrapMode.Clamp;
            }
            else
            {
                textureImporter.npotScale = TextureImporterNPOTScale.None;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.maxTextureSize = 6144;
            }

            textureImporter.SaveAndReimport();
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
                if (skybox == null) { continue; }

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                { // skybox title
                    EditorGUILayout.LabelField($"{skybox.Title} {skybox.Status} {skybox.CreatedAt}");

                    if (GUILayout.Button(deleteContent, defaultColumnWidthOption))
                    {
                        EditorApplication.delayCall += () => DeleteSkybox(skybox);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                { // Download and export all options
                    GUI.enabled = !loadingSkyboxes.TryGetValue(skybox.ObfuscatedId, out _);

                    var hasAllDownloads = skybox.Exports.All(export => Rest.TryGetFileNameFromUrl(export.Value, out var filename) && File.Exists(GetFullLocalPath(skybox, export.Key, Path.GetExtension(filename))));

                    if (!hasAllDownloads &&
                        GUILayout.Button("Download All", defaultColumnWidthOption))
                    {
                        EditorApplication.delayCall += () => SaveAllSkyboxAssets(skybox);
                    }

                    if (!skyboxExportOptions.All(option => skybox.Exports.ContainsKey(option.Key)) &&
                        GUILayout.Button("Export All", defaultColumnWidthOption))
                    {
                        EditorApplication.delayCall += () => Export(skybox, skyboxExportOptions.Where(exportOption => !skybox.Exports.ContainsKey(exportOption.Key)).ToArray());
                    }

                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();

                foreach (var export in skyboxExportOptions)
                {
                    if (skybox.Exports.TryGetValue(export.Key, out var exportUrl))
                    {
                        Rest.TryGetFileNameFromUrl(exportUrl, out var filename);
                        var assetPath = GetFullLocalPath(skybox, export.Key, Path.GetExtension(filename));

                        if (File.Exists(assetPath))
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(export.Key, expandWidthOption);
                            var asset = AssetDatabase.LoadAssetAtPath<Object>(GetLocalPath(assetPath));

                            if (asset != null)
                            {
                                switch (export.Key)
                                {
                                    case SkyboxExportOption.CubeMap_PNG:
                                    case SkyboxExportOption.CubeMap_Roblox_PNG:
                                        EditorGUILayout.ObjectField(asset, typeof(Cubemap), false);
                                        break;
                                    case SkyboxExportOption.Equirectangular_JPG:
                                    case SkyboxExportOption.Equirectangular_PNG:
                                    case SkyboxExportOption.DepthMap_PNG:
                                    case SkyboxExportOption.HDRI_HDR:
                                    case SkyboxExportOption.HDRI_EXR:
                                        EditorGUILayout.ObjectField(asset, typeof(Texture2D), false);
                                        break;
                                    case SkyboxExportOption.Video_LandScape_MP4:
                                    case SkyboxExportOption.Video_Portrait_MP4:
                                    case SkyboxExportOption.Video_Square_MP4:
                                        EditorGUILayout.ObjectField(asset, typeof(VideoClip), false);
                                        break;
                                    default:
                                        Debug.LogWarning($"Unhandled export key: {export.Key}");
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
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(export.Key);

                            if (GUILayout.Button(downloadContent, defaultColumnWidthOption))
                            {
                                EditorApplication.delayCall += async () =>
                                {
                                    try
                                    {
                                        if (skybox.TryGetAssetCachePath(export.Key, out var cachedPath))
                                        {
                                            await CopyFileAsync(editorDownloadDirectory, cachedPath, skybox, export.Key);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogError(e);
                                    }
                                    finally
                                    {
                                        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                                    }
                                };
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
                                Debug.Log($"Exporting: {skybox.Id} -> {export.Name}");
                                Export(skybox, export);
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

        private static async void Export(SkyboxInfo skybox, params SkyboxExportOption[] options)
        {
            var exportTasks = new List<Task>();

            foreach (var exportOption in options)
            {
                exportTasks.Add(api.SkyboxEndpoint.ExportSkyboxAsync(skybox, exportOption));
            }

            try
            {
                await Task.WhenAll(exportTasks).ConfigureAwait(true);
                skybox = await api.SkyboxEndpoint.GetSkyboxInfoAsync(skybox.Id);
                SaveAllSkyboxAssets(skybox);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorApplication.delayCall += FetchSkyboxHistory;
            }
        }

        private static string GetFullLocalPath(SkyboxInfo skybox, string assetType, string extension)
            => Path.Combine(editorDownloadDirectory, $"{skybox.ObfuscatedId}-{assetType}{extension}").Replace("\\", "/").Replace(".zip", ".cubemap");

        private static string GetLocalPath(string path)
            => path.Replace("\\", "/").Replace(Application.dataPath, "Assets");

        private static async void DeleteSkybox(SkyboxInfo skyboxInfo)
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
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                EditorApplication.delayCall += FetchSkyboxHistory;
            }
        }

        private static async void FetchSkyboxHistory()
        {
            if (isFetchingSkyboxHistory) { return; }
            isFetchingSkyboxHistory = true;

            try
            {
                history = await api.SkyboxEndpoint.GetSkyboxHistoryAsync(new() { Limit = limit, Offset = page });
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
