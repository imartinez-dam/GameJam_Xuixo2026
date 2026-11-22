using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Xuixo;
using Xuixo.UI;

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
public class EnrotllarMassa : MonoBehaviour
{
    [SerializeField] private Camera cameraJoc;
    [SerializeField] private TMP_Text textPercentatge;
    [SerializeField] private Transform marcaInici;
    [SerializeField] private Transform marcaFinal;
    [SerializeField] private string escenaSeguent = "FryingFinale";
    [SerializeField, Min(0f)] private float pausaResultat = 2f;
    [SerializeField, Min(0)] private int puntsMaxims = 20;
    [SerializeField, Range(0.2f, 0.8f)] private float amplada = 0.55f;
    [SerializeField] private Color colorSuperat = Color.green;
    [SerializeField] private Transform puntsEspiral;
    [SerializeField] private AudioSource soEnrotllar;



    public static int UltimPercentatge { get; private set; }

    private const int SegmentsTap = 8;
    private static readonly Color VoraMarca = new Color32(26, 16, 10, 255);

    private LineRenderer linia;
    private TMP_Text textProgresLateral;
    private GameObject panellResultat;
    private RectTransform etiquetaInici;
    private RectTransform fletxaInici;
    private TextMeshProUGUI textEtiquetaInici;

    private int ultimTram = -1;

    private Vector2[] punts;
    private float[] distancies;
    private int NombreSegments => punts.Length - 1;

    private PolygonCollider2D passadis;
    private Vector2 posicioAnterior;
    private float distanciaMaxima;
    private bool resseguint;
    private bool esperaRepresa;
    private bool acabat;
    private enum FinalIntent { Completat, ForaDelCami, BotoAlliberat, RatoliNoDisponible }

    private void Start()
    {
        if (cameraJoc == null)
            cameraJoc = Camera.main;

        if (cameraJoc == null || string.IsNullOrWhiteSpace(escenaSeguent)
            || !Application.CanStreamedLevelBeLoaded(escenaSeguent))
        {
            Debug.LogError("Assigna la càmera i afegeix l’escena següent al perfil de compilació.", this);
            enabled = false;
            return;
        }

        if (puntsEspiral == null || puntsEspiral.childCount < 2)
        {
            Debug.LogError("Assigna PuntsEspiral amb almenys dos punts fills en ordre.", this);
            enabled = false;
            return;
        }

        punts = new Vector2[puntsEspiral.childCount];
        distancies = new float[punts.Length];

        for (int i = 0; i < punts.Length; i++)
        {
            // Els fills poden tenir un pare desplaçat respecte d'EspiralMassa.
            punts[i] = transform.InverseTransformPoint(puntsEspiral.GetChild(i).position);

            if (i > 0 && (punts[i] - punts[i - 1]).sqrMagnitude < 0.000001f)
            {
                Debug.LogError($"Separa els punts consecutius {i - 1} i {i} de PuntsEspiral.", this);
                enabled = false;
                return;
            }
        }

        passadis = GetComponent<PolygonCollider2D>();
        GeneraEspiral();
        DecoraMarca(marcaInici, true);
        DecoraMarca(marcaFinal, false);
        PreparaPanells();
        MostraPercentatge(0);
    }

    private void PreparaPanells()
    {
        if (textPercentatge == null)
            return;

        TemaPastisseria tema = TemaPastisseria.Carrega();
        Sprite pergami = tema != null ? tema.SpritePanell : EstilPastisseria.Arrodonit;

        Transform pare = textPercentatge.transform.parent;
        RectTransform resultat = CreaPergamiLateral("PergamiResultat", pare, pergami, false);
        RectTransform text = textPercentatge.rectTransform;
        text.SetParent(resultat, false);
        AjustaTextPergami(text);

        textPercentatge.color = TemaPastisseria.ColorTextFosc;
        textPercentatge.fontSize = 36f;
        textPercentatge.alignment = TextAlignmentOptions.Center;
        textPercentatge.textWrappingMode = TextWrappingModes.Normal;
        textPercentatge.margin = Vector4.zero;
        textPercentatge.raycastTarget = false;
        panellResultat = resultat.gameObject;
        panellResultat.SetActive(false);

        RectTransform lateral = CreaPergamiLateral("PergamiProgres", pare, pergami, true);

        GameObject etiqueta = new GameObject("ProgresLateral", typeof(RectTransform));
        etiqueta.transform.SetParent(lateral, false);
        textProgresLateral = etiqueta.AddComponent<TextMeshProUGUI>();
        AjustaTextPergami(textProgresLateral.rectTransform);
        textProgresLateral.font = textPercentatge.font;
        textProgresLateral.color = TemaPastisseria.ColorTextFosc;
        textProgresLateral.fontSize = 28f;
        textProgresLateral.alignment = TextAlignmentOptions.Center;
        textProgresLateral.raycastTarget = false;

        if (marcaInici != null)
        {
            fletxaInici = CreaPergami("FletxaInici", pare, EstilPastisseria.Arrodonit);
            fletxaInici.sizeDelta = new Vector2(20f, 20f);
            fletxaInici.localRotation = Quaternion.Euler(0f, 0f, 45f);
            fletxaInici.GetComponent<Image>().color = VoraMarca;

            etiquetaInici = CreaPergami("EtiquetaInici", pare, pergami);
            etiquetaInici.sizeDelta = new Vector2(200f, 48f);
            GameObject titol = new GameObject("Text", typeof(RectTransform));
            titol.transform.SetParent(etiquetaInici, false);
            TextMeshProUGUI textInici = titol.AddComponent<TextMeshProUGUI>();
            textEtiquetaInici = textInici;
            RectTransform marcText = textInici.rectTransform;
            marcText.anchorMin = Vector2.zero;
            marcText.anchorMax = Vector2.one;
            marcText.offsetMin = marcText.offsetMax = Vector2.zero;
            textInici.font = textPercentatge.font;
            textInici.text = "Mantén premut";
            textInici.color = TemaPastisseria.ColorTextFosc;
            textInici.fontSize = 24f;
            textInici.alignment = TextAlignmentOptions.Center;
            textInici.raycastTarget = false;
        }
    }

    private static RectTransform CreaPergamiLateral(string nom, Transform pare, Sprite pergami, bool dreta)
    {
        RectTransform panell = CreaPergami(nom, pare, pergami);
        Vector2 costat = new Vector2(dreta ? 1f : 0f, 0.5f);
        panell.anchorMin = panell.anchorMax = panell.pivot = costat;
        panell.anchoredPosition = new Vector2(dreta ? -48f : 48f, 0f);
        panell.sizeDelta = dreta ? new Vector2(264f, 160f) : new Vector2(320f, 192f);
        return panell;
    }

    private static void AjustaTextPergami(RectTransform text)
    {
        text.anchorMin = Vector2.zero;
        text.anchorMax = Vector2.one;
        text.offsetMin = new Vector2(12f, 8f);
        text.offsetMax = new Vector2(-12f, -8f);
    }

    private void LateUpdate()
    {
        if (etiquetaInici == null || marcaInici == null)
            return;

        RectTransform pare = (RectTransform)etiquetaInici.parent;
        Vector3 referencia = esperaRepresa ? transform.TransformPoint(posicioAnterior) : marcaInici.position;
        Vector2 pantalla = cameraJoc.WorldToScreenPoint(referencia);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(pare, pantalla, null, out Vector2 punt))
            return;

        float migAmple = etiquetaInici.sizeDelta.x * 0.5f;
        float x = Mathf.Clamp(punt.x, pare.rect.xMin + migAmple, pare.rect.xMax - migAmple);
        etiquetaInici.localPosition = new Vector3(x, punt.y - 68f, 0f);
        float xFletxa = Mathf.Clamp(punt.x, x - migAmple + 15f, x + migAmple - 15f);
        fletxaInici.localPosition = new Vector3(xFletxa, punt.y - 43f, 0f);
    }

    private void DecoraMarca(Transform marca, bool inici)
    {
        if (marca == null || !marca.TryGetComponent(out SpriteRenderer cercle) || cercle.sprite == null)
            return;

        cercle.enabled = inici;
        cercle.color = TemaPastisseria.ColorTextClar;

        GameObject vora = new GameObject("Vora");
        vora.transform.SetParent(marca, false);
        LineRenderer anell = vora.AddComponent<LineRenderer>();
        anell.sharedMaterial = linia.sharedMaterial;
        anell.sortingLayerID = cercle.sortingLayerID;
        anell.sortingOrder = cercle.sortingOrder + 1;
        anell.useWorldSpace = false;
        anell.loop = true;
        anell.startColor = anell.endColor = VoraMarca;
        float radi = Mathf.Min(cercle.sprite.bounds.extents.x, cercle.sprite.bounds.extents.y) * 0.9f;
        anell.startWidth = anell.endWidth = radi * 0.25f;

        const int segments = 32;
        anell.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            anell.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radi);
        }
    }

    private static RectTransform CreaPergami(string nom, Transform pare, Sprite sprite)
    {
        GameObject panell = new GameObject(nom, typeof(RectTransform));
        RectTransform rect = (RectTransform)panell.transform;
        rect.SetParent(pare, false);

        Image imatge = panell.AddComponent<Image>();
        imatge.sprite = sprite;
        imatge.type = Image.Type.Sliced;
        imatge.raycastTarget = false;
        return rect;
    }

    private void GeneraEspiral()
    {
        linia = GetComponent<LineRenderer>();
        linia.useWorldSpace = false;
        linia.loop = false;
        linia.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
        linia.widthMultiplier = amplada * 0.75f;
        linia.startColor = colorSuperat;
        linia.endColor = colorSuperat;
        linia.numCapVertices = SegmentsTap;
        linia.positionCount = 0;

        for (int i = 1; i < punts.Length; i++)
        {
            distancies[i] = distancies[i - 1] + Vector2.Distance(punts[i - 1], punts[i]);
        }

        Vector2[] normals = new Vector2[punts.Length];
        for (int i = 0; i < punts.Length; i++)
        {
            int anterior = Mathf.Max(0, i - 1);
            int seguent = Mathf.Min(NombreSegments, i + 1);
            Vector2 tangent = (punts[seguent] - punts[anterior]).normalized;
            normals[i] = new Vector2(-tangent.y, tangent.x);
        }

        float migAmple = amplada / 2f;
        List<Vector2> contorn = new List<Vector2>();

        for (int i = 0; i < punts.Length; i++)
            contorn.Add(punts[i] + normals[i] * migAmple);

        AfegeixTap(contorn, punts[NombreSegments], normals[NombreSegments], migAmple);

        for (int i = NombreSegments; i >= 0; i--)
            contorn.Add(punts[i] - normals[i] * migAmple);

        AfegeixTap(contorn, punts[0], -normals[0], migAmple);

        passadis.offset = Vector2.zero;
        passadis.isTrigger = true;
        passadis.pathCount = 1;
        passadis.SetPath(0, contorn.ToArray());
        Physics2D.SyncTransforms();

        if (marcaInici != null)
            marcaInici.position = transform.TransformPoint(punts[0]);

        if (marcaFinal != null)
            marcaFinal.position = transform.TransformPoint(punts[NombreSegments]);
    }

    private static void AfegeixTap(
        List<Vector2> contorn, Vector2 centre, Vector2 normal, float radi)
    {
        Vector2 tangent = new Vector2(normal.y, -normal.x);

        for (int i = 1; i < SegmentsTap; i++)
        {
            float angle = Mathf.PI * i / SegmentsTap;
            Vector2 direccio = normal * Mathf.Cos(angle) + tangent * Mathf.Sin(angle);
            contorn.Add(centre + direccio * radi);
        }
    }

    private void Update()
    {
        if (acabat)
            return;

        if (SortidaJoc.BloquejaControls)
        {
            if (resseguint && !esperaRepresa)
            {
                esperaRepresa = true;
                if (textEtiquetaInici != null)
                    textEtiquetaInici.text = "Reprèn aquí";
                if (soEnrotllar != null)
                    soEnrotllar.Pause();
            }
            return;
        }

        Mouse ratoli = Mouse.current;

        if (ratoli == null)
        {
            if (resseguint)
                Acaba(FinalIntent.RatoliNoDisponible);

            return;
        }

        Vector2 pantalla = ratoli.position.ReadValue();
        bool clic = ratoli.leftButton.wasPressedThisFrame;
        bool premut = ratoli.leftButton.isPressed;

        Ray raig = cameraJoc.ScreenPointToRay(pantalla);
        Plane pla = new Plane(transform.forward, transform.position);

        if (!pla.Raycast(raig, out float distanciaRaig))
        {
            if (resseguint)
                Acaba(FinalIntent.ForaDelCami);
            return;
        }

        Vector2 posicio = transform.InverseTransformPoint(raig.GetPoint(distanciaRaig));

        // El clic al diàleg no compta com a part del recorregut.
        if (esperaRepresa)
        {
            if (premut && Vector2.Distance(posicio, posicioAnterior) <= amplada * 0.45f
                && EsDinsDelPassadis(posicio))
            {
                esperaRepresa = false;
                posicioAnterior = posicio;
                if (textEtiquetaInici != null)
                    textEtiquetaInici.text = "Mantén premut";
                if (soEnrotllar != null)
                    soEnrotllar.UnPause();
            }
            return;
        }

        if (!resseguint)
        {
            if (clic && premut && Vector2.Distance(posicio, punts[0]) <= amplada * 0.45f
                && EsDinsDelPassadis(posicio))
            {
                resseguint = true;
                posicioAnterior = posicio;

                if (soEnrotllar != null)
                {
                    soEnrotllar.Play();
                }
            }

            return;
        }

        if (!premut)
        {
            Acaba(FinalIntent.BotoAlliberat);
            return;
        }

        int passos = Mathf.Max(1,
            Mathf.CeilToInt(Vector2.Distance(posicioAnterior, posicio) / (amplada / 5f)));

        for (int i = 1; i <= passos; i++)
        {
            Vector2 mostra = Vector2.Lerp(posicioAnterior, posicio, (float)i / passos);

            if (!EsDinsDelPassadis(mostra))
            {
                Acaba(FinalIntent.ForaDelCami);
                return;
            }

            distanciaMaxima = Mathf.Max(distanciaMaxima, DistanciaSobreEspiral(mostra));

            if (Vector2.Distance(mostra, punts[NombreSegments]) <= amplada * 0.3f)
            {
                Acaba(FinalIntent.Completat);
                return;
            }
        }

        posicioAnterior = posicio;
        MostraPercentatge(CalculaPercentatge());
    }

    private bool EsDinsDelPassadis(Vector2 posicioLocal)
    {
        return passadis.OverlapPoint(transform.TransformPoint(posicioLocal));
    }

    private float DistanciaSobreEspiral(Vector2 posicio)
    {
        float millorDistanciaQuadrada = float.PositiveInfinity;
        float distanciaRecorreguda = 0f;

        for (int i = 0; i < NombreSegments; i++)
        {
            Vector2 segment = punts[i + 1] - punts[i];
            float t = Mathf.Clamp01(Vector2.Dot(posicio - punts[i], segment) / segment.sqrMagnitude);
            Vector2 projeccio = punts[i] + segment * t;
            float distanciaQuadrada = (posicio - projeccio).sqrMagnitude;

            if (distanciaQuadrada < millorDistanciaQuadrada)
            {
                millorDistanciaQuadrada = distanciaQuadrada;
                distanciaRecorreguda = Mathf.Lerp(distancies[i], distancies[i + 1], t);
            }
        }

        return distanciaRecorreguda;
    }

    private void ActualitzaLiniaProgres(int percentatge)
    {
        int tram = percentatge / 5;

        if (tram == ultimTram)
            return;

        ultimTram = tram;

        if (tram == 0)
        {
            linia.positionCount = 0;
            return;
        }

        float limit = distancies[NombreSegments] * (tram / 20f);
        int final = 1;
        while (final < NombreSegments && distancies[final] < limit)
            final++;

        float t = Mathf.InverseLerp(distancies[final - 1], distancies[final], limit);
        Vector2 punta = Vector2.Lerp(punts[final - 1], punts[final], t);

        linia.positionCount = final + 1;
        for (int i = 0; i < final; i++)
            linia.SetPosition(i, punts[i]);

        linia.SetPosition(final, punta);
    }
    private int CalculaPercentatge()
    {
        return Mathf.Min(99, Mathf.FloorToInt(100f * distanciaMaxima / distancies[NombreSegments]));
    }

    private void MostraPercentatge(int percentatge)
    {
        if (textProgresLateral != null)
            textProgresLateral.text = $"<size=72><b>{percentatge}%</b></size>\nEnrotllat";

        ActualitzaLiniaProgres(percentatge);
    }

    private void Acaba(FinalIntent motiu)
    {
        acabat = true;
        if(soEnrotllar != null)
        { soEnrotllar.Stop(); }

        UltimPercentatge = motiu == FinalIntent.Completat ? 100 : CalculaPercentatge();

        string missatge = motiu switch
        {
            FinalIntent.Completat => "Massa enrotllada!",
            FinalIntent.ForaDelCami => "Has sortit del camí",
            FinalIntent.BotoAlliberat => "Has deixat anar el botó",
            _ => "No es detecta el ratolí"
        };

        if (textPercentatge != null)
        {
            textPercentatge.text = $"<size=44><b>RESULTAT</b></size>\n{missatge}";
            panellResultat.SetActive(true);
        }

        MostraPercentatge(UltimPercentatge);
        int punts = Mathf.RoundToInt(UltimPercentatge / 100f * puntsMaxims);
        StartCoroutine(StageTransition.Advance(GameManager.RollStage, punts, pausaResultat, escenaSeguent, this));
    }
}
