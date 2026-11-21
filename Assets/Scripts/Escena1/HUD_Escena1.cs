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

        GUIStyle _titol;
        GUIStyle _subtitol;
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

            // Titol
            if (logotip != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(34f * k, 18f * k, 300f * k, 166f * k), logotip, ScaleMode.ScaleToFit, true);
                //GUI.color = new Color(0.30f, 0.19f, 0.10f, 0.9f);
                //GUI.Label(new Rect(38f * k, 186f * k, 1200f * k, 50f * k), "Escena 1 · El bol de la massa", _subtitol);
            }
            else
            {
                GUI.color = new Color(0.29f, 0.18f, 0.09f, 0.92f);
                GUI.Label(new Rect(48f * k, 26f * k, 1200f * k, 80f * k), "XUIXO 2026", _titol);

                //GUI.color = new Color(0.36f, 0.26f, 0.16f, 0.85f);
                //GUI.Label(new Rect(50f * k, 100f * k, 1200f * k, 50f * k), "Escena 1 · El bol de la massa", _subtitol);
            }

            // Consell segons l'estat (amb banda fosca per llegir-se sobre la fusta)
            string consell = TextConsell();
            GUI.color = new Color(0.07f, 0.05f, 0.03f, 0.42f);
            GUI.DrawTexture(new Rect(0f, Screen.height - 790f * k, Screen.width, 64f * k), _blanc);

            GUI.color = new Color(0.99f, 0.95f, 0.87f, 0.98f);
            GUI.Label(new Rect(0f, Screen.height - 780f * k, Screen.width, 80f * k), consell, _consell);

            //// Icones de les fletxes al costat del consell
            //if (iconaEsquerra != null && iconaDreta != null && !string.IsNullOrEmpty(consell) &&
            //    masa.EstatActual == MasaJugador.Estat.DinsBol)
            //{
            //    float midax = _consell.CalcSize(new GUIContent(consell)).x;
            //    float ix = (Screen.width - midax) * 0.5f;
            //    float iy = Screen.height - 760f * k;
            //    GUI.color = Color.white;
            //    GUI.DrawTexture(new Rect(ix - 48f * k, iy, 34f * k, 34f * k), iconaEsquerra, ScaleMode.ScaleToFit, true);
            //    GUI.DrawTexture(new Rect(ix + midax + 14f * k, iy, 34f * k, 34f * k), iconaDreta, ScaleMode.ScaleToFit, true);
            //}

            // Indicador d'escena (1 de 6)
            if (indicadorActiu != null && indicadorInactiu != null)
            {
                const int total = 6;
                float d = 22f * k;
                float sep = 10f * k;
                float ample = total * d + (total - 1) * sep;
                float x0 = Screen.width - ample - 36f * k;
                float y0 = 30f * k;
                GUI.color = Color.white;
                for (int i = 0; i < total; i++)
                    GUI.DrawTexture(new Rect(x0 + i * (d + sep), y0, d, d), i == 0 ? indicadorActiu : indicadorInactiu, ScaleMode.ScaleToFit, true);
            }

            // Barra d'impuls
            if (!masa.HaSortit)
            {
                float bw = 560f * k;
                float bh = 48f * k;
                float bx = (Screen.width - bw) * 0.5f;
                float by = Screen.height - 650f * k;

                if (barraMarc != null && barraFarciment != null)
                {
                    GUI.color = Color.white;
                    GUI.DrawTexture(new Rect(bx, by, bw, bh), barraMarc, ScaleMode.StretchToFill, true);

                    float fx = bx + 12f * k;
                    float fy = by + 12f * k;
                    float fw = 536f * k;
                    float fh = 24f * k;
                    float progres = Mathf.Clamp01(masa.Carrega);

                    GUI.BeginGroup(new Rect(fx, fy, fw * progres, fh));
                    GUI.DrawTexture(new Rect(0f, 0f, fw, fh), barraFarciment, ScaleMode.StretchToFill, true);
                    GUI.EndGroup();
                }
                else
                {
                    float bhSimple = 28f * k;
                    float bySimple = Screen.height - 370f * k;

                    GUI.color = new Color(0.29f, 0.18f, 0.09f, 0.22f);
                    GUI.DrawTexture(new Rect(bx - 5f * k, bySimple - 5f * k, 520f * k + 10f * k, bhSimple + 10f * k), _blanc);

                    GUI.color = new Color(0.99f, 0.96f, 0.90f, 0.9f);
                    GUI.DrawTexture(new Rect(bx, bySimple, 520f * k, bhSimple), _blanc);

                    GUI.color = new Color(0.93f, 0.55f, 0.18f, 1f);
                    GUI.DrawTexture(new Rect(bx, bySimple, 520f * k * Mathf.Clamp01(masa.Carrega), bhSimple), _blanc);
                }

                GUI.color = Color.black;
                GUI.Label(new Rect(bx, by - 40f * k, bw, 36f * k), "NIVELL DE IMPULS", _mitjaNegreta);

                // Botó premut just a la dreta de la barra
                Texture2D boto = botoPremut != null ? botoPremut : botoNormal;
                if (boto != null)
                {
                    float ibw = 56f * k;
                    float ibh = 30f * k;
                    GUI.color = Color.white;
                    GUI.DrawTexture(new Rect(bx + bw + 12f * k, by + 6f * k, ibw, ibh), boto, ScaleMode.ScaleToFit, true);
                }
            }

            // Fos final
            if (final != null && final.Opacitat > 0f)
            {
                GUI.color = new Color(0.06f, 0.05f, 0.04f, final.Opacitat);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _blanc);
            }

            // Pantalla final (quan encara no hi ha escena 2)
            if (final != null && final.Acabat)
            {
                float aw = 1040f * k;
                float ah = 460f * k;
                float ax = (Screen.width - aw) * 0.5f;
                float ay = (Screen.height - ah) * 0.5f;

                if (panell != null)
                    Dibuixa9Tall(panell, new Rect(ax, ay, aw, ah), 22f * k);
                else
                {
                    GUI.color = new Color(0.98f, 0.94f, 0.86f, 0.97f);
                    GUI.DrawTexture(new Rect(ax, ay, aw, ah), _blanc);
                }

                GUI.color = new Color(0.29f, 0.18f, 0.09f, 1f);
                GUI.Label(new Rect(ax, ay + 60f * k, aw, 90f * k), "ESCENA 1 COMPLETADA", _gran);

                GUI.color = new Color(0.36f, 0.26f, 0.16f, 1f);
                GUI.Label(new Rect(ax + 60f * k, ay + 175f * k, aw - 120f * k, 130f * k),
                    "La massa ha arribat al corró del cuiner.\nAra toca estirar-la...", _mitja);

                GUI.color = new Color(0.45f, 0.35f, 0.24f, 1f);
                GUI.Label(new Rect(ax + 60f * k, ay + 315f * k, aw - 120f * k, 60f * k),
                    "Propera pantalla: Escena 2 (a carrec d'un altre company)", _petit);

                GUI.color = new Color(0.93f, 0.55f, 0.18f, 1f);
                GUI.Label(new Rect(ax, ay + ah - 80f * k, aw, 60f * k), "Prem R per tornar-ho a jugar", _petit);
            }
        }

        /// <summary>Dibuixa una textura en 9 talls (cantonades fixes) amb GUI immediat.</summary>
        void Dibuixa9Tall(Texture2D textura, Rect r, float marge)
        {
            GUI.color = Color.white;

            float tw = textura.width;
            float th = textura.height;
            marge = Mathf.Clamp(marge, 1f, Mathf.Min(r.width, r.height) * 0.45f);

            // Proporcions del marge dins la textura
            float u = marge / tw;
            float v = marge / th;

            float wMitja = r.width - marge * 2f;
            float hMitja = r.height - marge * 2f;

            // Cantonades
            GUI.DrawTextureWithTexCoords(new Rect(r.x, r.y, marge, marge), textura, new Rect(0f, 1f - v, u, v));
            GUI.DrawTextureWithTexCoords(new Rect(r.x + r.width - marge, r.y, marge, marge), textura, new Rect(1f - u, 1f - v, u, v));
            GUI.DrawTextureWithTexCoords(new Rect(r.x, r.y + r.height - marge, marge, marge), textura, new Rect(0f, 0f, u, v));
            GUI.DrawTextureWithTexCoords(new Rect(r.x + r.width - marge, r.y + r.height - marge, marge, marge), textura, new Rect(1f - u, 0f, u, v));

            // Costats
            GUI.DrawTextureWithTexCoords(new Rect(r.x + marge, r.y, wMitja, marge), textura, new Rect(u, 1f - v, 1f - u * 2f, v));
            GUI.DrawTextureWithTexCoords(new Rect(r.x + marge, r.y + r.height - marge, wMitja, marge), textura, new Rect(u, 0f, 1f - u * 2f, v));
            GUI.DrawTextureWithTexCoords(new Rect(r.x, r.y + marge, marge, hMitja), textura, new Rect(0f, v, u, 1f - v * 2f));
            GUI.DrawTextureWithTexCoords(new Rect(r.x + r.width - marge, r.y + marge, marge, hMitja), textura, new Rect(1f - u, v, u, 1f - v * 2f));

            // Centre
            GUI.DrawTextureWithTexCoords(new Rect(r.x + marge, r.y + marge, wMitja, hMitja), textura, new Rect(u, v, 1f - u * 2f, 1f - v * 2f));
        }

        string TextConsell()
        {
            switch (masa.EstatActual)
            {
                case MasaJugador.Estat.DinsBol:
                    if (masa.Carrega > 0.7f)
                        return "Ja casi hi ets! Segueix remenant";
                    return "Mou la massa amb les fletxes ← → per sortir del bol";
                case MasaJugador.Estat.Sortint:
                    return "Fora del bol! Sempre surts cap a la dreta";
                case MasaJugador.Estat.Carrera:
                    return "Endavant! Arriba fins al corró del cuiner";
                default:
                    return "";
            }
        }

        void PreparaEstils(float k)
        {
            if (_titol == null)
            {
                _titol = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft, fontStyle = FontStyle.Bold };
                _subtitol = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft, fontStyle = FontStyle.Italic };
                _consell = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                _petit = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                _gran = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                _mitja = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                _mitjaNegreta = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
            }

            _titol.fontSize = Mathf.RoundToInt(58f * k);
            _subtitol.fontSize = Mathf.RoundToInt(26f * k);
            _consell.fontSize = Mathf.RoundToInt(34f * k);
            _petit.fontSize = Mathf.RoundToInt(22f * k);
            _gran.fontSize = Mathf.RoundToInt(46f * k);
            _mitja.fontSize = Mathf.RoundToInt(28f * k);
            _mitjaNegreta = new GUIStyle(_mitja) { fontStyle = FontStyle.Bold };
        }
    }
}
