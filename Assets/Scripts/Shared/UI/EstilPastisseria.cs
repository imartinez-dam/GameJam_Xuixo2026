using UnityEngine;

namespace Xuixo.UI
{
    /// <summary>
    /// Estil compartit "pastisseria artesana" per a tota la UI del joc:
    /// colors de caramel, crema i fusta, amb cantonades arrodonides.
    /// Els colors coincideixen amb els del boto "Som-hi!" de l'escena inicial.
    /// </summary>
    public static class EstilPastisseria
    {
        public static readonly Color Caramel = new Color(0.9098f, 0.6392f, 0.2392f);
        public static readonly Color CaramelRessaltat = new Color(1f, 0.8235f, 0.4784f);
        public static readonly Color CaramelPremut = new Color(0.7882f, 0.5098f, 0.1647f);
        public static readonly Color Pergami = new Color(0.99f, 0.955f, 0.88f, 0.97f);
        public static readonly Color TextFosc = new Color(0.29f, 0.18f, 0.09f);
        public static readonly Color Vora = new Color(0.62f, 0.42f, 0.16f, 0.9f);
        public static readonly Color Ombra = new Color(0.25f, 0.15f, 0.06f, 0.4f);

        private static Sprite _arrodonit;

        /// <summary>Sprite blanc amb cantonades arrodonides (9 talls) generat per codi.</summary>
        public static Sprite Arrodonit
        {
            get
            {
                if (_arrodonit == null)
                    _arrodonit = GeneraArrodonit(32, 10);
                return _arrodonit;
            }
        }

        private static Sprite GeneraArrodonit(int mida, int radi)
        {
            Texture2D textura = new Texture2D(mida, mida, TextureFormat.RGBA32, false);
            textura.wrapMode = TextureWrapMode.Clamp;
            textura.filterMode = FilterMode.Bilinear;

            Color32 ple = new Color32(255, 255, 255, 255);
            Color32 buit = new Color32(255, 255, 255, 0);

            for (int y = 0; y < mida; y++)
            {
                for (int x = 0; x < mida; x++)
                {
                    int dx = Mathf.Max(0, Mathf.Max(radi - 1 - x, x - (mida - radi)));
                    int dy = Mathf.Max(0, Mathf.Max(radi - 1 - y, y - (mida - radi)));
                    textura.SetPixel(x, y, dx * dx + dy * dy <= radi * radi ? ple : buit);
                }
            }

            textura.Apply();

            return Sprite.Create(
                textura,
                new Rect(0f, 0f, mida, mida),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radi, radi, radi, radi));
        }
    }
}
