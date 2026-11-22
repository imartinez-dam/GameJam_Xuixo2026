using UnityEngine;
using UnityEngine.SceneManagement;
using Xuixo.Escena1;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Xuixo
{
    /// <summary>
    /// Capçalera compartida de les escenes: logotip (o el text "XUIXO 2026")
    /// a l'esquerra de la pantalla i punts d'indicació de l'escena actual a
    /// la dreta. S'assumeix sola en carregar cada escena
    /// (RuntimeInitializeOnLoadMethod), així totes les escenes tenen la
    /// mateixa capçalera sense editar els fitxers .unity.
    /// </summary>
    public class CabeceraEsquena : MonoBehaviour
    {
        private static GameObject _instancia;
        private static Texture2D _dot;

        [SerializeField] private Texture2D logotip;
        [SerializeField] private Texture2D indicadorActiu;
        [SerializeField] private Texture2D indicadorInactiu;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Inicia()
        {
            SceneManager.sceneLoaded += OnEscenaCarregada;
            Construeix(SceneManager.GetActiveScene().name);
        }

        private static void OnEscenaCarregada(Scene escena, LoadSceneMode mode)
        {
            Construeix(escena.name);
        }

        private static void Construeix(string nomEscena)
        {
            if (_instancia != null)
                Object.Destroy(_instancia);
            _instancia = null;

            // La capçalera no serveix a la de benvinguda (ObradorIntro).
            if (nomEscena == "ObradorIntro")
                return;

            _instancia = new GameObject("CabeceraEsquena");
            _instancia.AddComponent<CabeceraEsquena>();
            Object.DontDestroyOnLoad(_instancia);
        }

        private void OnGUI()
        {
            float k = Mathf.Max(0.65f, Screen.height / 1080f);

            // Titol: el logotip de l'Escena 1, sinó el logo_xuixo d'Art/UI (editor),
            // i si no, el text "XUIXO 2026".
            Texture2D logo = logotip;
            if (logo == null)
            {
                HUD_Escena1 hud = Object.FindObjectOfType<HUD_Escena1>(true);
                if (hud != null)
                {
                    logo = hud.logotip;
                    if (indicadorActiu == null) indicadorActiu = hud.indicadorActiu;
                    if (indicadorInactiu == null) indicadorInactiu = hud.indicadorInactiu;
                }
            }
            if (logo == null)
                logo = LogoXuixo();

            if (logo != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(34f * k, 18f * k, 300f * k, 166f * k), logo, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUIStyle titol = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft, fontStyle = FontStyle.Bold };
                titol.fontSize = Mathf.RoundToInt(58f * k);
                GUI.color = new Color(0.29f, 0.18f, 0.09f, 1f);
                GUI.Label(new Rect(48f * k, 26f * k, 1200f * k, 80f * k), "XUIXO 2026", titol);
            }

            // Punts d'indicació de l'escena (5 punts, un per escena de joc).
            const int total = 5;
            float d = 22f * k;
            float sep = 10f * k;
            float ample = total * d + (total - 1) * sep;
            float x0 = Screen.width - ample - 36f * k;
            float y0 = 30f * k;

            int escena = Mathf.Clamp(SceneManager.GetActiveScene().buildIndex - 1, 0, total - 1);

            Texture2D dotActiu = indicadorActiu != null ? indicadorActiu : IndicadorActiuAsset();
            Texture2D dotInactiu = indicadorInactiu != null ? indicadorInactiu : IndicadorInactiuAsset();

            for (int i = 0; i < total; i++)
            {
                Rect rect = new Rect(x0 + i * (d + sep), y0, d, d);
                bool actiu = i == escena;

                if (actiu)
                {
                    GUI.color = Color.white;
                    GUI.DrawTexture(rect, dotActiu != null ? dotActiu : Dot(), ScaleMode.ScaleToFit, true);
                }
                else if (dotInactiu != null)
                {
                    GUI.color = Color.white;
                    GUI.DrawTexture(rect, dotInactiu, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    GUI.color = new Color(0.45f, 0.45f, 0.45f);
                    GUI.DrawTexture(rect, Dot(), ScaleMode.ScaleToFit, true);
                }
            }

            GUI.color = Color.white;
        }

        private static Texture2D _logoAuto;
        private static Texture2D _indicadorAutoActiu;
        private static Texture2D _indicadorAutoInactiu;

        /// <summary>Logo_xuixo de Art/UI (en editor); en build, reserveda el text "XUIXO 2026").</summary>
        private static Texture2D LogoXuixo()
        {
            if (_logoAuto != null) return _logoAuto;
#if UNITY_EDITOR
            _logoAuto = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/logo_xuixo.png");
#endif
            return _logoAuto;
        }

        /// <summary>indicador_actiu.png de Art/UI (en editor); en build, reserves Dot()).</summary>
        private static Texture2D IndicadorActiuAsset()
        {
            if (_indicadorAutoActiu != null) return _indicadorAutoActiu;
#if UNITY_EDITOR
            _indicadorAutoActiu = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/indicador_actiu.png");
#endif
            return _indicadorAutoActiu;
        }

        /// <summary>indicador_inactiu.png de Art/UI (en editor); en build, reserves Dot()).</summary>
        private static Texture2D IndicadorInactiuAsset()
        {
            if (_indicadorAutoInactiu != null) return _indicadorAutoInactiu;
#if UNITY_EDITOR
            _indicadorAutoInactiu = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/indicador_inactiu.png");
#endif
            return _indicadorAutoInactiu;
        }

        /// <summary>Punt arrodonit blanc d'indicador de l'escena (per si no hi ha els altres assets).</summary>
        private static Texture2D Dot()
        {
            if (_dot == null)
            {
                const int mida = 40;
                const int radi = 15;
                _dot = new Texture2D(mida, mida, TextureFormat.ARGB32, false);
                for (int y = 0; y < mida; y++)
                    for (int x = 0; x < mida; x++)
                    {
                        float dx = x - mida * 0.5f + 0.5f;
                        float dy = y - mida * 0.5f + 0.5f;
                        _dot.SetPixel(x, y, dx * dx + dy * dy <= radi * radi ? Color.white : Color.clear);
                    }
                _dot.Apply();
            }
            return _dot;
        }
    }
}
