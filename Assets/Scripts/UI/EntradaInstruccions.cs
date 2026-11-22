using UnityEngine;

namespace Xuixo.UI
{
    internal sealed class EntradaInstruccions : MonoBehaviour
    {
        private const float TempsLectura = 2f;
        private const float TempsPujada = 1f;
        private const float EscalaInicial = 1.25f;

        private RectTransform panell;
        private RectTransform inici;
        private RectTransform final;
        private float tempsInicial;

        internal void Prepara(RectTransform panell, RectTransform inici, RectTransform final)
        {
            this.panell = panell;
            this.inici = inici;
            this.final = final;
            tempsInicial = Time.unscaledTime;
            ColLocaPanell(0f);
        }

        private void Update()
        {
            float temps = Time.unscaledTime - tempsInicial;
            if (temps < TempsLectura)
                return;

            float progres = Mathf.Clamp01((temps - TempsLectura) / TempsPujada);
            ColLocaPanell(Mathf.SmoothStep(0f, 1f, progres));

            if (progres >= 1f)
                enabled = false;
        }

        private void ColLocaPanell(float progres)
        {
            panell.anchorMin = panell.anchorMax = Vector2.Lerp(inici.anchorMin, final.anchorMin, progres);
            panell.pivot = Vector2.Lerp(inici.pivot, final.pivot, progres);
            panell.anchoredPosition = Vector2.Lerp(inici.anchoredPosition, final.anchoredPosition, progres);
            panell.localScale = Vector3.one * Mathf.Lerp(EscalaInicial, 1f, progres);
        }
    }
}
