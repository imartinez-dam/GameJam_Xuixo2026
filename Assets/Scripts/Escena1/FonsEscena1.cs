using UnityEngine;

namespace Xuixo.Escena1
{
    /// <summary>
    /// Fons de cuina (art pintat). S'ajusta per cobrir sempre la pantalla i
    /// queda ancorat a la línia de la taula del dibuix, que és on es juga.
    /// </summary>
    [ExecuteAlways]
    public class FonsEscena1 : MonoBehaviour
    {
        [Tooltip("SpriteRenderer del fons")]
        public SpriteRenderer fons;

        [Range(0f, 0.5f)]
        [Tooltip("Frac de l'alçada de la imatge on hi ha la superficie de la taula (0 = baix)")]
        public float fraccioTerra = 0.2435f;

        [Tooltip("Alçada (munda) de la superficie on es juga")]
        public float yTerra = -3f;

        [Tooltip("Desplacament horitzontal per donar una mica de parallax")]
        public float parallax = 0f;

        Camera _camera;

        void OnEnable()
        {
            _camera = Camera.main;
            Ajusta();
        }

        void LateUpdate()
        {
            Ajusta();
        }

        /// <summary>Col·loca i escala el fons perque cobreixi la pantalla i la taula quedi a yTerra.</summary>
        public void Ajusta()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null || fons == null || fons.sprite == null) return;

            Vector2 mida = fons.sprite.bounds.size;
            if (mida.x <= 0.001f || mida.y <= 0.001f) return;

            float alcada = _camera.orthographicSize * 2f;
            float amplada = alcada * _camera.aspect;

            // Cobrir tota la pantalla sense deformar la imatge.
            float escala = Mathf.Max(amplada / mida.x, alcada / mida.y);
            transform.localScale = new Vector3(escala, escala, 1f);

            float y = yTerra + (0.5f - fraccioTerra) * mida.y * escala;
            float x = _camera.transform.position.x * parallax;
            transform.position = new Vector3(x, y, 0f);
        }
    }
}
