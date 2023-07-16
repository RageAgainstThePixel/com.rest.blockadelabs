// Licensed under the MIT License. See LICENSE in the project root for license information.

using BlockadeLabs.Skyboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabs.Editor
{
    [CustomPropertyDrawer(typeof(SkyboxStyle))]
    public class SkyboxStylePropertyDrawer : PropertyDrawer
    {
        private static BlockadeLabsClient blockadeLabsClient;

        private static BlockadeLabsClient BlockadeLabsClient => blockadeLabsClient ??= new BlockadeLabsClient();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                if (!BlockadeLabsClient.HasValidAuthentication)
                {
                    EditorGUI.LabelField(position, "Cannot fetch skybox styles");
                    return;
                }
            }
            catch (AuthenticationException)
            {
                EditorGUI.HelpBox(position, "Check blockade labs api key", MessageType.Error);

                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                return;
            }

            var id = property.FindPropertyRelative("id");
            var name = property.FindPropertyRelative("name");

            if (options.Length < 1)
            {
                FetchStyles();

                if (string.IsNullOrWhiteSpace(name.stringValue))
                {
                    EditorGUI.HelpBox(position, "Fetching skybox styles...", MessageType.Info);
                    return;
                }

                EditorGUI.LabelField(position, label, new GUIContent(name.stringValue, id.intValue.ToString()));
                return;
            }

            // dropdown
            var index = -1;
            dynamic currentVoiceOption = null;

            if (id.intValue > 0)
            {
                currentVoiceOption = styles?.FirstOrDefault(style => style.Id.ToString() == id.intValue.ToString());
            }

            if (currentVoiceOption != null)
            {
                for (var i = 0; i < options.Length; i++)
                {
                    if (options[i].tooltip.Contains(currentVoiceOption.Id.ToString()))
                    {
                        index = i;
                        break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, label, index, options);

            if (EditorGUI.EndChangeCheck())
            {
                currentVoiceOption = styles?.FirstOrDefault(style => options[index].text.Contains(style.Name));
                id.intValue = currentVoiceOption!.Id;
                name.stringValue = currentVoiceOption!.Name;
            }
        }

        private static bool isFetchingStyles;

        public static bool IsFetchingStyles => isFetchingStyles;

        private static IReadOnlyList<SkyboxStyle> styles = new List<SkyboxStyle>();

        public static IReadOnlyList<SkyboxStyle> Styles => styles;

        private static GUIContent[] options = Array.Empty<GUIContent>();

        public static IReadOnlyList<GUIContent> Options => options;

        public static async void FetchStyles()
        {
            if (isFetchingStyles) { return; }
            isFetchingStyles = true;

            try
            {
                styles = await BlockadeLabsClient.SkyboxEndpoint.GetSkyboxStylesAsync();
                options = styles.OrderBy(style => style.Id).Select(style => new GUIContent(style.Name, style.Id.ToString())).ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingStyles = false;
            }
        }
    }
}
