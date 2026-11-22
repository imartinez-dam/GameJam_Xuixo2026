using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Xuixo.UI
{
    /// <summary>
    /// Barra d'instruccions sempre visible a dalt de cada escena, amb el kit
    /// d'UI de l'Escena 1 (Art/UI): panell de pergami i xips amb el boto de
    /// fusta per a cada tecla. Es construeix sola en carregar cada escena
    /// (no cal tocar els fitxers .unity) i aplica el tema a tots els botons
    /// Unity existents perque comparteixin la mateixa tematica.
    /// </summary>
    public static class InstruccionsUI
    {
        private struct Pas
        {
            public readonly string Tecla;
            public readonly string Accio;
            public readonly string Icona;

            public Pas(string tecla, string accio, string icona = null)
            {
                Tecla = tecla;
                Accio = accio;
                Icona = icona;
            }
        }

        private static readonly Dictionary<string, Pas[]> PerEscena = new Dictionary<string, Pas[]>
        {
            { "ObradorIntro", new[]
                {
                    new Pas("SOM-HI!", "Comença la partida"),
                    new Pas("ESPAI / CLIC", "Avança el diàleg"),
                } },

            { "Escena1_Bol", new[]
                {
                    new Pas("Mou la massa cap a l'esquerra i cap a la dreta", ""),
                    new Pas("←", ""),
                    new Pas("→", ""),
                } },

            // Escena1_Bol uses the pixel-art cue above the bowl instead.

            { "Estirar", new[]
                {
                    new Pas("ESPAI", "Engega la barra i atura-la al punt just per estirar la massa (3 intents)"),
                } },
            { "Crema", new[]
                {
                    new Pas("ESPAI", "Deixa caure la crema just a sobre del xuixo (3 intents)"),
                } },
            { "EnrotllarMassa", new[]
                {
                    new Pas("BOTÓ ESQUERRE", "Mantén premut a l'inici i ressegueix la massa fins al centre",
                        "UI/LeftClickMouse"),
                } },
            // FryingFinale has its own live prompts for both timing rounds.
        };

        private static GameObject _instancia;
        private static TMP_FontAsset _font;
        private static TemaPastisseria _tema;

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

            if (_tema == null)
                _tema = TemaPastisseria.Carrega();

            AplicaEstilBotons();

            if (PerEscena.TryGetValue(nomEscena, out Pas[] passos))
                _instancia = CreaBarra(passos);
        }

        private static Sprite FonsPanell =>
            _tema != null ? _tema.SpritePanell : EstilPastisseria.Arrodonit;

        private static Sprite FonsXip =>
            _tema != null ? _tema.SpriteBotoNormal : EstilPastisseria.Arrodonit;

        private static Color ColorXip =>
            _tema != null ? Color.white : TemaPastisseria.ColorCaramel;

        /// <summary>Aplica el tema de pastisseria a tots els botons Unity de l'escena.</summary>
        private static void AplicaEstilBotons()
        {
            foreach (Button boto in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                ColorBlock colors = boto.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
                colors.disabledColor = new Color(1f, 1f, 1f, 0.5f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.06f;
                boto.colors = colors;

                if (boto.targetGraphic is Image imatge)
                {
                    imatge.sprite = FonsXip;
                    imatge.type = Image.Type.Sliced;
                    imatge.color = ColorXip;
                }

                foreach (TMP_Text textTmp in boto.GetComponentsInChildren<TMP_Text>(true))
                    FesMesGruixut(textTmp);

                foreach (Text textLegacy in boto.GetComponentsInChildren<Text>(true))
                {
                    textLegacy.fontStyle = FontStyle.Bold;
                    if (textLegacy.GetComponent<Outline>() == null)
                    {
                        Outline contorn = textLegacy.gameObject.AddComponent<Outline>();
                        contorn.effectColor = textLegacy.color;
                        contorn.effectDistance = new Vector2(1f, -1f);
                    }
                }

                if (_tema != null)
                {
                    boto.transition = Selectable.Transition.SpriteSwap;
                    SpriteState estat = new SpriteState
                    {
                        highlightedSprite = _tema.SpriteBotoHover,
                        pressedSprite = _tema.SpriteBotoPremut,
                        selectedSprite = _tema.SpriteBotoHover
                    };
                    boto.spriteState = estat;
                }
            }
        }

        private static void FesMesGruixut(TMP_Text tmp)
        {
            tmp.fontStyle |= FontStyles.Bold;
            tmp.fontWeight = FontWeight.Black;
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.25f);
        }

        private static GameObject CreaBarra(Pas[] passos)
        {
            GameObject arrel = new GameObject("InstruccionsUI", typeof(RectTransform));

            Canvas canvas = arrel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler escalador = arrel.AddComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1920f, 1080f);
            escalador.matchWidthOrHeight = 0.5f;

            // Panell de pergami del kit d'UI
            GameObject panell = CreaFill("Panell", arrel.transform);
            Image fons = panell.AddComponent<Image>();
            fons.sprite = FonsPanell;
            fons.type = Image.Type.Sliced;
            fons.color = Color.white;
            fons.raycastTarget = false;

            RectTransform rt = (RectTransform)panell.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -14f);
            rt.sizeDelta = new Vector2(0f, 84f);

            HorizontalLayoutGroup grup = panell.AddComponent<HorizontalLayoutGroup>();
            grup.padding = new RectOffset(36, 36, 0, 0);
            grup.spacing = 26f;
            grup.childAlignment = TextAnchor.MiddleCenter;
            grup.childControlWidth = false;
            grup.childControlHeight = true;
            grup.childForceExpandWidth = false;
            grup.childForceExpandHeight = false;

            ContentSizeFitter ajust = panell.AddComponent<ContentSizeFitter>();
            ajust.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (Pas pas in passos)
                CreaPas(rt, pas);

            return arrel;
        }

        private static void CreaPas(Transform pare, Pas pas)
        {
            GameObject fila = CreaFill("Pas", pare);

            HorizontalLayoutGroup grupFila = fila.AddComponent<HorizontalLayoutGroup>();
            grupFila.spacing = 12f;
            grupFila.childAlignment = TextAnchor.MiddleCenter;
            grupFila.childControlWidth = true;
            grupFila.childControlHeight = true;
            grupFila.childForceExpandWidth = false;
            grupFila.childForceExpandHeight = false;

            ContentSizeFitter ajustFila = fila.AddComponent<ContentSizeFitter>();
            ajustFila.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            Sprite sprite = string.IsNullOrEmpty(pas.Icona) ? null : Resources.Load<Sprite>(pas.Icona);
            if (sprite != null)
            {
                GameObject icona = CreaFill("IconaTecla", fila.transform);
                Image imatge = icona.AddComponent<Image>();
                imatge.sprite = sprite;
                imatge.preserveAspect = true;
                imatge.raycastTarget = false;

                LayoutElement mida = icona.AddComponent<LayoutElement>();
                mida.preferredWidth = 88f;
                mida.preferredHeight = 72f;
                CreaText(fila.transform, "Accio", pas.Accio, 26f, TemaPastisseria.ColorTextFosc, false);
                return;
            }

            // Xip amb el boto de fusta del kit (la tecla)
            GameObject xip = CreaFill("Xip", fila.transform);
            Image fonsXip = xip.AddComponent<Image>();
            fonsXip.sprite = FonsXip;
            fonsXip.type = Image.Type.Sliced;
            fonsXip.color = ColorXip;
            fonsXip.raycastTarget = false;

            LayoutElement midaXip = xip.AddComponent<LayoutElement>();
            midaXip.preferredHeight = 48f;

            HorizontalLayoutGroup grupXip = xip.AddComponent<HorizontalLayoutGroup>();
            grupXip.padding = new RectOffset(20, 20, 0, 0);
            grupXip.childAlignment = TextAnchor.MiddleCenter;
            grupXip.childControlWidth = true;
            grupXip.childControlHeight = true;
            grupXip.childForceExpandWidth = false;
            grupXip.childForceExpandHeight = false;

            ContentSizeFitter ajustXip = xip.AddComponent<ContentSizeFitter>();
            ajustXip.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreaText(xip.transform, "Tecla", pas.Tecla, 24f, new Color32(26, 16, 10, 255), true);
            CreaText(fila.transform, "Accio", pas.Accio, 26f, TemaPastisseria.ColorTextFosc, false);
        }

        private static void CreaText(Transform pare, string nom, string contingut, float mida, Color color, bool negreta)
        {
            GameObject go = CreaFill(nom, pare);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = contingut;
            tmp.fontSize = mida;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;

            TMP_FontAsset font = TrobaFont();
            if (font != null)
                tmp.font = font;

            if (negreta)
                FesMesGruixut(tmp);
            else
                tmp.fontStyle = FontStyles.Normal;
        }

        private static TMP_FontAsset TrobaFont()
        {
            if (_font == null)
            {
                _font = TMP_Settings.defaultFontAsset;
                if (_font == null)
                    _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
            return _font;
        }

        private static GameObject CreaFill(string nom, Transform pare)
        {
            GameObject go = new GameObject(nom, typeof(RectTransform));
            go.transform.SetParent(pare, false);
            return go;
        }
    }
}
