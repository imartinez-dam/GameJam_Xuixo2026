using System.IO;
using UnityEditor;
using UnityEngine;

namespace Xuixo.EditorTools
{
    /// <summary>
    /// Art de suport de l'escena 1. El fons i la massa venen de fora
    /// de la carpeta de contingut del bol; aqui nomes generem l'ombra tova.
    /// </summary>
    public static class ArtEscena1
    {
        public const string Carpeta = ContentPaths.Bowl + "Art";

        // Imatges externes
        public const string RutaFons = Carpeta + "/Backgrounds/Escena2.jpg";
        public const string RutaMasa = ContentPaths.BowlWorld + "dough_pixel.png";

        // Mides al mon
        public const float AMPLADA_FONS = 15f;
        public const float AMPLADA_MASA = 0.4f;

        public const int PPU_OMBRA = 200;

        public static Sprite Ombra { get; private set; }
        public static Sprite Fons { get; private set; }
        public static Sprite Masa { get; private set; }

        public static void GeneraTots()
        {
            Directory.CreateDirectory(Path.GetFullPath(Carpeta));

            Ombra = Desa("ombra", DibuixaOmbra(), 256, 128, PPU_OMBRA);
            Fons = ImportaImatge(RutaFons, AMPLADA_FONS, false);
            Masa = ImportaImatge(RutaMasa, AMPLADA_MASA, true);

            AssetDatabase.SaveAssets();
        }

        /// <summary>Ombra radial tova (escala amb la massa).</summary>
        static Color32[] DibuixaOmbra()
        {
            const int W = 256, H = 128;
            var p = new Color32[W * H];
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    float dx = (x + 0.5f - W * 0.5f) / (W * 0.47f);
                    float dy = (y + 0.5f - H * 0.5f) / (H * 0.44f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float cov = Mathf.Clamp01(1f - d);
                    cov = cov * cov;
                    if (cov <= 0f) continue;
                    p[y * W + x] = new Color(0.08f, 0.05f, 0.03f, cov * 0.85f);
                }
            }
            return p;
        }

        static Sprite Desa(string nom, Color32[] pixels, int amplada, int alcada, int ppu)
        {
            string rel = Carpeta + "/" + nom + ".png";
            string abs = Path.GetFullPath(rel);

            var tex = new Texture2D(amplada, alcada, TextureFormat.RGBA32, false);
            tex.SetPixels32(pixels);
            tex.Apply();
            File.WriteAllBytes(abs, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(rel, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(rel);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int)SpriteImportMode.Single;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(rel);
        }

        /// <summary>
        /// Importa una imatge externa com a sprite d'una sola peça amb l'amplada
        /// de mon indicada (aixi sempre fa la mida que toca al joc).
        /// </summary>
        public static Sprite ImportaImatge(string ruta, float ampladaUnitats, bool alfa)
        {
            if (!File.Exists(Path.GetFullPath(ruta)))
            {
                Debug.LogWarning("[Xuixo] No s'ha trobat la imatge: " + ruta);
                return null;
            }

            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(ruta);
            importer.GetSourceTextureWidthAndHeight(out int ampladaPx, out int alcadaPx);
            if (ampladaPx <= 0) ampladaPx = 1000;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ampladaPx / Mathf.Max(0.01f, ampladaUnitats);
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = alfa;
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

            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        }
    }
}
