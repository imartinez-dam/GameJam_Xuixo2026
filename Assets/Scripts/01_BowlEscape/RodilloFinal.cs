using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Xuixo.Escena1
{
    /// <summary>
    /// El corró del cuiner: quan la massa hi arriba, la captura, l'aplasta
    /// i fa la transicio cap a la seguent pantalla.
    /// </summary>
    public class RodilloFinal : MonoBehaviour
    {
        [Header("Referencies")]
        public MasaJugador masa;
        public CamaraEscena1 camara;

        [Header("Transicio")]
        [Tooltip("Escena que es carregara despres (si existeix a Build Settings)")]
        public string escenaSeguent = SceneNames.StretchDough;

        public float duradaAplastament = 0.55f;
        public float pausaAbansDeFos = 0.9f;
        public float duradaFos = 1.1f;

        /// <summary>Ja s'ha tocat el corró i comenca la sequencia final.</summary>
        public bool Activat { get; private set; }

        /// <summary>La sequencia ha acabat i no hi ha cap escena seguent: es mostra el panell.</summary>
        public bool Acabat { get; private set; }

        /// <summary>Opacitat del fos a negre (0..1).</summary>
        public float Opacitat { get; private set; }

        /// <summary>Existeix una escena seguent configurada i disponible.</summary>
        public bool TeSeguent { get; private set; }

        void OnTriggerEnter2D(Collider2D altres)
        {
            if (Activat) return;

            MasaJugador m = altres.GetComponentInParent<MasaJugador>();
            if (m == null) return;

            Activat = true;
            if (masa == null) masa = m;
            StartCoroutine(Sequencia());
        }

        IEnumerator Sequencia()
        {
            masa.Capturar();

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, duradaAplastament);
                //masa.Aplastar(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * 7f);
                yield return null;
            }

            transform.localRotation = Quaternion.identity;

            if (camara != null)
                camara.Sacsejar(0.35f);

            yield return new WaitForSeconds(pausaAbansDeFos);

            TeSeguent = EscenaDisponible(escenaSeguent);

            float o = 0f;
            while (o < 1f)
            {
                o += Time.deltaTime / Mathf.Max(0.01f, duradaFos);
                Opacitat = Mathf.Clamp01(o);
                yield return null;
            }

            if (TeSeguent)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.TryAddScore(GameManager.BowlStage,
                        Mathf.RoundToInt(masa.CarregaEnSortir * GameManager.PointLimit(GameManager.BowlStage)));
                SceneManager.LoadScene(escenaSeguent);
            }
            else
            {
                Acabat = true;
            }
        }

        static bool EscenaDisponible(string nom)
        {
            if (string.IsNullOrEmpty(nom)) return false;

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string ruta = SceneUtility.GetScenePathByBuildIndex(i);
                if (Path.GetFileNameWithoutExtension(ruta) == nom)
                    return true;
            }

            return false;
        }
    }
}
