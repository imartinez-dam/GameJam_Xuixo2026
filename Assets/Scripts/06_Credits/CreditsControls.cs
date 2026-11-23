using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Xuixo.UI
{
    [RequireComponent(typeof(VideoPlayer))]
    public sealed class CreditsControls : MonoBehaviour
    {
        private VideoPlayer video;
        [SerializeField] private GameObject controls;
        [SerializeField] private AspectRatioFitter videoArea;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
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
            restartButton.onClick.AddListener(Restart);
            quitButton.onClick.AddListener(Quit);
        }
        private void OnEnable() => video.loopPointReached += ShowControls;
        private void OnDisable() => video.loopPointReached -= ShowControls;

        //string url = "Application.streamingAssetsPath + \"/Videos/videofinal.mp4\";
        private void Update()
        {
            if (SortidaJoc.BloquejaControls || !creditsFinished || leaving || Keyboard.current == null)
                return;

            if (Keyboard.current.rKey.wasPressedThisFrame)
                Restart();
        }

        private void ShowControls(VideoPlayer player)
        {
            if (creditsFinished)
                return;

            creditsFinished = true;
            if (player.height > 0)
                videoArea.aspectRatio = (float)player.width / player.height;
            controls.SetActive(true);
        }

#if UNITY_EDITOR
        private void AuthorControls()
        {
            GameObject overlay = new GameObject("Credits Controls", typeof(RectTransform),
                typeof(Canvas), typeof(GraphicRaycaster));
            overlay.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject videoArea = new GameObject("Video Area", typeof(RectTransform), typeof(AspectRatioFitter));
            RectTransform area = (RectTransform)videoArea.transform;
            area.SetParent(overlay.transform, false);
            AspectRatioFitter fit = videoArea.GetComponent<AspectRatioFitter>();
            fit.aspectRatio = 16f / 9f;
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

            // Bounds dels botons al fotograma final del vídeo (1280 × 720).
            restartButton = CreateButton(area, "Reiniciar", new Rect(340, 445, 202, 113));
            quitButton = CreateButton(area, "Sortir", new Rect(698, 445, 266, 113));
            controls = overlay;
            this.videoArea = fit;
            controls.SetActive(false);
        }

        private static Button CreateButton(RectTransform parent, string name, Rect bounds)
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
            return button;
        }
#endif

        private void Restart()
        {
            if (leaving)
                return;

            const string intro = SceneNames.Intro;
            if (!Application.CanStreamedLevelBeLoaded(intro))
            {
                Debug.LogError($"Afegeix {intro} al perfil de compilació actiu.", this);
                return;
            }

            leaving = true;
            SceneManager.LoadScene(intro);
        }

        private void Quit()
        {
            if (leaving)
                return;

            SortidaJoc.DemanaSortir();
        }
    }
}
