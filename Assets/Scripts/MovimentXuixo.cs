using UnityEngine;
using Xuixo;

public class MovimentXuixo : MonoBehaviour
{
    private SpriteRenderer massa;
    public Sprite[] sprites;
    private const int IntentsTotals = 3;
    private int intents = IntentsTotals;
    private int cremaenmassa = 0;
    private float velocitat = 3f;
    private float incrementvelocitat = 1f;
    private int direccio = 2;
    public GameObject crema;
    [SerializeField] private AudioSource soCrema;

    [Header("Transició")]
    [SerializeField] private string escenaSeguent = "EnrotllarMassa";
    [SerializeField, Min(0f)] private float pausaResultat = 1.5f;
    [SerializeField, Min(0)] private int puntsMaxims = 20;

    private bool gotaEnCurs;
    private bool acabat;

    void Start()
    {
        massa = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (SortidaJoc.BloquejaControls || acabat) return;

        transform.Translate(Vector2.right * velocitat * direccio * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && intents > 0 && !gotaEnCurs && !crema.activeSelf)
        {
            gotaEnCurs = true;
            intents--;
            crema.SetActive(true);
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Paret"))
        {
            direccio *= -1;
        }
    }

    public void ResolGota(bool encert)
    {
        if (!gotaEnCurs || acabat) return;

        gotaEnCurs = false;

        if (encert)
        {
            AugmentarVelocitat();
            if(soCrema != null)
            {
                soCrema.Play();
            }
        }

        if (intents > 0) return;

        acabat = true;
        GameManager.Instance?.RecordCustardResult(cremaenmassa);
        int punts = Mathf.RoundToInt((float)cremaenmassa / IntentsTotals * puntsMaxims);
        StartCoroutine(StageTransition.Advance(GameManager.CustardStage, punts, pausaResultat, escenaSeguent, this));
    }




    private void AugmentarVelocitat()
    {
        velocitat = velocitat + incrementvelocitat;
        cremaenmassa++;

        if (cremaenmassa >= 1 && cremaenmassa <= sprites.Length)
        {
            massa.sprite = sprites[cremaenmassa - 1];
        }
    }
}
