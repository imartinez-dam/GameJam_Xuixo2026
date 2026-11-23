using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using Xuixo.UI;

namespace Xuixo
{
    [DefaultExecutionOrder(-1000)]
    public sealed class SortidaJoc : MonoBehaviour
    {
        private static SortidaJoc instancia;
        [SerializeField] private GameObject dialeg;
        [SerializeField] private Button continua;
        [SerializeField] private Button surt;
        private EventSystem events;
        private GameObject seleccioAnterior;
        private VideoPlayer videoPausat;
        private float escalaAnterior;
        private bool audioPausat;
        private int frameTancat = -1;

        internal static bool BloquejaControls => instancia != null
            && (instancia.dialeg.activeSelf || instancia.frameTancat == Time.frameCount);

        private void Awake()
        {
            instancia = this;
            continua.onClick.AddListener(Continua);
            surt.onClick.AddListener(Surt);
        }

        private void OnDestroy()
        {
            if (dialeg != null && dialeg.activeSelf)
                RestauraJoc();
            if (instancia == this)
                instancia = null;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (dialeg.activeSelf)
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

            events = EventSystem.current;
            seleccioAnterior = events != null ? events.currentSelectedGameObject : null;
            dialeg.SetActive(true);
            if (events != null)
                events.SetSelectedGameObject(continua.gameObject);
        }

#if UNITY_EDITOR
        private void AuthorDialog(Sprite pergami, Sprite boto)
        {
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

            Image ombra = Imatge("Ombra", dialeg.transform, null, new Color(0f, 0f, 0f, 0.62f));
            RectTransform fons = ombra.rectTransform;
            fons.anchorMin = Vector2.zero;
            fons.anchorMax = Vector2.one;
            fons.offsetMin = fons.offsetMax = Vector2.zero;

            Image panell = Imatge("Pergamí", fons, pergami, Color.white);
            panell.rectTransform.sizeDelta = new Vector2(408f, 180f);
            TextCentrat(panell.transform, "Vols sortir del joc?", new Vector2(0f, 36f), new Vector2(380f, 54f), 28);
            continua = CreaBoto(panell.transform, boto, "Continua", -90f);
            surt = CreaBoto(panell.transform, boto, "Surt", 90f);

            continua.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnRight = surt };
            surt.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = continua };
            dialeg.SetActive(false);
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

        private static Button CreaBoto(Transform pare, Sprite sprite, string nom, float x)
        {
            Image imatge = Imatge(nom, pare, sprite, Color.white);
            imatge.rectTransform.anchoredPosition = new Vector2(x, -36f);
            imatge.rectTransform.sizeDelta = new Vector2(156f, 56f);
            Button boto = imatge.gameObject.AddComponent<Button>();
            boto.targetGraphic = imatge;
            ColorBlock colors = boto.colors;
            colors.selectedColor = colors.highlightedColor = EstilPastisseria.CaramelRessaltat;
            boto.colors = colors;
            TextCentrat(boto.transform, nom, Vector2.zero, new Vector2(140f, 46f), 23);
            return boto;
        }
#endif

        private void Continua()
        {
            frameTancat = Time.frameCount;
            RestauraJoc();
            dialeg.SetActive(false);
            if (events != null && events.isActiveAndEnabled)
                events.SetSelectedGameObject(seleccioAnterior);
        }

        private void RestauraJoc()
        {
            Time.timeScale = escalaAnterior;
            AudioListener.pause = audioPausat;
            if (videoPausat != null)
                videoPausat.Play();
            videoPausat = null;

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
