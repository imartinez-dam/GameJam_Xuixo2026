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
            if (!ruta.StartsWith(ContentPaths.UiArt)) return;

            var importer = (TextureImporter)assetImporter;
            Vector4 border = System.IO.Path.GetFileNameWithoutExtension(ruta) switch
            {
                "panell_9slice" => new Vector4(16, 16, 16, 16),
                "boto_normal" or "boto_hover" or "boto_premut" => new Vector4(28, 24, 28, 24),
                _ => Vector4.zero
            };
            importer.textureType = border == Vector4.zero ? TextureImporterType.Default : TextureImporterType.Sprite;
            if (border != Vector4.zero)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = border;
            }
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
