using BlockadeLabs.Skyboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Extensions;

namespace BlockadeLabs.Demo
{
    public class SkyboxDemo : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField promptInputField;

        [SerializeField]
        private TMP_Dropdown skyboxStyleDropdown;

        [SerializeField]
        private Button generateButton;

        [SerializeField]
        private Material skyboxMaterial;

        private BlockadeLabsClient api;

        private CancellationTokenSource lifetimeCancellationTokenSource;

        private IReadOnlyList<SkyboxStyle> skyboxOptions;

        private void OnValidate()
        {
            promptInputField.Validate();
            skyboxStyleDropdown.Validate();
            generateButton.Validate();
        }

        private void Awake()
        {
            OnValidate();
            lifetimeCancellationTokenSource = new CancellationTokenSource();

            try
            {
                api = new BlockadeLabsClient();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(BlockadeLabsClient)}!\n{e}");
                enabled = false;
                return;
            }

            GetSkyboxStyles();
            generateButton.onClick.AddListener(GenerateSkybox);
            promptInputField.onSubmit.AddListener(GenerateSkybox);
            promptInputField.onValueChanged.AddListener(ValidateInput);
        }

        private void ValidateInput(string input)
        {
            generateButton.interactable = !string.IsNullOrWhiteSpace(input);
        }

        private void GenerateSkybox() => GenerateSkybox(promptInputField.text);

        private async void GenerateSkybox(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                Debug.LogWarning("No prompt given!");
                return;
            }

            try
            {
                var request = new SkyboxRequest(prompt, skyboxStyleId: skyboxOptions[skyboxStyleDropdown.value].Id);
                var (texture, skyboxInfo) = await api.SkyboxEndpoint.GenerateSkyboxAsync(request, lifetimeCancellationTokenSource.Token);
                skyboxMaterial.mainTexture = texture;
                Debug.Log($"Successfully created skybox: {skyboxInfo.Id}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnDestroy()
        {
            lifetimeCancellationTokenSource.Cancel();
        }

        private async void GetSkyboxStyles()
        {
            try
            {
                skyboxOptions = await api.SkyboxEndpoint.GetSkyboxStylesAsync(lifetimeCancellationTokenSource.Token).ConfigureAwait(true);
                var dropdownOptions = new List<TMP_Dropdown.OptionData>(skyboxOptions.Count);
                dropdownOptions.AddRange(skyboxOptions.Select(skyboxStyle => new TMP_Dropdown.OptionData(skyboxStyle.Name)));
                skyboxStyleDropdown.options = dropdownOptions;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
