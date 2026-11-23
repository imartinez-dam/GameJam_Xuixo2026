using UnityEngine;

public class ApareixEsquitx : MonoBehaviour
{
    private Animator animacio;

    void Start()
    {
        animacio = GetComponent<Animator>();
    }

    void Update()
    {
        if (animacio != null && animacio.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            gameObject.SetActive(false);
        }
    }

    public void Esquitxar(Vector2 posicioInicial)
    {
        transform.position = posicioInicial;
        gameObject.SetActive(true);

        if (animacio != null)
        {
            animacio.Play("Esquitx", 0, 0f);
        }
    }
}