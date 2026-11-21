using UnityEngine;

namespace Xuixo.UI
{
    /// <summary>
    /// Tema visual compartit del joc: fa servir el kit d'UI d'Art/UI
    /// (botons de fusta i panell de pergami de l'Escena 1).
    /// Es carrega amb Resources.Load&lt;TemaPastisseria&gt;("TemaPastisseria").
    /// Les textures es converteixen en sprites retallables (9 talls) en runtime.
    /// </summary>
    public class TemaPastisseria : ScriptableObject
    {
        [Header("Textures del kit (Art/UI)")]
        public Texture2D panell;
        public Texture2D botoNormal;
        public Texture2D botoHover;
        public Texture2D botoPremut;

        [Header("Vores per al 9 talls (píxels)")]
        public int voraPanell = 16;
        public int voraBotoX = 28;
        public int voraBotoY = 24;

        public static readonly Color ColorTextFosc = new Color(0.29f, 0.18f, 0.09f);
        public static readonly Color ColorTextClar = new Color(0.965f, 0.925f, 0.84f);
        public static readonly Color ColorCaramel = new Color(0.9098f, 0.6392f, 0.2392f);

        private static TemaPastisseria _instancia;

        /// <summary>Carrega el tema (amb reserva null si falta l'asset).</summary>
        public static TemaPastisseria Carrega()
        {
            if (_instancia == null)
            {
                _instancia = Resources.Load<TemaPastisseria>("TemaPastisseria");
                if (_instancia == null)
                    Debug.LogError("TemaPastisseria: no s'ha trobat l'asset a Resources/TemaPastisseria.");
            }
            return _instancia;
        }

        private Sprite _spritePanell;
        private Sprite _spriteBotoNormal;
        private Sprite _spriteBotoHover;
        private Sprite _spriteBotoPremut;

        public Sprite SpritePanell
        {
            get
            {
                if (_spritePanell == null)
                    _spritePanell = CreaRetallable(panell, voraPanell, voraPanell);
                return _spritePanell;
            }
        }

        public Sprite SpriteBotoNormal
        {
            get
            {
                if (_spriteBotoNormal == null)
                    _spriteBotoNormal = CreaRetallable(botoNormal, voraBotoX, voraBotoY);
                return _spriteBotoNormal;
            }
        }

        public Sprite SpriteBotoHover
        {
            get
            {
                if (_spriteBotoHover == null)
                    _spriteBotoHover = CreaRetallable(botoHover, voraBotoX, voraBotoY);
                return _spriteBotoHover;
            }
        }

        public Sprite SpriteBotoPremut
        {
            get
            {
                if (_spriteBotoPremut == null)
                    _spriteBotoPremut = CreaRetallable(botoPremut, voraBotoX, voraBotoY);
                return _spriteBotoPremut;
            }
        }

        /// <summary>Crea un sprite 9-talls a partir d'una textura; reserva al sprite generat si falta.</summary>
        private static Sprite CreaRetallable(Texture2D textura, int voraX, int voraY)
        {
            if (textura == null)
                return EstilPastisseria.Arrodonit;

            return Sprite.Create(
                textura,
                new Rect(0f, 0f, textura.width, textura.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(voraX, voraY, voraX, voraY));
        }
    }
}
