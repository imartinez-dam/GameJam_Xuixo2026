using UnityEngine;
using UnityEngine.SceneManagement;
using Xuixo.UI;

namespace Xuixo.Escena1
{
    /// <summary>Interficie (HUD) de l'escena 1: titol, consells, barra d'impuls i pantalla final.</summary>
    public class HUD_Escena1 : MonoBehaviour
    {
        [Header("Referencies")]
        public MasaJugador masa;
        public RodilloFinal final;

        [Header("Art (opcional)")]
        public Texture2D logotip;
        public Texture2D barraMarc;
        public Texture2D barraFarciment;
        public Texture2D panell;
        public Texture2D iconaEsquerra;
        public Texture2D iconaDreta;
        public Texture2D indicadorActiu;
        public Texture2D indicadorInactiu;
        public Texture2D botoPremut;
        public Texture2D botoNormal;

        GUIStyle _consell;
        GUIStyle _petit;
        GUIStyle _gran;
        GUIStyle _mitja;
        GUIStyle _mitjaNegreta;
        Texture2D _blanc;
        FryerActionCue _cueBol;
        GameObject _canvasCueBol;

        void Awake()
        {
            _blanc = Texture2D.whiteTexture;
        }

        void Update()
        {
            if (final != null && final.Acabat && EntradaJugador.Reintentar())
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

#if UNITY_EDITOR
            if (EntradaJugador.Sortir())
                UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        void OnGUI()
        {
            if (masa == null) return;

            float k = Mathf.Max(0.65f, Screen.height / 1080f);
            PreparaEstils(k);

            // Capçalera (logotip/títol + indicador d'escena) ja la genera CabeceraEsquena.
        }

        void LateUpdate()
        {
            if (masa == null || masa.bol == null || masa.HaSortit)
            {
                if (_cueBol != null) _cueBol.enabled = false;
                return;
            }
            Camera cameraJoc = Camera.main;
            if (cameraJoc == null)
            {
                if (_cueBol != null) _cueBol.enabled = false;
                return;
            }

            Vector3 puntBol = masa.bol.transform.position;
            puntBol.y = masa.bol.yVora;
            Vector3 pantalla = cameraJoc.WorldToScreenPoint(puntBol);
            if (pantalla.z <= 0f)
            {
                if (_cueBol != null) _cueBol.enabled = false;
                return;
            }

            if (_cueBol == null) CreaIndicadorBol();
            RectTransform rect = _cueBol.rectTransform;
            float escala = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
            Vector2 mida = new Vector2(132f, 48f) * escala;
            rect.sizeDelta = mida;
            rect.anchoredPosition = new Vector2(
                Mathf.Clamp(pantalla.x, mida.x * 0.5f, Screen.width - mida.x * 0.5f),
                Mathf.Clamp(pantalla.y + mida.y * 0.5f, mida.y * 0.35f, Screen.height - mida.y * 0.5f));
            float entrada = EntradaJugador.Llegeix().x;
            _cueBol.ShowRocking(entrada < -0.15f ? -1 : entrada > 0.15f ? 1 : 0);
        }

        void CreaIndicadorBol()
        {
            _canvasCueBol = new GameObject("Bowl Action Cue", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(_canvasCueBol, gameObject.scene);
            Canvas canvas = _canvasCueBol.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            GameObject indicador = new GameObject("Rock the Dough", typeof(RectTransform), typeof(FryerActionCue));
            indicador.transform.SetParent(_canvasCueBol.transform, false);
            _cueBol = indicador.GetComponent<FryerActionCue>();
            _cueBol.raycastTarget = false;
            _cueBol.rectTransform.anchorMin = _cueBol.rectTransform.anchorMax = Vector2.zero;
        }

        void OnDisable()
        {
            if (_cueBol != null) _cueBol.enabled = false;
        }

        void OnDestroy()
        {
            if (_canvasCueBol != null) Destroy(_canvasCueBol);
        }
       
        void PreparaEstils(float k)
        {
            if (_consell == null)
            {
                _consell = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                _petit = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                _gran = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                _mitja = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                _mitjaNegreta = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
            }

            _consell.fontSize = Mathf.RoundToInt(34f * k);
            _petit.fontSize = Mathf.RoundToInt(22f * k);
            _gran.fontSize = Mathf.RoundToInt(46f * k);
            _mitja.fontSize = Mathf.RoundToInt(28f * k);
            _mitjaNegreta = new GUIStyle(_mitja) { fontStyle = FontStyle.Bold };
        }
    }
}
