using System.Collections;
using UnityEngine;
using Xuixo;

public class MassaScript : MonoBehaviour
{
    public bool escenaAcabada;
    public int intents;
    public float puntuacio;
    public MouBarra Barra;
    [SerializeField] private AudioSource soEstirar;
    [Header("Transició")]
    [SerializeField] private string escenaSeguent = "Crema";
    [SerializeField, Min(0f)] private float pausaResultat = 1.5f;
    [SerializeField, Min(0)] private int puntsMaxims = 20;

    private int intentsInicials;
    private float duracio = 1;
    private float multScale = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    void Start()
    {
        intentsInicials = Mathf.Max(1, intents);
    }


    // Update is called once per frame
    void Update()
    {   

        if (Input.GetKeyDown(KeyCode.Space) && intents > 0)
        {
            if (!Barra.iniciat)
            {
                Barra.Paused = false;
                StartCoroutine(iniciarBarra());
            } else if(!Barra.Paused)
            {
                Barra.Paused = true;
                intents--;
                if (soEstirar != null) soEstirar.Play();
                
                Vector3 end = Barra.transform.position;
                if(Barra.progress < 1){
                    if(Mathf.Abs(transform.position.x-end.x) < 2) puntuacio += 100;
                    else if(Mathf.Abs(transform.position.x-end.x) < 4) puntuacio += 70;
                    else puntuacio += 10;
                    end.x += transform.localScale.x*multScale;
                    
                } else {
                    float trueprog = Barra.progress-1;
                    if (trueprog > 0.5) trueprog = 1f-trueprog;
                    puntuacio += trueprog/0.5f*100f;

                    end.x -= transform.localScale.x*multScale;
                    
                }

                bool animar;
                if(((Barra.progress < 1 && (Barra.progress > 0.7 || Barra.progress < 0.1)) || (Barra.progress > 1 && (1.3 > Barra.progress || 1.8 < Barra.progress))))
                {
                    animar = false;
                } else
                {
                    animar = true;
                }
                StartCoroutine(MoveBetween(Barra.transform.position, end, animar));
                
            } 
        }
        
    }
    

    IEnumerator iniciarBarra()
    {
        yield return new WaitForSeconds(0.2f);
        Barra.iniciat = true;
    }

    IEnumerator MoveBetween(Vector3 start, Vector3 end, bool animar)
    {
        
        float elapsedTime = 0f;
        Vector2 originalScale = transform.localScale;
        Vector2 originalPos = transform.position;
        Vector2 targetScale = originalScale;
        targetScale.x += 0.5f;
        Vector2 targetPosition = originalPos;
        if(start.x>end.x){
            targetPosition.x -= 1.4f;
        } else {
            targetPosition.x += 1.4f;
        }
        


        while (elapsedTime < duracio)
        {
            float t = elapsedTime / duracio;

            // Ease In and Ease Out
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            
            Barra.transform.position = Vector3.Lerp(
                start,
                end,
                easedT
            );

            if (animar)
            {
                transform.position = Vector3.Lerp(
                    originalPos,
                    targetPosition,
                    easedT
                );

                transform.localScale = Vector3.Lerp(
                    originalScale,
                    targetScale,
                    easedT
                );
            }


            elapsedTime += Time.deltaTime;

            yield return null;
        }

        if (animar)
        {
            transform.position = targetPosition;
            transform.localScale = targetScale;
        }

        Barra.transform.position = end;
        
        
        float recorregut = transform.localScale.x*multScale/Barra.Length;
        if(Barra.progress > 1)
        {
            Barra.progress += recorregut;
            if(Barra.progress>2) Barra.progress = 0;
        } else
        {
            Barra.progress += recorregut;
            if(Barra.progress>1) Barra.progress = 1;
        }

        if (soEstirar != null) soEstirar.Stop();

        if (intents != 0)
        {
            Barra.Paused = false;
        } else
        {
             puntuacio /= intentsInicials;
             escenaAcabada = true;
             int punts = Mathf.RoundToInt(Mathf.Clamp01(puntuacio / 100f) * puntsMaxims);
             StartCoroutine(StageTransition.Advance(GameManager.StretchStage, punts, pausaResultat, escenaSeguent, this));
         }
     }

    //private void OnGUI()
    //{
    //    if (escenaAcabada)
    //        StageTransition.DrawResult("Dough stretched");
    //}
}
