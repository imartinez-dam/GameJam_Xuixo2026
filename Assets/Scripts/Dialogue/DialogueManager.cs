using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class DialogueBubble
{
    public GameObject root;
    public TMP_Text bodyText;
    public GameObject continueIcon;
}

public class DialogueManager : MonoBehaviour
{
    [Header("Bocadillos")]
    [SerializeField] private DialogueBubble presenterBubble;
    [SerializeField] private DialogueBubble chefBubble;

    [Header("Config")]
    [SerializeField, Min(1f)] private float charsPerSecond = 40f;
    [SerializeField, Min(0.01f)] private float popDuration = 0.12f;

    public event Action Finished;
    public bool IsRunning { get; private set; }

    private DialogueData _data;
    private int _index;
    private DialogueBubble _current;
    private Coroutine _typing;
    private bool _isTyping;

    public void Play(DialogueData data)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Debug.LogWarning("DialogueManager: no hi ha línies per reproduir.", this);
            Finished?.Invoke();
            return;
        }

        _data = data;
        _index = 0;
        IsRunning = true;
        ShowLine();
    }

    private void Update()
    {
        if (Xuixo.SortidaJoc.BloquejaControls) return;
        if (!IsRunning || !AdvancePressed()) return;

        if (_isTyping) CompleteLine();
        else NextLine();
    }

    private static bool AdvancePressed()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        return (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
            || (mouse != null && mouse.leftButton.wasPressedThisFrame)
            || (touch != null && touch.primaryTouch.press.wasPressedThisFrame);
    }

    private void ShowLine()
    {
        var line = _data.lines[_index];

        presenterBubble.root.SetActive(false);
        chefBubble.root.SetActive(false);

        _current = line.speaker == Speaker.Presenter ? presenterBubble : chefBubble;
        _current.root.SetActive(true);
        _current.bodyText.text = line.text;
        _current.bodyText.maxVisibleCharacters = 0;
        _current.continueIcon.SetActive(false);

        StartCoroutine(PopRoutine(_current.root.transform));

        if (_typing != null) StopCoroutine(_typing);
        _typing = StartCoroutine(TypeRoutine());
    }

    private IEnumerator PopRoutine(Transform target)
    {
        float t = 0f;
        while (t < popDuration)
        {
            if (!Xuixo.SortidaJoc.BloquejaControls)
                t += Time.unscaledDeltaTime;
            target.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, t / popDuration);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    private IEnumerator TypeRoutine()
    {
        _isTyping = true;
        _current.bodyText.ForceMeshUpdate();
        int total = _current.bodyText.textInfo.characterCount;
        float visible = 0f;

        while (_current.bodyText.maxVisibleCharacters < total)
        {
            if (!Xuixo.SortidaJoc.BloquejaControls)
                visible += charsPerSecond * Time.unscaledDeltaTime;
            _current.bodyText.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(visible));
            yield return null;
        }

        EndTyping();
    }

    private void CompleteLine()
    {
        if (_typing != null) StopCoroutine(_typing);
        _current.bodyText.maxVisibleCharacters = int.MaxValue;
        EndTyping();
    }

    private void EndTyping()
    {
        _isTyping = false;
        _current.continueIcon.SetActive(true);
    }

    private void NextLine()
    {
        _index++;
        if (_index >= _data.lines.Length)
        {
            IsRunning = false;
            presenterBubble.root.SetActive(false);
            chefBubble.root.SetActive(false);
            Finished?.Invoke();
            return;
        }
        ShowLine();
    }
}
