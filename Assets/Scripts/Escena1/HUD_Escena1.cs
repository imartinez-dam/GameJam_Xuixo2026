using UnityEngine;
using UnityEngine.SceneManagement;

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