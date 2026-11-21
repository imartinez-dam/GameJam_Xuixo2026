using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MassaScript : MonoBehaviour
{
    public bool escenaAcabada;
    public int intents;
    public float puntuacio;
    public MouBarra Barra;
    private float duracio = 1;
    private float multScale = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    void Start()
    {

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
                
                float trueprog = Barra.progress%1;

                if (trueprog > 0.5) trueprog = 1f-trueprog;
                puntuacio += trueprog/0.5f*100f;
                
                Vector3 end = Barra.transform.position;
                end.x -= transform.localScale.x*multScale;
                StartCoroutine(MoveBetween(Barra.transform.position, end));
            } 
        }
        
    }
    

    IEnumerator iniciarBarra()
    {
        yield return new WaitForSeconds(0.2f);
        Barra.iniciat = true;
    }

    IEnumerator MoveBetween(Vector3 start, Vector3 end)
    {
        
        float elapsedTime = 0f;
        Vector2 originalScale = transform.localScale;

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


            elapsedTime += Time.deltaTime;

            yield return null;
        }

        Barra.transform.position = end;
        
        
        float recorregut = transform.localScale.x*multScale/Barra.Length;
        if(Barra.progress > 1)
        {
            Barra.progress += recorregut;
            if(Barra.progress>2) Barra.progress = 0;
        } else
        {
            Barra.progress = 2-Barra.progress+recorregut;
            if(Barra.progress>2) Barra.progress = 0;
        }

        if (intents != 0)
        {
            Barra.Paused = false;
        } else
        {
            puntuacio /= 3;
            escenaAcabada = true;
        }
    }


}
