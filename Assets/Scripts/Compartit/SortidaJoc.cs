using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.Video;
using Xuixo.UI;

namespace Xuixo
{
    [DefaultExecutionOrder(-1000)]
    internal sealed class SortidaJoc : MonoBehaviour
    {
        private static SortidaJoc instancia;
        private GameObject dialeg;
        private EventSystem events;
        private GameObject seleccioAnterior;
        private VideoPlayer videoPausat;
        private float escalaAnterior;
        private bool audioPausat;
        private int frameTancat = -1;

        internal static bool BloquejaControls => instancia != null
            && (instancia.dialeg != null || instancia.frameTancat == Time.frameCount);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Inicialitza()
        {
            GameObject sortida = new GameObject("SortidaJoc");
            DontDestroyOnLoad(sortida);
            instancia = sortida.AddComponent<SortidaJoc>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (dialeg != null)
                    Continua();
                else
                    ObreDialeg();
            }
        }

        internal static void DemanaSortir()
        {
            if (!BloquejaControls && instancia != null)
                instancia.ObreDialeg();
        }

        internal static IEnumerator Espera(float segons)
        {
            while (segons > 0f || BloquejaControls)
            {
                yield return null;
                if (!BloquejaControls)
                    segons -= Time.unscaledDeltaTime;
            }
        }

        private void ObreDialeg()
        {
            escalaAnterior = Time.timeScale;
            Time.timeScale = 0f;
            audioPausat = AudioListener.pause;
            AudioListener.pause = true;

            foreach (VideoPlayer video in FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None))
            {
                if (!video.isPlaying) continue;
                videoPausat = video;
                video.Pause();
                break;
            }

            dialeg = new GameObject("ConfirmaSortida", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            dialeg.transform.SetParent(transform, false);
            Canvas canvas = dialeg.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            CanvasScaler escalador = dialeg.GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(960f, 540f);
            escalador.matchWidthOrHeight = 0.5f;

            events = EventSystem.current;
            if (events == null)
            {
                GameObject sistema = new GameObject("EventSystemSortida", typeof(EventSystem),
                    typeof(InputSystemUIInputModule));
                sistema.transform.SetParent(dialeg.transform, false);
                events = sistema.GetComponent<EventSystem>();
            }
            seleccioAnterior = events.currentSelectedGameObject;

            Image ombra = Imatge("Ombra", dialeg.transform, null, new Color(0f, 0f, 0f, 0.62f));
            RectTransform fons = ombra.rectTransform;
            fons.anchorMin = Vector2.zero;
            fons.anchorMax = Vector2.one;
            fons.offsetMin = fons.offsetMax = Vector2.zero;

            TemaPastisseria tema = TemaPastisseria.Carrega();
            Sprite pergami = tema != null ? tema.SpritePanell : EstilPastisseria.Arrodonit;
            Image panell = Imatge("Pergamí", fons, pergami, tema != null ? Color.white : EstilPastisseria.Pergami);
            panell.rectTransform.sizeDelta = new Vector2(408f, 180f);
            TextCentrat(panell.transform, "Vols sortir del joc?", new Vector2(0f, 36f), new Vector2(380f, 54f), 28);
            Button continua = CreaBoto(panell.transform, tema, "Continua", -90f, Continua);
            Button surt = CreaBoto(panell.transform, tema, "Surt", 90f, Surt);

            continua.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnRight = surt };
            surt.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = continua };
            events.SetSelectedGameObject(continua.gameObject);
        }

        private static Image Imatge(string nom, Transform pare, Sprite sprite, Color color)
        {
            Image imatge = new GameObject(nom, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            imatge.transform.SetParent(pare, false);
            imatge.sprite = sprite;
            imatge.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            imatge.color = color;
            return imatge;
        }

        private static void TextCentrat(Transform pare, string contingut, Vector2 posicio, Vector2 mida, int lletra)
        {
            Text text = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(pare, false);
            text.rectTransform.anchoredPosition = posicio;
            text.rectTransform.sizeDelta = mida;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = contingut;
            text.fontSize = lletra;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = TemaPastisseria.ColorTextFosc;
            text.raycastTarget = false;
        }

        private static Button CreaBoto(Transform pare, TemaPastisseria tema, string nom, float x,
            UnityEngine.Events.UnityAction accio)
        {
            Sprite sprite = tema != null ? tema.SpriteBotoNormal : EstilPastisseria.Arrodonit;
            Image imatge = Imatge(nom, pare, sprite, Color.white);
            imatge.rectTransform.anchoredPosition = new Vector2(x, -36f);
            imatge.rectTransform.sizeDelta = new Vector2(156f, 56f);
            Button boto = imatge.gameObject.AddComponent<Button>();
            boto.targetGraphic = imatge;
            ColorBlock colors = boto.colors;
            colors.selectedColor = colors.highlightedColor = EstilPastisseria.CaramelRessaltat;
            boto.colors = colors;
            boto.onClick.AddListener(accio);
            TextCentrat(boto.transform, nom, Vector2.zero, new Vector2(140f, 46f), 23);
            return boto;
        }

        private void Continua()
        {
            frameTancat = Time.frameCount;
            Time.timeScale = escalaAnterior;
            AudioListener.pause = audioPausat;
            if (videoPausat != null)
                videoPausat.Play();
            videoPausat = null;

            dialeg.SetActive(false);
            Destroy(dialeg);
            dialeg = null;
            if (events != null && events.isActiveAndEnabled)
                events.SetSelectedGameObject(seleccioAnterior);
        }

        private static void Surt()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
