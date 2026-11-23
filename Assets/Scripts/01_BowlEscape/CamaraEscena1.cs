using UnityEngine;

namespace Xuixo.Escena1
{
    /// <summary>Camera 2D que segueix la massa amb un petit suavitzat i permet sacsejar-la.</summary>
    public class CamaraEscena1 : MonoBehaviour
    {
        [Header("Seguiment")]
        public Transform objectiu;
        public float minX = 0f;
        public float maxX = 22f;
        public float y = -0.8f;
        public float suavitzat = 0.18f;

        float _sacseig;

        void LateUpdate()
        {
            if (objectiu == null) return;

            float x = Mathf.Clamp(objectiu.position.x, minX, maxX);
            Vector3 desti = new Vector3(x, y, -10f);
            float k = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.001f, suavitzat));
            transform.position = Vector3.Lerp(transform.position, desti, k);

            if (_sacseig > 0f)
            {
                _sacseig = Mathf.Max(0f, _sacseig - Time.deltaTime);
                Vector2 desplacament = Random.insideUnitCircle * (_sacseig * 0.45f);
                transform.position += new Vector3(desplacament.x, desplacament.y, 0f);
            }
        }

        public void Sacsejar(float durada)
        {
            _sacseig = Mathf.Max(_sacseig, durada);
        }
    }
}
