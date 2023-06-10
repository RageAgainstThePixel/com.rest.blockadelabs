// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using Utilities.Rest.Editor;

namespace BlockadeLabs.Editor
{
    [CustomEditor(typeof(BlockadeLabsConfiguration))]
    public class BlockadeLabsConfigurationInspector : BaseConfigurationInspector<BlockadeLabsConfiguration>
    {
        private static bool triggerReload;

        private SerializedProperty apiKey;
        private SerializedProperty proxyDomain;

        #region Project Settings Window

        [SettingsProvider]
        private static SettingsProvider Preferences()
            => GetSettingsProvider(nameof(BlockadeLabs), CheckReload);

        #endregion Project Settings Window

        #region Inspector Window

        private void OnEnable()
        {
            GetOrCreateInstance(target);

            try
            {
                apiKey = serializedObject.FindProperty(nameof(apiKey));
                proxyDomain = serializedObject.FindProperty(nameof(proxyDomain));
            }
            catch (Exception)
            {
                // Ignored
            }
        }

        private void OnDisable() => CheckReload();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(apiKey);
            EditorGUILayout.PropertyField(proxyDomain);

            if (EditorGUI.EndChangeCheck())
            {
                triggerReload = true;
            }

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion Inspector Window

        private static void CheckReload()
        {
            if (triggerReload)
            {
                triggerReload = false;
                EditorUtility.RequestScriptReload();
            }
        }
    }
}
