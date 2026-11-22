using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Xuixo.UI
{
    [RequireComponent(typeof(VideoPlayer))]
    public sealed class CreditsControls : MonoBehaviour
    {
        private VideoPlayer video;
        private bool creditsFinished;
        private bool leaving;

        private void Start()
        {
            video.Play();
        }
        private void Awake()
        {
            video = GetComponent<VideoPlayer>();
            video.source = VideoSource.Url; 
            video.url = Application.streamingAssetsPath + "/Videos/videofinal.mp4";
        }
        private void OnEnable() => video.loopPointReached += ShowControls;
        private void OnDisable() => video.loopPointReached -= ShowControls;

        //string url = "Application.streamingAssetsPath + \"/Videos/videofinal.mp4\";
        private void Update()
        {
            if (!creditsFinished || leaving || Keyboard.current == null)
                return;

            if (Keyboard.current.rKey.wasPressedThisFrame)
                Restart();
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Quit();
        }

        private void ShowControls(VideoPlayer player)
        {
            if (creditsFinished)
                return;

            creditsFinished = true;
            if (EventSystem.current == null)
                new GameObject("Credits EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            GameObject overlay = new GameObject("Credits Controls", typeof(RectTransform),
                typeof(Canvas), typeof(GraphicRaycaster));
            overlay.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject videoArea = new GameObject("Video Area", typeof(RectTransform), typeof(AspectRatioFitter));
            RectTransform area = (RectTransform)videoArea.transform;
            area.SetParent(overlay.transform, false);
            AspectRatioFitter fit = videoArea.GetComponent<AspectRatioFitter>();
            fit.aspectRatio = player.height > 0 ? (float)player.width / player.height : 16f / 9f;
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

            // Bounds dels botons al fotograma final del vídeo (1280 × 720).
            CreateButton(area, "Reiniciar", new Rect(340, 445, 202, 113), Restart);
            CreateButton(area, "Sortir", new Rect(698, 445, 266, 113), Quit);
        }

        private static void CreateButton(RectTransform parent, string name, Rect bounds,
            UnityEngine.Events.UnityAction action)
        {
            GameObject zone = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = (RectTransform)zone.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(bounds.xMin / 1280f, 1f - bounds.yMax / 720f);
            rect.anchorMax = new Vector2(bounds.xMax / 1280f, 1f - bounds.yMin / 720f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            Image image = zone.GetComponent<Image>();
            image.color = Color.clear;
            Button button = zone.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(action);
        }

        private void Restart()
        {
            if (leaving)
                return;

            const string intro = "ObradorIntro";
            if (!Application.CanStreamedLevelBeLoaded(intro))
            {
                Debug.LogError("Afegeix ObradorIntro al perfil de compilació actiu.", this);
                return;
            }

            leaving = true;
            SceneManager.LoadScene(intro);
        }

        private void Quit()
        {
            if (leaving)
                return;

#if UNITY_EDITOR
            Debug.Log("El botó Sortir tanca el joc només a la build.", this);
#else
            leaving = true;
            Application.Quit();
#endif
        }
    }
}
