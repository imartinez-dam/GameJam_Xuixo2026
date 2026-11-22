using UnityEngine;
using UnityEngine.SceneManagement;
using Xuixo.Escena1;
using Xuixo.UI;

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
            CabeceraEsquena capcalera = _instancia.AddComponent<CabeceraEsquena>();
            TemaPastisseria tema = TemaPastisseria.Carrega();
            if (tema != null)
            {
                capcalera.logotipTema = tema.logotip;
                capcalera.indicadorActiuTema = tema.indicadorActiu;
                capcalera.indicadorInactiuTema = tema.indicadorInactiu;
            }
            Object.DontDestroyOnLoad(_instancia);
        }

        private Texture2D logotipTema;
        private Texture2D indicadorActiuTema;
        private Texture2D indicadorInactiuTema;

        private void OnGUI()
        {
            float k = Mathf.Max(0.65f, Screen.height / 1080f);

            // Titol: el logotip de l'Escena 1, sinó el logotip del tema compartit,
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
                logo = logotipTema;

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

            Texture2D dotActiu = indicadorActiu != null ? indicadorActiu : indicadorActiuTema;
            Texture2D dotInactiu = indicadorInactiu != null ? indicadorInactiu : indicadorInactiuTema;

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
