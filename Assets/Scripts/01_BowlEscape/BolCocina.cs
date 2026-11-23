using UnityEngine;

namespace Xuixo.Escena1
{
    /// <summary>
    /// El bol de la cuina on comenca la massa.
    /// La vora dreta es pot obrir per deixar sortir la massa (sempre cap a la dreta).
    /// Tambe fa un petit balanceig mentre la massa agafa impuls.
    /// </summary>
    public class BolCocina : MonoBehaviour
    {
        [Header("Referencies")]
        [Tooltip("Parts visuals del bol que es balancegen (no les colisions)")]
        public Transform[] visuals;

        [Tooltip("Col·lisionador de la vora dreta, que s'obre per deixar sortir la massa")]
        public Collider2D guardaDreta;

        [Tooltip("Alcada (munda) de la vora del bol")]
        public float yVora = -0.95f;

        [Header("Balanceig")]
        public float angleMaxim = 2.4f;

        float[] _anglesBase;
        float _intensitat;

        void Awake()
        {
            if (visuals != null)
            {
                _anglesBase = new float[visuals.Length];
                for (int i = 0; i < visuals.Length; i++)
                {
                    if (visuals[i] != null)
                        _anglesBase[i] = visuals[i].localEulerAngles.z;
                }
            }
        }

        void Update()
        {
            if (visuals == null) return;

            float angle = Mathf.Sin(Time.time * (9f + 14f * _intensitat)) * angleMaxim * _intensitat;
            for (int i = 0; i < visuals.Length; i++)
            {
                if (visuals[i] == null) continue;
                visuals[i].localRotation = Quaternion.Euler(0f, 0f, _anglesBase[i] + angle);
            }
        }

        /// <summary>Intensitat del balanceig (0..1), normalment la carrega de la massa.</summary>
        public void SetIntensitat(float valor)
        {
            _intensitat = Mathf.Clamp01(valor);
        }

        /// <summary>Obre el costat dret del bol perque la massa hi pugui sortir.</summary>
        public void ObrirBandaDreta()
        {
            if (guardaDreta != null)
                guardaDreta.enabled = false;
        }
    }
}
