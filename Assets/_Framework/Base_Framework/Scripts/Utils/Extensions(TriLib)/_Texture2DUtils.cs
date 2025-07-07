using System.Collections;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;
using UnityEngine;
using System;

namespace _Base_Framework
{


    public class _Texture2DUtils
    {
        public static Texture2D LoadImageAtPath(string imagePath, /*int maxSize = -1,*/ bool markTextureNonReadable = true,
            bool generateMipmaps = true, bool linearColorSpace = false)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                throw new ArgumentException("Parameter 'imagePath' is null or empty!");
            }

            if (File.Exists(imagePath) == false)
            {
                throw new FileNotFoundException("File not found at " + imagePath);
            }

            /*
            if (maxSize <= 0)
            {
                maxSize = SystemInfo.maxTextureSize;
            }
            */

            /*
#if !UNITY_EDITOR && UNITY_ANDROID
		string loadPath = AJC.CallStatic<string>( "LoadImageAtPath", Context, imagePath, TemporaryImagePath, maxSize );
#elif !UNITY_EDITOR && UNITY_IOS
		string loadPath = _NativeGallery_LoadImageAtPath( imagePath, TemporaryImagePath, maxSize );
#else
            */
            string loadPath = imagePath;
            /*
#endif
            */

            string extension = Path.GetExtension(imagePath).ToLowerInvariant();
            TextureFormat format = (extension == ".jpg" || extension == ".jpeg") ? TextureFormat.RGB24 : TextureFormat.RGBA32;

            Texture2D result = new Texture2D(2, 2, format, generateMipmaps, linearColorSpace);

            try
            {
                if (result.LoadImage(File.ReadAllBytes(loadPath), markTextureNonReadable) == false)
                {
                    Object.DestroyImmediate(result);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                Object.DestroyImmediate(result);
                return null;
            }
            finally
            {
                if (loadPath != imagePath)
                {
                    try
                    {
                        File.Delete(loadPath);
                    }
                    catch
                    {
                        //
                    }
                }
            }

            return result;
        }

        public static void SaveTexture(Texture2D texture, string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath) == true)
            {
                filePath = Path.Combine(Application.temporaryCachePath, "temp_Image.png");
            }
            File.WriteAllBytes(filePath, GetTextureBytes(texture, false));
        }

        public static byte[] GetTextureBytes(Texture2D texture, bool isJpeg)
        {
            try
            {
                return isJpeg ? texture.EncodeToJPG(100) : texture.EncodeToPNG();
            }
            catch (UnityException)
            {
                return GetTextureBytesFromCopy(texture, isJpeg);
            }
            catch (System.ArgumentException)
            {
                return GetTextureBytesFromCopy(texture, isJpeg);
            }

#pragma warning disable 0162
            return null;
#pragma warning restore 0162
        }

        private static byte[] GetTextureBytesFromCopy(Texture2D texture, bool isJpeg)
        {
            // Texture is marked as non-readable, create a readable copy and share it instead
            Debug.LogWarning("Sharing non-readable textures is slower than sharing readable textures");

            Texture2D sourceTexReadable = null;
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height);
            RenderTexture activeRT = RenderTexture.active;

            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;

                sourceTexReadable = new Texture2D(texture.width, texture.height, isJpeg ? TextureFormat.RGB24 : TextureFormat.RGBA32, false);
                sourceTexReadable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
                sourceTexReadable.Apply(false, false);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);

                Object.DestroyImmediate(sourceTexReadable);
                return null;
            }
            finally
            {
                RenderTexture.active = activeRT;
                RenderTexture.ReleaseTemporary(rt);
            }

            try
            {
                return isJpeg ? sourceTexReadable.EncodeToJPG(100) : sourceTexReadable.EncodeToPNG();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return null;
            }
            finally
            {
                Object.DestroyImmediate(sourceTexReadable);
            }
        }
    }
}