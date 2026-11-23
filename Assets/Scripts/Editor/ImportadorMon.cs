using UnityEditor;
using UnityEngine;

namespace Xuixo.EditorTools
{
    /// <summary>
    /// Configura els sprites de mon (objectes, efectes i la massa normalitzada)
    /// a la densitat dels fons: 160 px per unitat de joc.
    /// </summary>
    public class ImportadorMon : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            string ruta = assetPath.Replace('\\', '/');
            if (!ruta.StartsWith(ContentPaths.BowlWorld)
                && !ruta.StartsWith(ContentPaths.WorldArt)
                && !ruta.StartsWith(ContentPaths.Root + SceneNames.Intro + "/Art/World/")
                && !ruta.StartsWith(ContentPaths.Root + SceneNames.StretchDough + "/Art/World/")
                && !ruta.StartsWith(ContentPaths.Root + "Archive/Art/World/")) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 160f;
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

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int)SpriteImportMode.Single;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
        }
    }
}
