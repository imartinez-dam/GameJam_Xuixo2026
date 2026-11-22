using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ObradorIntroController : MonoBehaviour
{
    [Header("Referències")]
    [SerializeField] private CanvasGroup posterGroup;
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private Button playButton;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData introDialogue;

    [Header("Configuració")]
    [SerializeField] private string nextSceneName = "Escena1_Bol";
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.5f;

    private bool _started;

    private void OnEnable()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        dialogueManager.Finished += OnDialogueFinished;
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveListener(OnPlayClicked);
        dialogueManager.Finished -= OnDialogueFinished;
    }

    private void Update()
    {
        if (_started) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
        {
            OnPlayClicked();
        }
    }

    private void OnPlayClicked()
    {
        if (_started) return;
        _started = true;

        Xuixo.GameManager.BeginRun();
        playButton.interactable = false;
        StartCoroutine(PlayIntroRoutine());
    }

    private IEnumerator PlayIntroRoutine()
    {
        yield return Fade(posterGroup, 1f, 0f);
        posterGroup.gameObject.SetActive(false);
        yield return null; // evita que la mateixa pulsació avanci el diàleg
        dialogueManager.Play(introDialogue);
    }

    private void OnDialogueFinished() => StartCoroutine(LoadNextRoutine());

    private IEnumerator LoadNextRoutine()
    {
        yield return Fade(fadeOverlay, 0f, 1f);

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogWarning($"L'escena '{nextSceneName}' no és a la Scene List de Build Profiles.", this);
            yield return Fade(fadeOverlay, 1f, 0f);
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        group.alpha = to;
    }
}