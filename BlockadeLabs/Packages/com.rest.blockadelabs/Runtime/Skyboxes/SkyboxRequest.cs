// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using UnityEngine;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxRequest : IDisposable
    {
        /// <summary>
        /// Creates a new Skybox Request.
        /// </summary>
        /// <param name="prompt">
        /// Text prompt describing the skybox world you wish to create.
        /// Maximum number of characters: 550.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the max-char response parameter defined for each style.
        /// </param>
        /// <param name="negativeText">
        /// Describe things to avoid in the skybox world you wish to create.
        /// Maximum number of characters: 200.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the negative-text-max-char response parameter defined for each style.
        /// </param>
        /// <param name="enhancePrompt">
        /// Have an AI automatically improve your prompt to generate pro-level results every time (default: false)
        /// </param>
        /// <param name="seed">
        /// Send 0 for a random seed generation.
        /// Any other number (1-2147483647) set will be used to "freeze" the image generator generator and
        /// create similar images when run again with the same seed and settings.
        /// </param>
        /// <param name="skyboxStyleId">
        /// Id of predefined style that influences the overall aesthetic of your skybox generation.
        /// </param>
        /// <param name="remixImagineId">
        /// ID of a previously generated skybox.
        /// </param>
        public SkyboxRequest(
            string prompt,
            string negativeText = null,
            bool? enhancePrompt = null,
            int? seed = null,
            int? skyboxStyleId = null,
            int? remixImagineId = null)
        {
            Prompt = prompt;
            NegativeText = negativeText;
            EnhancePrompt = enhancePrompt;
            Seed = seed;
            SkyboxStyleId = skyboxStyleId;
            RemixImagineId = remixImagineId;
        }

        [Obsolete]
        public SkyboxRequest(
            string prompt,
            string negativeText,
            int? seed,
            int? skyboxStyleId,
            int? remixImagineId,
            bool depth)
        {
            Prompt = prompt;
            NegativeText = negativeText;
            Seed = seed;
            SkyboxStyleId = skyboxStyleId;
            RemixImagineId = remixImagineId;
            Depth = depth;
        }

        /// <summary>
        /// Creates a new Skybox Request.
        /// </summary>
        /// <param name="prompt">
        /// Text prompt describing the skybox world you wish to create.
        /// Maximum number of characters: 550.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the max-char response parameter defined for each style.
        /// </param>
        /// <param name="controlImagePath">
        /// File path to the control image for the request.
        /// </param>
        /// <param name="controlModel">
        /// Model used for the <see cref="ControlImage"/>.
        /// Currently the only option is: "scribble".
        /// </param>
        /// <param name="negativeText">
        /// Describe things to avoid in the skybox world you wish to create.
        /// Maximum number of characters: 200.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the negative-text-max-char response parameter defined for each style.
        /// </param>
        /// <param name="enhancePrompt">
        /// Have an AI automatically improve your prompt to generate pro-level results every time (default: false)
        /// </param>
        /// <param name="seed">
        /// Send 0 for a random seed generation.
        /// Any other number (1-2147483647) set will be used to "freeze" the image generator generator and
        /// create similar images when run again with the same seed and settings.
        /// </param>
        /// <param name="skyboxStyleId">
        /// Id of predefined style that influences the overall aesthetic of your skybox generation.
        /// </param>
        /// <param name="remixImagineId">
        /// ID of a previously generated skybox.
        /// </param>
        public SkyboxRequest(
            string prompt,
            string controlImagePath,
            string controlModel = null,
            string negativeText = null,
            bool? enhancePrompt = null,
            int? seed = null,
            int? skyboxStyleId = null,
            int? remixImagineId = null)
            : this(
                prompt,
                File.OpenRead(controlImagePath),
                Path.GetFileName(controlImagePath),
                controlModel,
                negativeText,
                enhancePrompt,
                seed,
                skyboxStyleId,
                remixImagineId)
        {
        }

        [Obsolete]
        public SkyboxRequest(
            string prompt,
            string controlImagePath,
            string controlModel = null,
            string negativeText = null,
            int? seed = null,
            int? skyboxStyleId = null,
            int? remixImagineId = null,
            bool depth = false)
            : this(
                prompt,
                File.OpenRead(controlImagePath),
                Path.GetFileName(controlImagePath),
                controlModel,
                negativeText,
                seed,
                skyboxStyleId,
                remixImagineId,
                depth)
        {
        }

        /// <summary>
        /// Creates a new Skybox Request.
        /// </summary>
        /// <param name="prompt">
        /// Text prompt describing the skybox world you wish to create.
        /// Maximum number of characters: 550.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the max-char response parameter defined for each style.
        /// </param>
        /// <param name="controlImage">
        /// <see cref="Texture2D"/> Control image used to influence the generation.
        /// The image needs to be exactly 1024 pixels wide and 512 pixels tall PNG equirectangular projection image
        /// of a scribble with black background and white brush strokes.
        /// </param>
        /// <param name="controlModel">
        /// Model used for the <see cref="ControlImage"/>.
        /// Currently the only option is: "scribble".
        /// </param>
        /// <param name="negativeText">
        /// Describe things to avoid in the skybox world you wish to create.
        /// Maximum number of characters: 200.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the negative-text-max-char response parameter defined for each style.
        /// </param>
        /// <param name="enhancePrompt">
        /// Have an AI automatically improve your prompt to generate pro-level results every time (default: false)
        /// </param>
        /// <param name="seed">
        /// Send 0 for a random seed generation.
        /// Any other number (1-2147483647) set will be used to "freeze" the image generator generator and
        /// create similar images when run again with the same seed and settings.
        /// </param>
        /// <param name="skyboxStyleId">
        /// Id of predefined style that influences the overall aesthetic of your skybox generation.
        /// </param>
        /// <param name="remixImagineId">
        /// ID of a previously generated skybox.
        /// </param>
        public SkyboxRequest(
            string prompt,
            Texture2D controlImage,
            string controlModel = null,
            string negativeText = null,
            bool? enhancePrompt = null,
            int? seed = null,
            int? skyboxStyleId = null,
            int? remixImagineId = null)
            : this(
                prompt,
                new MemoryStream(controlImage.EncodeToPNG()),
                !string.IsNullOrWhiteSpace(controlImage.name) ? $"{controlImage.name}.png" : null,
                controlModel,
                negativeText,
                enhancePrompt,
                seed,
                skyboxStyleId,
                remixImagineId)
        {
            if (controlImage.height != 512 || controlImage.width != 1024)
            {
                throw new ArgumentException($"{nameof(ControlImage)} dimensions should be 512x1024");
            }
        }

        [Obsolete]
        public SkyboxRequest(
            string prompt,
            Texture2D controlImage,
            string controlModel = null,
            string negativeText = null,
            int? seed = null,
            int? skyboxStyleId = null,
            int? remixImagineId = null,
            bool depth = false)
            : this(
                prompt,
                new MemoryStream(controlImage.EncodeToPNG()),
                !string.IsNullOrWhiteSpace(controlImage.name) ? $"{controlImage.name}.png" : null,
                controlModel,
                negativeText,
                seed,
                skyboxStyleId,
                remixImagineId,
                depth)
        {
            if (controlImage.height != 512 || controlImage.width != 1024)
            {
                throw new ArgumentException($"{nameof(ControlImage)} dimensions should be 512x1024");
            }
        }

        /// <summary>
        /// Creates a new Skybox Request.
        /// </summary>
        /// <param name="prompt">
        /// Text prompt describing the skybox world you wish to create.
        /// Maximum number of characters: 550.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the max-char response parameter defined for each style.
        /// </param>
        /// <param name="controlImage">
        /// <see cref="Stream"/> data of control image for request.
        /// </param>
        /// <param name="controlImageFileName">
        /// File name of <see cref="controlImage"/>.
        /// </param>
        /// <param name="controlModel">
        /// Model used for the <see cref="ControlImage"/>.
        /// Currently the only option is: "scribble".
        /// </param>
        /// <param name="negativeText">
        /// Describe things to avoid in the skybox world you wish to create.
        /// Maximum number of characters: 200.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the negative-text-max-char response parameter defined for each style.
        /// </param>
        /// <param name="enhancePrompt">
        /// Have an AI automatically improve your prompt to generate pro-level results every time (default: false)
        /// </param>
        /// <param name="seed">
        /// Send 0 for a random seed generation.
        /// Any other number (1-2147483647) set will be used to "freeze" the image generator generator and
        /// create similar images when run again with the same seed and settings.
        /// </param>
        /// <param name="skyboxStyleId">
        /// Id of predefined style that influences the overall aesthetic of your skybox generation.
        /// </param>
        /// <param name="remixImagineId">
        /// ID of a previously generated skybox.
        /// </param>
        public SkyboxRequest(
            string prompt,
            Stream controlImage,
            string controlImageFileName,
            string controlModel = null,
            string negativeText = null,
            bool? enhancePrompt = null,
            int? seed = null,
            int? skyboxStyleId = null,
            int? remixImagineId = null)
            : this(prompt, negativeText, enhancePrompt, seed, skyboxStyleId, remixImagineId)
        {
            ControlImage = controlImage;

            if (string.IsNullOrWhiteSpace(controlImageFileName))
            {
                const string defaultImageName = "control_image.png";
                controlImageFileName = defaultImageName;
            }

            ControlImageFileName = controlImageFileName;
            ControlModel = controlModel;
        }

        [Obsolete]
        public SkyboxRequest(
            string prompt,
            Stream controlImage,
            string controlImageFileName,
            string controlModel = null,
            string negativeText = null,
            int? seed = null,
            int? skyboxStyleId = null,
            int? remixImagineId = null,
            bool depth = false)
            : this(prompt, negativeText, seed, skyboxStyleId, remixImagineId, depth)
        {
            ControlImage = controlImage;

            if (string.IsNullOrWhiteSpace(controlImageFileName))
            {
                const string defaultImageName = "control_image.png";
                controlImageFileName = defaultImageName;
            }

            ControlImageFileName = controlImageFileName;
            ControlModel = controlModel;
        }

        ~SkyboxRequest() => Dispose(false);

        /// <summary>
        /// Text prompt describing the skybox world you wish to create.
        /// Maximum number of characters: 550.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the max-char response parameter defined for each style.
        /// </summary>
        public string Prompt { get; }

        /// <summary>
        /// Describe things to avoid in the skybox world you wish to create.
        /// Maximum number of characters: 200.
        /// If you are using <see cref="SkyboxStyleId"/> then the maximum number of characters is defined
        /// in the negative-text-max-char response parameter defined for each style.
        /// </summary>
        public string NegativeText { get; }

        /// <summary>
        /// Have an AI automatically improve your prompt to generate pro-level results every time (default: false)
        /// </summary>
        public bool? EnhancePrompt { get; }

        /// <summary>
        /// Send 0 for a random seed generation.
        /// Any other number (1-2147483647) set will be used to "freeze" the image generator generator and
        /// create similar images when run again with the same seed and settings.
        /// </summary>
        public int? Seed { get; }

        /// <summary>
        /// Id of predefined style that influences the overall aesthetic of your skybox generation.
        /// </summary>
        public int? SkyboxStyleId { get; }

        /// <summary>
        /// ID of a previously generated skybox.
        /// </summary>
        public int? RemixImagineId { get; }

        /// <summary>
        /// Return depth map image.
        /// </summary>
        [Obsolete]
        public bool Depth { get; }

        /// <summary>
        /// Control image used to influence the generation.
        /// The image needs to be exactly 1024 pixels wide and 512 pixels tall PNG equirectangular projection image
        /// of a scribble with black background and white brush strokes.
        /// </summary>
        public Stream ControlImage { get; }

        /// <summary>
        /// File name of <see cref="ControlImage"/>.
        /// </summary>
        public string ControlImageFileName { get; }

        /// <summary>
        /// Model used for the <see cref="ControlImage"/>.
        /// Currently the only option is: "scribble".
        /// </summary>
        public string ControlModel { get; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ControlImage?.Close();
                ControlImage?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
