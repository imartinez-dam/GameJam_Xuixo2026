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

    private LineRenderer linia;

    private int ultimTram = -1;

    private Vector2[] punts;
    private float[] distancies;
    private int NombreSegments => punts.Length - 1;

    private PolygonCollider2D passadis;
    private Vector2 posicioAnterior;
    private float distanciaMaxima;
    private bool resseguint;
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
        AplicaEstilResultat();
        MostraPercentatge(0);
    }

    private void AplicaEstilResultat()
    {
        if (textPercentatge == null)
            return;

        TemaPastisseria tema = TemaPastisseria.Carrega();
        if (tema == null)
            return;

        RectTransform text = textPercentatge.rectTransform;
        GameObject panell = new GameObject("PergamiResultat", typeof(RectTransform));
        RectTransform fons = (RectTransform)panell.transform;
        fons.SetParent(text.parent, false);
        fons.SetSiblingIndex(text.GetSiblingIndex());
        fons.anchorMin = text.anchorMin;
        fons.anchorMax = text.anchorMax;
        fons.pivot = text.pivot;
        fons.anchoredPosition = text.anchoredPosition;
        fons.sizeDelta = text.sizeDelta;

        Image imatge = panell.AddComponent<Image>();
        imatge.sprite = tema.SpritePanell;
        imatge.type = Image.Type.Sliced;
        imatge.raycastTarget = false;

        textPercentatge.color = TemaPastisseria.ColorTextFosc;
        textPercentatge.fontSize = 34f;
        textPercentatge.margin = new Vector4(24f, 0f, 24f, 0f);
        textPercentatge.raycastTarget = false;
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
        if (textPercentatge != null)
            textPercentatge.text = $"Enrotllat: {percentatge}%";

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
            textPercentatge.text = $"{missatge} · {UltimPercentatge}%";

        ActualitzaLiniaProgres(UltimPercentatge);
        int punts = Mathf.RoundToInt(UltimPercentatge / 100f * puntsMaxims);
        StartCoroutine(StageTransition.Advance(GameManager.RollStage, punts, pausaResultat, escenaSeguent, this));
    }
}
