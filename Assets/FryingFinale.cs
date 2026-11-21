using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Xuixo;

public sealed class FryingFinale : MonoBehaviour
{
    [Header("Fryer layout")]
    [SerializeField] private RectTransform canvas;
    [SerializeField] private RectTransform trayStart;
    [SerializeField] private RectTransform trayEdge;
    [SerializeField] private RectTransform oilSurface;
    [SerializeField] private RectTransform oilMask;
    [SerializeField] private RawImage pastry;

    [Header("Frying UI")]
    [SerializeField] private Text prompt;
    [SerializeField] private Text hint;
    [SerializeField] private RectTransform meter;
    [SerializeField] private Text meterLabel;
    [SerializeField] private Image fill;
    [SerializeField] private RectTransform targetMarks;
    [SerializeField] private RectTransform lowerTick;
    [SerializeField] private RectTransform upperTick;
    [SerializeField] private RectTransform colorSwatch;

    [Header("Pastry textures")]
    [SerializeField] private Texture2D friedXuixo;
    [SerializeField] private Texture2D sugaredXuixo;
    [SerializeField] private Texture2D wonkyXuixo;
    [SerializeField] private Texture2D extraWonkyXuixo;

    [Header("Hidden score")]
    [SerializeField, Min(0)] private int fryingPoints = 20;
    [SerializeField, Min(1)] private int wonkyMinScore = 40;
    [SerializeField, Min(2)] private int wellMadeMinScore = 70;
    [Tooltip("Earlier-stage score used only when testing this scene directly in the Editor.")]
    [SerializeField, Min(0)] private int standaloneScore = 55;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float heatSeconds = 4.4f;
    [SerializeField, Min(0.1f)] private float handsOffSeconds = 2f;
    [SerializeField, Range(0.05f, 0.85f)] private float targetStart = 0.56f;
    [SerializeField, Range(0.1f, 0.95f)] private float targetEnd = 0.68f;

    [Header("Ending hooks")]
    [SerializeField] private UnityEvent onXuixoEnding = new UnityEvent();
    [SerializeField] private UnityEvent onWonkyEnding = new UnityEvent();
    [SerializeField] private UnityEvent onExtraWonkyEnding = new UnityEvent();

    // PNG pixels (x=640, y=700 measured from the top) from the raw and fried sprites.
    private static readonly Color32 RawColor = new Color32(247, 211, 174, 255);
    private static readonly Color32 FriedColor = new Color32(246, 167, 65, 255);
    private static readonly Color32 BurntColor = new Color32(45, 29, 27, 255);
    private static readonly Rect RollCrop = new Rect(0.055f, 0.31f, 0.89f, 0.29f);
    private static readonly Rect WonkyCrop = new Rect(0.055f, 0.27f, 0.89f, 0.40f);
    private const float RevealHeight = 112f;
    private const float RevealWidth = 320f;
    private const float OilEntryX = -140f;

    private enum Stage { Ready, Roll, Drop, Sink, Flip, HandsOff, Bubbling, Pop, Sugar, Finished }
    private enum Outcome { ExtraWonky, Wonky, WellMade }

    private Stage stage;
    private float elapsed;
    private Outcome outcome;
    private float pastryHeight;
    private float heat;
    private float TargetEnd => Mathf.Max(targetStart + 0.01f, targetEnd);

    private readonly Image[] bubbles = new Image[7];
    private float bubbleTime;

    private void Start()
    {
        if (!canvas || !trayStart || !trayEdge || !oilSurface || !oilMask || !pastry || !pastry.texture
            || !friedXuixo || !sugaredXuixo || !wonkyXuixo || !extraWonkyXuixo
            || !prompt || !hint || !meter || !meterLabel || !fill || !targetMarks
            || !lowerTick || !upperTick || !colorSwatch)
        {
            Debug.LogError("FryingFinale needs its scene layout, UI and pastry textures assigned.", this);
            enabled = false;
            return;
        }

        SetVerticalAnchor(lowerTick, targetStart);
        SetVerticalAnchor(upperTick, TargetEnd);
        SetVerticalAnchor(colorSwatch, (targetStart + TargetEnd) * 0.5f);
        targetMarks.gameObject.SetActive(true);
        // The meter is visible in edit mode for layout work, but not until frying during play.
        meter.gameObject.SetActive(false);
        CreateBubbles();
        pastryHeight = pastry.rectTransform.rect.height;
        stage = Stage.Ready;
    }

    private void Update()
    {
        bool pressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        elapsed += Time.deltaTime;
        AnimateBubbles(stage == Stage.Flip || stage == Stage.HandsOff || stage == Stage.Bubbling);

        // A transition consumes this frame's press; it can't affect the next stage too.
        switch (stage)
        {
            case Stage.Ready:
                if (pressed) SetStage(Stage.Roll, "Into the oil…", "");
                break;
            case Stage.Roll:
                float roll = Mathf.Clamp01(elapsed / 0.45f);
                pastry.rectTransform.position = Vector3.Lerp(trayStart.position, trayEdge.position, roll * roll);
                pastry.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -12f * roll);
                if (roll >= 1f) SetStage(Stage.Drop, "Into the oil…", "");
                break;
            case Stage.Drop:
                float drop = Mathf.Clamp01(elapsed / 0.30f);
                Vector3 entry = oilMask.TransformPoint(new Vector3(OilEntryX, 0f, 0f));
                pastry.rectTransform.position = Vector3.Lerp(trayEdge.position, entry, drop)
                    + canvas.TransformVector(Vector3.up * (16f * Mathf.Sin(drop * Mathf.PI)));
                pastry.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -12f * (1f - drop));
                if (drop >= 1f)
                {
                    // Clip at the oil line only after leaving the tray.
                    RectTransform art = pastry.rectTransform;
                    art.SetParent(oilMask, false);
                    art.anchorMin = art.anchorMax = new Vector2(0.5f, 0f);
                    art.localRotation = Quaternion.identity;
                    art.anchoredPosition = new Vector2(OilEntryX, 0f);
                    SetStage(Stage.Sink, "Into the oil…", "");
                }
                break;
            case Stage.Sink:
                float sink = Mathf.Clamp01(elapsed / 0.22f);
                pastry.rectTransform.anchoredPosition = new Vector2(OilEntryX, -pastryHeight * sink);
                if (sink >= 1f)
                {
                    pastry.enabled = false;
                    meter.gameObject.SetActive(true);
                    SetFill(0f, RawColor);
                    SetStage(Stage.Flip, "Flip in the golden zone", "SPACE: Flip once when the fill reaches the marked band");
                }
                break;
            case Stage.Flip:
                // Judge against the fill shown to the player on the previous rendered frame.
                if (pressed && heat <= TargetEnd)
                {
                    if (heat < targetStart)
                        StartReveal(false);
                    else
                    {
                        targetMarks.gameObject.SetActive(false);
                        meterLabel.text = "REST";
                        SetFill(0f, FriedColor);
                        SetStage(Stage.HandsOff, "Hands off!", "Let the second side fry without pressing Space");
                    }
                    break;
                }

                heat = Mathf.Clamp01(elapsed / heatSeconds);
                Color color = heat < targetStart
                    ? Color.Lerp(RawColor, FriedColor, heat / targetStart)
                    : Color.Lerp(FriedColor, BurntColor, Mathf.InverseLerp(TargetEnd, 1f, heat));
                SetFill(heat, color);
                if (heat >= 1f) StartReveal(false);
                break;
            case Stage.HandsOff:
                if (pressed && elapsed < handsOffSeconds) StartReveal(false);
                else if (elapsed >= handsOffSeconds) StartReveal(true);
                else SetFill(elapsed / handsOffSeconds, FriedColor);
                break;
            case Stage.Bubbling:
                if (elapsed >= 0.65f)
                {
                    Texture2D result = outcome == Outcome.WellMade ? friedXuixo
                        : outcome == Outcome.Wonky ? wonkyXuixo : extraWonkyXuixo;
                    SetPastry(result, outcome == Outcome.WellMade ? RollCrop : WonkyCrop, RevealWidth);
                    MovePastry(-pastryHeight);
                    pastry.enabled = true;
                    SetStage(Stage.Pop, "", "");
                }
                break;
            case Stage.Pop:
                float pop = Mathf.Clamp01(elapsed / 0.7f);
                MovePastry(Mathf.Lerp(-pastryHeight, RevealHeight, 1f - Mathf.Pow(1f - pop, 3f))
                    + 38f * Mathf.Sin(pop * Mathf.PI));
                if (pop >= 1f)
                {
                    if (outcome == Outcome.WellMade) SetStage(Stage.Sugar, "A xuixo!", "SPACE: Add sugar");
                    else
                    {
                        bool wonky = outcome == Outcome.Wonky;
                        SetStage(Stage.Finished, wonky ? "A wonky xuixo…" : "An extra-wonky xuixo!", "The end");
                        if (wonky) onWonkyEnding.Invoke();
                        else onExtraWonkyEnding.Invoke();
                    }
                }
                break;
            case Stage.Sugar:
                if (pressed)
                {
                    SetPastry(sugaredXuixo, RollCrop, RevealWidth);
                    SetStage(Stage.Finished, "A perfect xuixo!", "The end");
                    onXuixoEnding.Invoke();
                }
                break;
        }
    }

    private void StartReveal(bool friedSuccessfully)
    {
        meter.gameObject.SetActive(false);
        int points = friedSuccessfully ? fryingPoints : 0;
        GameManager manager = GameManager.Instance;
        int total;
        if (manager != null && manager.HasScore(GameManager.BowlStage))
        {
            manager.TryAddScore(GameManager.FryingStage, points);
            total = manager.TotalScore;
        }
#if UNITY_EDITOR
        else if (manager == null)
        {
            total = standaloneScore + points;
        }
#endif
        else
        {
            Debug.LogError("FryingFinale reached without a bowl score. Start a run from ObradorIntro.", this);
            SetStage(Stage.Finished, "Could not finish this run", "Start a new run from the intro");
            return;
        }

        outcome = total >= wellMadeMinScore ? Outcome.WellMade
            : total >= wonkyMinScore ? Outcome.Wonky : Outcome.ExtraWonky;
        SetStage(Stage.Bubbling, "What came out of the fryer?", "");
    }

    private void OnValidate()
    {
        wonkyMinScore = Mathf.Clamp(wonkyMinScore, 1, int.MaxValue - 1);
        wellMadeMinScore = Mathf.Max(wonkyMinScore + 1, wellMadeMinScore);
    }

    private void SetStage(Stage next, string title, string instructions)
    {
        stage = next;
        elapsed = 0f;
        prompt.text = title;
        hint.text = instructions;
    }

    private void SetFill(float amount, Color color)
    {
        RectTransform rect = fill.rectTransform;
        rect.anchorMax = new Vector2(rect.anchorMax.x, amount);
        fill.color = color;
    }

    private static void SetVerticalAnchor(RectTransform rect, float fraction)
    {
        rect.anchorMin = new Vector2(rect.anchorMin.x, fraction);
        rect.anchorMax = new Vector2(rect.anchorMax.x, fraction);
    }

    private void SetPastry(Texture2D texture, Rect crop, float width)
    {
        pastry.texture = texture;
        pastry.uvRect = crop;
        pastryHeight = width * texture.height * crop.height / (texture.width * crop.width);
        pastry.rectTransform.sizeDelta = new Vector2(width, pastryHeight);
    }

    private void MovePastry(float y) => pastry.rectTransform.anchoredPosition = new Vector2(0f, y);

    private void AnimateBubbles(bool visible)
    {
        if (visible) bubbleTime += Time.deltaTime;
        for (int i = 0; i < bubbles.Length; i++)
        {
            Image bubble = bubbles[i];
            bubble.enabled = visible;
            if (!visible) continue;

            float cycle = Mathf.Repeat(bubbleTime * (0.9f + i * 0.065f) + i * 0.17f, 1f);
            bubble.rectTransform.anchoredPosition = new Vector2(-138f + 46f * i, 2f + cycle * 32f);
            bubble.rectTransform.localScale = Vector3.one * (0.55f + cycle * 0.65f);
            bubble.color = new Color(1f, 0.77f, 0.42f, 1f - cycle);
        }
    }

    private void CreateBubbles()
    {
        for (int i = 0; i < bubbles.Length; i++)
        {
            GameObject bubble = new GameObject("Bubble", typeof(RectTransform));
            RectTransform rect = bubble.GetComponent<RectTransform>();
            rect.SetParent(oilSurface, false);
            rect.sizeDelta = Vector2.one * (6f + i % 3);
            Image image = bubble.AddComponent<Image>();
            image.raycastTarget = false;
            image.enabled = false;
            bubbles[i] = image;
        }
    }
}
