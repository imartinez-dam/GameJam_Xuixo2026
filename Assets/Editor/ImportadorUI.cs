using UnityEditor;
using UnityEngine;

namespace Xuixo.EditorTools
{
    /// <summary>
    /// Configura les textures de la interficie: alfa, sense mipmaps ni compressio
    /// (la compressio DXT trenca els colors plans i les vores del pixel-art).
    /// </summary>
    public class ImportadorUI : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            string ruta = assetPath.Replace('\\', '/');
            if (!ruta.StartsWith("Assets/Art/UI/")) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            TextureImporterPlatformSettings plataforma = importer.GetDefaultPlatformTextureSettings();
            plataforma.maxTextureSize = 4096;
            plataforma.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(plataforma);
        }
    }
}
