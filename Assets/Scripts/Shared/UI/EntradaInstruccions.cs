using UnityEngine;

namespace Xuixo.UI
{
    public sealed class EntradaInstruccions : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float tempsLectura = 2f;
        [SerializeField, Min(0.01f)] private float tempsPujada = 1f;
        [SerializeField, Min(0.1f)] private float escalaInicial = 1.25f;

        [SerializeField] private RectTransform panell;
        [SerializeField] private RectTransform inici;
        private float tempsInicial;

        private Vector2 ancoraFinal, pivotFinal, posicioFinal;
        private Vector3 escalaFinal;

        private void Start()
        {
            ancoraFinal = panell.anchorMin;
            pivotFinal = panell.pivot;
            posicioFinal = panell.anchoredPosition;
            escalaFinal = panell.localScale;
            tempsInicial = Time.unscaledTime;
            ColLocaPanell(0f);
        }

        private void Update()
        {
            float temps = Time.unscaledTime - tempsInicial;
            if (temps < tempsLectura)
                return;

            float progres = Mathf.Clamp01((temps - tempsLectura) / tempsPujada);
            ColLocaPanell(Mathf.SmoothStep(0f, 1f, progres));

            if (progres >= 1f)
                enabled = false;
        }

        private void ColLocaPanell(float progres)
        {
            panell.anchorMin = panell.anchorMax = Vector2.Lerp(inici.anchorMin, ancoraFinal, progres);
            panell.pivot = Vector2.Lerp(inici.pivot, pivotFinal, progres);
            panell.anchoredPosition = Vector2.Lerp(inici.anchoredPosition, posicioFinal, progres);
            panell.localScale = escalaFinal * Mathf.Lerp(escalaInicial, 1f, progres);
        }
    }
}
