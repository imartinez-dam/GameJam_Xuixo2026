using UnityEngine;

namespace Xuixo
{
    /// <summary>Reprodueix un spritesheet (fotogrames Horitzontals) i s'autodestrueix.</summary>
    public class AnimacioSprite : MonoBehaviour
    {
        public Sprite[] fotogrames;
        public float fps = 12f;
        public float esperaFinal = 0.1f;

        SpriteRenderer _sr;
        float _t;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (fotogrames == null || fotogrames.Length == 0)
            {
                Destroy(gameObject);
                return;
            }

            _t += Time.deltaTime;
            int i = (int)(_t * fps);

            if (i >= fotogrames.Length)
            {
                Destroy(gameObject, esperaFinal);
                enabled = false;
                return;
            }

            if (_sr != null) _sr.sprite = fotogrames[i];
        }
    }
}
