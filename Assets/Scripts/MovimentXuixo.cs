using UnityEngine;

public class MovimentXuixo : MonoBehaviour
{
    private SpriteRenderer massa;
    public Sprite[] sprites;
    private int intents = 3;
    private int cremaenmassa = 0;
    private float velocitat = 3f;
    private float incrementvelocitat = 1f;
    private int direccio = 2;
    public GameObject crema;

    void Start()
    {
        massa = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        transform.Translate(Vector2.right * velocitat * direccio * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && intents > 0 && !crema.activeSelf)
        {
            crema.SetActive(true);
            intents--;
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Paret"))
        {
            direccio *= -1;
        }
    }

    public void AugmentarVelocitat()
    {
        velocitat = velocitat + incrementvelocitat;
        cremaenmassa++;

        if (cremaenmassa >= 1 && cremaenmassa <= sprites.Length)
        {
            massa.sprite = sprites[cremaenmassa - 1];
        }
    }
}