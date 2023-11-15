// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.SharpZipLib.Zip;
using UnityEngine;

namespace BlockadeLabs
{
    internal static class ExportUtilities
    {
        /// <summary>
        /// Unzips the archive at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of files in the archive.</returns>
        internal static async Task<IReadOnlyList<string>> UnZipAsync(string path, CancellationToken cancellationToken = default)
        {
            path = path.Replace("file://", string.Empty);
            var zipStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var directory = Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path));

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileList = new List<string>();

            try
            {
                var zipFile = new ZipFile(zipStream);

                foreach (ZipEntry entry in zipFile)
                {
                    var filePath = Path.Combine(directory, entry.Name);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    var parentDirectory = Directory.GetParent(filePath)!;

                    if (!parentDirectory.Exists)
                    {
                        Directory.CreateDirectory(parentDirectory.FullName);
                    }

                    var itemStream = zipFile.GetInputStream(entry);

                    try
                    {
                        var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

                        try
                        {
                            await itemStream.CopyToAsync(fileStream, cancellationToken);
                            await fileStream.FlushAsync(cancellationToken);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                        finally
                        {
                            fileStream.Close();
                            await fileStream.DisposeAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    finally
                    {
                        itemStream.Close();
                        await itemStream.DisposeAsync();
                    }

                    fileList.Add(filePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                await zipStream.DisposeAsync();
            }

            return fileList;
        }

        /// <summary>
        /// Creates a cubemap with specified files.
        /// </summary>
        /// <param name="textures">List of local files to be used to create <see cref="Cubemap"/>.</param>
        /// <returns><see cref="Cubemap"/></returns>
        internal static Cubemap BuildCubemap(IReadOnlyList<Texture2D> textures)
        {
            if (textures is not { Count: 6 })
            {
                throw new ArgumentException("Must have 6 textures for each face of the cubemap");
            }

            var cubemap = new Cubemap(textures.First().width, TextureFormat.RGB24, false, true);

            foreach (var face in textures)
            {
                switch (face.name)
                {
                    case "cube_up":
                        cubemap.SetPixels(face.GetPixels(), CubemapFace.PositiveY);
                        break;
                    case "cube_down":
                        cubemap.SetPixels(face.GetPixels(), CubemapFace.NegativeY);
                        break;
                    case "cube_front":
                        cubemap.SetPixels(face.GetPixels(), CubemapFace.PositiveZ);
                        break;
                    case "cube_back":
                        cubemap.SetPixels(face.GetPixels(), CubemapFace.NegativeZ);
                        break;
                    case "cube_right":
                        cubemap.SetPixels(face.GetPixels(), CubemapFace.PositiveX);
                        break;
                    case "cube_left":
                        cubemap.SetPixels(face.GetPixels(), CubemapFace.NegativeX);
                        break;
                }
            }

            cubemap.Apply(false);
            return cubemap;
        }

        internal enum TextureRotation
        {
            Clockwise90,
            Counterclockwise90,
            Rotate180
        }

        internal static Texture2D Rotate(this Texture2D texture, TextureRotation rotation)
        {
            var width = texture.width;
            var height = texture.height;
            var pixels = texture.GetPixels32();
            var rotatedPixels = new Color32[pixels.Length];

            switch (rotation)
            {
                case TextureRotation.Clockwise90:
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            rotatedPixels[x * height + (height - y - 1)] = pixels[y * width + x];
                        }
                    }
                    break;

                case TextureRotation.Counterclockwise90:
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            rotatedPixels[(width - x - 1) * height + y] = pixels[y * width + x];
                        }
                    }
                    break;

                case TextureRotation.Rotate180:
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        rotatedPixels[i] = pixels[pixels.Length - i - 1];
                    }
                    break;
            }

            texture.SetPixels32(rotatedPixels);
            texture.Apply();
            return texture;
        }
    }
}
