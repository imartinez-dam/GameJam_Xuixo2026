using UnityEngine;

namespace Xuixo.Escena1
{
    /// <summary>Efectes de particules de l'escena 1 (farina, estrelles).</summary>
    public class VfxEscena1 : MonoBehaviour
    {
        [Header("Referencies")]
        public MasaJugador masa;
        public RodilloFinal final;

        [Header("Spritesheets (fotogrames horitzontals)")]
        public Texture2D farina;
        public Texture2D estrelles;

        [Header("Ajustos")]
        public float midaFarina = 0.55f;
        public float midaEstrelles = 0.65f;
        public float fps = 12f;
        public float cadaPolsim = 0.35f;

        MasaJugador.Estat _estatAnterior = MasaJugador.Estat.Capturada;
        float _properPolsim;

        void Awake()
        {
            if (masa != null) _estatAnterior = masa.EstatActual;
        }

        void Update()
        {
            if (masa == null) return;

            MasaJugador.Estat estat = masa.EstatActual;
            if (estat != _estatAnterior) CanviEstat(_estatAnterior, estat);
            _estatAnterior = estat;

            // polsims mentre s'agafa impuls
            if (estat == MasaJugador.Estat.DinsBol && masa.Carrega > 0.25f && Time.time >= _properPolsim)
            {
                _properPolsim = Time.time + cadaPolsim;
                Crea(farina, masa.transform.position + Vector3.up * 0.05f, midaFarina * 0.45f, 40);
            }
        }

        void CanviEstat(MasaJugador.Estat anterior, MasaJugador.Estat nou)
        {
            if (nou == MasaJugador.Estat.Sortint)
                Crea(farina, masa.transform.position, midaFarina, 40);

            if (nou == MasaJugador.Estat.Carrera)
                Crea(farina, masa.transform.position + Vector3.down * 0.05f, midaFarina * 0.7f, 40);

            if (nou == MasaJugador.Estat.Capturada)
            {
                Vector3 punt = final != null ? final.transform.position : masa.transform.position;
                Crea(estrelles, punt + Vector3.up * 0.15f, midaEstrelles, 45);
            }
        }

        void Crea(Texture2D textura, Vector3 posicio, float mida, int ordre)
        {
            if (textura == null || textura.height <= 0) return;

            int fotogrames = Mathf.Max(1, textura.width / textura.height);
            var go = new GameObject("Vfx " + textura.name);
            go.transform.position = new Vector3(posicio.x, posicio.y, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = ordre;

            var anim = go.AddComponent<AnimacioSprite>();
            anim.fps = fps;

            var sprites = new Sprite[fotogrames];
            float ppu = textura.height / Mathf.Max(0.01f, mida);
            for (int i = 0; i < fotogrames; i++)
            {
                var rect = new Rect(i * textura.height, 0f, textura.height, textura.height);
                sprites[i] = Sprite.Create(textura, rect, new Vector2(0.5f, 0.5f), ppu);
            }

            anim.fotogrames = sprites;
            sr.sprite = sprites[0];
        }
    }
}
