using UnityEngine;

public class Crema : MonoBehaviour
{
    public MovimentXuixo xuixo;
    public ApareixEsquitx esquitx;

    private Vector2 posicioInicial;
    private Rigidbody2D crema;

    void Start()
    {
        posicioInicial = transform.position;
        crema = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!gameObject.activeSelf) return;

        if (other.gameObject.CompareTag("Xuixo"))
        {
            Reiniciar();
            xuixo.ResolGota(true);
        }
        else if (other.gameObject.CompareTag("Terra"))
        {
            ContactPoint2D punt = other.GetContact(0);

            esquitx.Esquitxar(punt.point);

            Reiniciar();
            xuixo.ResolGota(false);
        }
    }

    void Reiniciar()
    {
        crema.linearVelocity = Vector2.zero;
        crema.angularVelocity = 0f;

        transform.position = posicioInicial;
        gameObject.SetActive(false);
    }
}
