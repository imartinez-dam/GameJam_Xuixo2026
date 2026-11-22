using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Xuixo;
using Xuixo.UI;

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
    [SerializeField] private Image targetBand;
    [SerializeField] private Color targetHighlight = new Color(1f, 0.8f, 0.3f, 0.45f);
    [SerializeField] private bool useGraphicCues = true;
    [SerializeField] private FryerActionCue graphicCue;

    [Header("Pastry textures")]
    [SerializeField] private Texture2D friedXuixo;
    [SerializeField] private Texture2D sugaredXuixo;
    [SerializeField] private Texture2D wonkyXuixo;
    [SerializeField] private Texture2D extraWonkyXuixo;
    [SerializeField] private Texture2D croissant;

    [Header("Hidden score")]
    [SerializeField, Min(0)] private int fryingPoints = 20;
    [SerializeField, Min(1)] private int wonkyMinScore = 40;
    [SerializeField, Min(2)] private int wellMadeMinScore = 70;
    [Tooltip("Earlier-stage score used only when testing this scene directly in the Editor.")]
    [SerializeField, Min(0)] private int standaloneScore = 55;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float heatSeconds = 4.4f;
    [SerializeField, Range(0.05f, 0.85f)] private float targetStart = 0.56f;
    [SerializeField, Range(0.1f, 0.95f)] private float targetEnd = 0.76f;
    [SerializeField, Range(0.05f, 0.85f)] private float secondTargetStart = 0.15f;
    [SerializeField, Range(0.1f, 0.95f)] private float secondTargetEnd = 0.35f;

    [Header("Ending hooks")]
    [SerializeField, Min(0f)] private float creditsDelay = 3f;
    [SerializeField] private UnityEvent onXuixoEnding = new UnityEvent();
    [SerializeField] private UnityEvent onWonkyEnding = new UnityEvent();
    [SerializeField] private UnityEvent onExtraWonkyEnding = new UnityEvent();
    [SerializeField] private UnityEvent onCroissantEnding = new UnityEvent();

    [Header("Victory presentation")]
    [SerializeField, Min(0f)] private float sugarPauseSeconds = 0.65f;
    [SerializeField] private RectTransform awardRibbon;
    [SerializeField] private AudioSource awardAudio;
    [SerializeField] private AudioClip victoryJingle;

    // PNG pixels (x=640, y=700 measured from the top) from the raw and fried sprites.
    private static readonly Color32 RawColor = new Color32(247, 211, 174, 255);
    private static readonly Color32 FriedColor = new Color32(246, 167, 65, 255);
    private static readonly Color32 BurntColor = new Color32(45, 29, 27, 255);
    private static readonly Rect RollCrop = new Rect(0.055f, 0.31f, 0.89f, 0.29f);
    private static readonly Rect WonkyCrop = new Rect(0.055f, 0.27f, 0.89f, 0.40f);
    private static readonly Rect CroissantCrop = new Rect(0.095f, 0.235f, 0.80f, 0.51f);
    private const float RevealHeight = 112f;
    private const float RevealWidth = 320f;
    private const float OilEntryX = -140f;
    private const float FlipAnimationSeconds = 0.42f;
    private const float SplashSeconds = 0.24f;

    private enum Stage { Ready, Roll, Drop, Sink, Flip, Flipping, TakeOut, Bubbling, Pop, Sugar, SugarPause, Serve, JuryPause, Finished }
    private enum TimingCue { Wait, Now, TooLate }
    private enum Outcome { ExtraWonky, Wonky, WellMade, Croissant }

    private Stage stage;
    private float elapsed;
    private Outcome outcome;
    private float pastryHeight;
    private float heat;
    private bool IsSecondSide => stage == Stage.TakeOut;
    private bool HasGraphicCues => useGraphicCues && graphicCue != null;
    private float ActiveTargetStart => IsSecondSide ? secondTargetStart : targetStart;
    private float ActiveTargetEnd => Mathf.Max(ActiveTargetStart + 0.01f, IsSecondSide ? secondTargetEnd : targetEnd);
    // Both the visible cue and the next input check use the last displayed heat value.
    private TimingCue CurrentCue => heat < ActiveTargetStart ? TimingCue.Wait : heat <= ActiveTargetEnd ? TimingCue.Now : TimingCue.TooLate;
    private Color targetIdle;
    private Vector3 pastryScale;
    private Rect rawCrop;
    private Vector3 serveStart;
    private Vector3 trayPastryPosition;
    private float trayPastryWidth;

    private readonly Image[] bubbles = new Image[7];
    private float bubbleTime;
    private Image promptParchment;
    private Image hintParchment;
    private Image meterParchment;

    private void Start()
    {
        if (!canvas || !trayStart || !trayEdge || !oilSurface || !oilMask || !pastry || !pastry.texture
            || !friedXuixo || !sugaredXuixo || !wonkyXuixo || !extraWonkyXuixo || !croissant
            || !prompt || !hint || !meter || !meterLabel || !fill || !targetBand
            || !awardRibbon)
        {
            Debug.LogError("FryingFinale needs its scene layout, UI and pastry textures assigned.", this);
            enabled = false;
            return;
        }

        TemaPastisseria tema = TemaPastisseria.Carrega();
        Sprite parchment = tema != null ? tema.SpritePanell : EstilPastisseria.Arrodonit;
        promptParchment = StyleLabel(prompt, parchment);
        hintParchment = StyleLabel(hint, parchment);
        meterParchment = StyleLabel(meterLabel, parchment);

        LayoutTargetBand();
        targetIdle = targetBand.color;
        targetBand.gameObject.SetActive(true);
        // The meter is visible in edit mode for layout work, but not until frying during play.
        meter.gameObject.SetActive(false);
        awardRibbon.gameObject.SetActive(false);
        CreateBubbles();
        pastryHeight = pastry.rectTransform.rect.height;
        pastryScale = pastry.rectTransform.localScale;
        rawCrop = pastry.uvRect;
        trayPastryPosition = trayStart.InverseTransformPoint(pastry.rectTransform.position);
        trayPastryWidth = pastry.rectTransform.sizeDelta.x;
        stage = Stage.Ready;
        RefreshGraphicCue();
    }

    private static Image StyleLabel(Text label, Sprite parchment)
    {
        label.color = TemaPastisseria.ColorTextFosc;
        foreach (Shadow effect in label.GetComponents<Shadow>())
            effect.enabled = false;

        RectTransform text = label.rectTransform;
        GameObject background = new GameObject(label.name + " Parchment", typeof(RectTransform), typeof(Image));
        RectTransform rect = (RectTransform)background.transform;
        rect.SetParent(text.parent, false);
        rect.SetSiblingIndex(text.GetSiblingIndex());
        rect.anchorMin = text.anchorMin;
        rect.anchorMax = text.anchorMax;
        rect.pivot = text.pivot;
        rect.anchoredPosition = text.anchoredPosition;
        rect.sizeDelta = text.sizeDelta;

        Image image = background.GetComponent<Image>();
        image.sprite = parchment;
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
        return image;
    }

    private void Update()
    {
        bool pressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        elapsed += Time.deltaTime;
        AnimateBubbles(stage == Stage.Flip || stage == Stage.TakeOut || stage == Stage.Bubbling);

        // A transition consumes this frame's press; it can't affect the next stage too.
        switch (stage)
        {
            case Stage.Ready:
                if (pressed) SetStage(Stage.Roll, "Dins de la fregidora…", "Espera el moment just.");
                break;
            case Stage.Roll:
                float roll = Mathf.Clamp01(elapsed / 0.45f);
                pastry.rectTransform.position = Vector3.Lerp(trayStart.position, trayEdge.position, roll * roll);
                pastry.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -12f * roll);
                if (roll >= 1f) SetStage(Stage.Drop, prompt.text, hint.text);
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
                    SetStage(Stage.Sink, prompt.text, hint.text);
                }
                break;
            case Stage.Sink:
                float sink = Mathf.Clamp01(elapsed / 0.22f);
                pastry.rectTransform.anchoredPosition = new Vector2(OilEntryX, -pastryHeight * sink);
                if (sink >= 1f)
                {
                    pastry.enabled = false;
                    BeginTimingRound(secondSide: false);
                }
                break;
            case Stage.Flip:
            case Stage.TakeOut:
                UpdateTimingRound(pressed);
                break;
            case Stage.Flipping:
                // Input is ignored during the flip; the second timing round starts at splashdown.
                float flip = Mathf.Clamp01(elapsed / FlipAnimationSeconds);
                if (HasGraphicCues)
                    LayoutTargetBand(Mathf.Lerp(targetStart, secondTargetStart, flip),
                        Mathf.Lerp(ActiveTargetEnd, Mathf.Max(secondTargetStart + 0.01f, secondTargetEnd), flip));
                float arc = 4f * flip * (1f - flip);
                MovePastry(Mathf.Lerp(-pastryHeight, 34f, arc));
                Vector3 scale = pastryScale;
                scale.y *= Mathf.Lerp(0.22f, 1f, Mathf.Abs(Mathf.Cos(flip * Mathf.PI)));
                pastry.rectTransform.localScale = scale;
                pastry.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 12f * Mathf.Sin(flip * 2f * Mathf.PI));
                pastry.uvRect = flip < 0.5f ? rawCrop : new Rect(rawCrop.x, rawCrop.y + rawCrop.height, rawCrop.width, -rawCrop.height);
                if (flip >= 1f)
                {
                    pastry.enabled = false;
                    pastry.rectTransform.localScale = pastryScale;
                    pastry.rectTransform.localRotation = Quaternion.identity;
                    pastry.uvRect = rawCrop;
                    BeginTimingRound(secondSide: true);
                }
                break;
            case Stage.Bubbling:
                if (elapsed >= 0.65f)
                {
                    var (result, crop) = outcome switch
                    {
                        Outcome.WellMade => (friedXuixo, RollCrop),
                        Outcome.Wonky => (wonkyXuixo, WonkyCrop),
                        Outcome.Croissant => (croissant, CroissantCrop),
                        _ => (extraWonkyXuixo, WonkyCrop)
                    };
                    SetPastry(result, crop, RevealWidth);
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
                    if (outcome == Outcome.WellMade) SetStage(Stage.Sugar, "Un xuixo!", "[Espai] Ensucrar");
                    else if (outcome == Outcome.Croissant)
                        StartServing("Un croissant?!", "Sense crema, no hi ha xuixo.");
                    else
                    {
                        bool wonky = outcome == Outcome.Wonky;
                        StartServing(wonky ? "Un intent de xuixo…" : "Un fracàs de xuixo!",
                            wonky ? "Està bé, però intentem que la propera vegada la crema quedi dins del xuixo!"
                                  : "Hi ha més crema a la fregidora que en l'interior del xuixo...");
                    }
                }
                break;
            case Stage.Sugar:
                if (pressed)
                {
                    SetPastry(sugaredXuixo, RollCrop, RevealWidth);
                    SetStage(Stage.SugarPause, "Un xuixo perfecte!", "");
                }
                break;
            case Stage.SugarPause:
                if (elapsed >= sugarPauseSeconds) StartServing(prompt.text, hint.text);
                break;
            case Stage.Serve:
                float serve = Mathf.Clamp01(elapsed / 0.55f);
                RectTransform servingArt = pastry.rectTransform;
                float servingWidth = Mathf.Lerp(RevealWidth, trayPastryWidth, serve);
                servingArt.sizeDelta = new Vector2(servingWidth, pastryHeight * (servingWidth / RevealWidth));
                servingArt.localPosition = Vector3.Lerp(serveStart, trayPastryPosition, serve)
                    + Vector3.up * (24f * Mathf.Sin(serve * Mathf.PI));
                if (serve >= 1f)
                {
                    servingArt.localPosition = trayPastryPosition;
                    if (outcome == Outcome.WellMade)
                        SetStage(Stage.JuryPause, "El jurat ja ha decidit…", "");
                    else
                    {
                        SetStage(Stage.Finished, prompt.text, hint.text);
                        if (outcome == Outcome.Wonky) onWonkyEnding.Invoke();
                        else if (outcome == Outcome.Croissant) onCroissantEnding.Invoke();
                        else onExtraWonkyEnding.Invoke();
                        StartCoroutine(LoadCredits());
                    }
                }
                break;
            case Stage.JuryPause:
                if (elapsed >= 0.8f)
                {
                    awardRibbon.gameObject.SetActive(true);
                    if (awardAudio && victoryJingle) awardAudio.PlayOneShot(victoryJingle);
                    SetStage(Stage.Finished, "Millor Xuixo del Món", "I el premi al Millor Xuixo del Món és per a… tu!");
                    onXuixoEnding.Invoke();
                    StartCoroutine(LoadCredits());
                }
                break;
        }
    }

    private IEnumerator LoadCredits()
    {
        const string creditsScene = "Credits";
        if (!Application.CanStreamedLevelBeLoaded(creditsScene))
        {
            Debug.LogError("Add the Credits scene to the active Build Profile.", this);
            yield break;
        }

        yield return new WaitForSecondsRealtime(creditsDelay);
        SceneManager.LoadScene(creditsScene);
    }

    private void StartReveal(bool friedSuccessfully)
    {
        meter.gameObject.SetActive(false);
        int points = friedSuccessfully ? fryingPoints : 0;
        GameManager manager = GameManager.Instance;
        int total;
        if (manager != null && manager.HasScore(GameManager.BowlStage)
            && manager.HasScore(GameManager.StretchStage)
            && manager.HasScore(GameManager.CustardStage)
            && manager.HasScore(GameManager.RollStage))
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
            Debug.LogError("FryingFinale reached without all preceding stage scores. Start a run from ObradorIntro.", this);
            SetStage(Stage.Finished, "Could not finish this run", "Start a new run from the intro");
            return;
        }

        outcome = manager != null && manager.MissedAllCustardDrops ? Outcome.Croissant
            : total >= wellMadeMinScore ? Outcome.WellMade
            : total >= wonkyMinScore ? Outcome.Wonky : Outcome.ExtraWonky;
        SetStage(Stage.Bubbling, "Veiem el resultat..", "");
    }

    private void OnValidate()
    {
        wonkyMinScore = Mathf.Clamp(wonkyMinScore, 1, int.MaxValue - 1);
        wellMadeMinScore = Mathf.Max(wonkyMinScore + 1, wellMadeMinScore);
    }

    private void StartServing(string title, string instructions)
    {
        RectTransform art = pastry.rectTransform;
        serveStart = trayStart.InverseTransformPoint(art.position);
        art.SetParent(trayStart, false);
        art.anchorMin = art.anchorMax = new Vector2(0.5f, 0.5f);
        art.localPosition = serveStart;
        SetStage(Stage.Serve, title, instructions);
    }

    private void BeginTimingRound(bool secondSide)
    {
        SetStage(secondSide ? Stage.TakeOut : Stage.Flip,
            secondSide ? "2 / 2: Prepara't per treure la massa!" : "1 / 2: Gira la massa!", "");
        heat = 0f;
        LayoutTargetBand();
        targetBand.gameObject.SetActive(true);
        SetFill(0f, RawColor);
        ShowTimingCue(CurrentCue);
        meter.gameObject.SetActive(true);
    }

    private void UpdateTimingRound(bool pressed)
    {
        TimingCue shownCue = CurrentCue;
        if (pressed && shownCue != TimingCue.TooLate)
        {
            if (shownCue == TimingCue.Wait)
                StartReveal(false);
            else if (IsSecondSide)
                StartReveal(true);
            else
            {
                targetBand.gameObject.SetActive(false);
                meterParchment.color = Color.white;
                meterLabel.text = "Girem-lo!";
                SetFill(0f, RawColor);
                MovePastry(-pastryHeight);
                pastry.enabled = true;
                SetStage(Stage.Flipping, "Girant…", "Prepara't per treure'l de la fregidora!");
                if (HasGraphicCues)
                {
                    targetBand.color = targetIdle;
                    targetBand.gameObject.SetActive(true);
                    meterLabel.text = "Next: Lift";
                }
            }
            return;
        }

        heat = Mathf.Clamp01(elapsed / heatSeconds);
        Color color = heat < ActiveTargetStart
            ? Color.Lerp(RawColor, FriedColor, heat / ActiveTargetStart)
            : Color.Lerp(FriedColor, BurntColor, Mathf.InverseLerp(ActiveTargetEnd, 1f, heat));
        SetFill(heat, color);
        if (CurrentCue != shownCue) ShowTimingCue(CurrentCue);
        if (heat >= 1f) StartReveal(false);
    }

    private void ShowTimingCue(TimingCue cue)
    {
        targetBand.color = cue == TimingCue.Now ? targetHighlight : targetIdle;
        meterParchment.color = cue == TimingCue.Now ? new Color(1f, 0.863f, 0.529f, 1f) : Color.white;
        meterLabel.text = cue == TimingCue.Now ? (IsSecondSide ? "[Espai] Treu-lo!" : "[Espai] Gira la massa!")
            : cue == TimingCue.Wait ? "Espera.." : "Massa tard...";
        hint.text = cue switch
        {
            TimingCue.Now => IsSecondSide ? "Prem espai ara per treure la massa" : "Prem espai per girar la massa.",
            TimingCue.Wait => IsSecondSide ? "Wait until the fill reaches the gold band near the bottom."
                                           : "Wait until the fill reaches the gold band.",
            _ => "The fill passed the gold band."
        };
        if (HasGraphicCues && cue != TimingCue.TooLate)
            meterLabel.text = (IsSecondSide ? "Lift" : "Flip") + (cue == TimingCue.Wait ? " · Wait" : "");
        RefreshGraphicCue();
    }

    private void SetStage(Stage next, string title, string instructions)
    {
        stage = next;
        elapsed = 0f;
        prompt.text = title;
        hint.text = instructions;
        RefreshGraphicCue();
    }

    private void RefreshGraphicCue()
    {
        bool timing = stage == Stage.Flip || stage == Stage.TakeOut;
        bool guide = stage == Stage.Ready || stage == Stage.Sugar || timing || stage == Stage.Flipping;
        hint.enabled = !HasGraphicCues || (!guide && stage != Stage.Roll && stage != Stage.Drop && stage != Stage.Sink);
        promptParchment.enabled = prompt.enabled && !string.IsNullOrWhiteSpace(prompt.text);
        hintParchment.enabled = hint.enabled && !string.IsNullOrWhiteSpace(hint.text);
        if (graphicCue == null) return;
        if (!HasGraphicCues || !guide)
        {
            graphicCue.enabled = false;
            return;
        }

        FryerActionCue.ActionIcon icon = stage switch
        {
            Stage.Ready => FryerActionCue.ActionIcon.Drop,
            Stage.Sugar => FryerActionCue.ActionIcon.Sugar,
            Stage.Flip => FryerActionCue.ActionIcon.Flip,
            _ => FryerActionCue.ActionIcon.Lift
        };
        Vector2 position = stage == Stage.Ready ? trayStart.anchoredPosition + Vector2.up * 86f
            : stage == Stage.Sugar ? new Vector2(0f, -156f)
            : meter.anchoredPosition + Vector2.down * 179f;
        graphicCue.rectTransform.anchoredPosition = position;
        bool pressNow = stage == Stage.Ready || stage == Stage.Sugar || (timing && CurrentCue == TimingCue.Now);
        graphicCue.Show(icon, pressNow);
    }

    private void SetFill(float amount, Color color)
    {
        RectTransform rect = fill.rectTransform;
        rect.anchorMax = new Vector2(rect.anchorMax.x, amount);
        fill.color = color;
    }

    private void LayoutTargetBand() => LayoutTargetBand(ActiveTargetStart, ActiveTargetEnd);

    private void LayoutTargetBand(float start, float end)
    {
        RectTransform rect = targetBand.rectTransform;
        rect.anchorMin = new Vector2(rect.anchorMin.x, start);
        rect.anchorMax = new Vector2(rect.anchorMax.x, end);
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
        bool splashing = stage == Stage.TakeOut && elapsed < SplashSeconds;
        for (int i = 0; i < bubbles.Length; i++)
        {
            Image bubble = bubbles[i];
            bubble.enabled = visible;
            if (!visible) continue;

            if (splashing)
            {
                float t = Mathf.Clamp01(elapsed / SplashSeconds);
                float spread = i - (bubbles.Length - 1) * 0.5f;
                bubble.rectTransform.anchoredPosition = new Vector2(spread * (6f + 19f * t),
                    4f * t * (1f - t) * (30f - 4f * Mathf.Abs(spread)));
                bubble.rectTransform.localScale = Vector3.one * (1.15f - t * 0.6f);
                bubble.color = new Color(1f, 0.8f, 0.45f, 1f - t);
                continue;
            }

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
