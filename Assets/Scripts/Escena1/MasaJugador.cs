using UnityEngine;

namespace Xuixo.Escena1
{
    /// <summary>
    /// La massa del xuixo. Es mou dins el bol fins que aconsegueix sortir
    /// (sempre per la dreta) i despres rodes fins al corró del cuiner.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MasaJugador : MonoBehaviour
    {
        public enum Estat
        {
            DinsBol,
            Sortint,
            Carrera,
            Capturada
        }

        [Header("Referencies")]
        public BolCocina bol;
        public Transform visual;
        public Transform spriteVisual;
        public SpriteRenderer ombra;

        [Header("Dins el bol")]
        [Tooltip("Forca horitzontal aplicada amb les fletxes")]
        public float forcaRemenar = 8f;

        [Tooltip("Velocitat maxima dins el bol (no pot escapar corrent)")]
        public float velocitatMaximaBol = 1.5f;

        [Tooltip("Impuls que s'acumula per segon mentre es prem una fletxa")]
        public float carregaPerSegon = 0.45f;

        [Tooltip("Bonus d'impuls cada cop que canviem de sentit (balanceig)")]
        public float bonusCanviSentit = 0.1f;

        [Tooltip("Si no es toca res durant aquests segons, la massa surt igualment")]
        public float tempsMaximSenseEntrada = 10f;

        [Header("Sortida del bol")]
        public Vector2 impulsSortida = new Vector2(2.8f, 4.2f);

        [Header("Carrera")]
        public float velocitatCarrera = 2.5f;
        public float acceleracioCarrera = 12f;
        public float empentaExtra = 1.5f;

        [Header("Visual")]
        [Tooltip("Radi que es fa servir per calcular el gir de la massa")]
        public float radiRotacio = 0.6f;

        [Tooltip("Escala de l'ombra respecte el sprite base")]
        public float escalaOmbra = 0.4f;

        [Tooltip("Distancia a la dreta del bol on la massa ja es considera en carrera")]
        public float distanciaCarrera = 0.55f;

        [Tooltip("Alçada de l'ombra dins el bol")]
        public float yTerraDinsBol = -2.62f;

        [Tooltip("Alçada de l'ombra sobre la taula")]
        public float yTerraFora = -2.625f;

        public Estat EstatActual { get; private set; } = Estat.DinsBol;
        public float Carrega { get; private set; }
        public float CarregaEnSortir { get; private set; }

        public bool HaSortit => EstatActual != Estat.DinsBol;

        Rigidbody2D _rb;
        float _tempsSenseEntrada;
        float _tempsSortida;
        int _sentitAnterior;
        float _angle;
        bool _canviatOrdre;
        Vector3 _escalaAplastada = Vector3.one;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            if (EstatActual == Estat.Capturada) return;

            Vector2 entrada = EntradaJugador.Llegeix();

            switch (EstatActual)
            {
                case Estat.DinsBol:
                    ActualitzaDinsBol(entrada);
                    break;
                case Estat.Sortint:
                    ActualitzaSortida();
                    break;
                case Estat.Carrera:
                    ActualitzaCarrera(entrada);
                    break;
            }
        }

        void ActualitzaDinsBol(Vector2 entrada)
        {
            if (Mathf.Abs(entrada.x) > 0.15f)
            {
                _rb.AddForce(new Vector2(entrada.x * forcaRemenar, 0f), ForceMode2D.Force);

                int sentit = entrada.x > 0f ? 1 : -1;
                if (_sentitAnterior != 0 && sentit != _sentitAnterior)
                    Carrega += bonusCanviSentit;
                _sentitAnterior = sentit;

                // Cap a la dreta sempre ajuda una mica mes: la sortida segura es la dreta.
                Carrega += carregaPerSegon * Mathf.Abs(entrada.x) * (sentit > 0 ? 1f : 0.75f) * Time.fixedDeltaTime;
                _tempsSenseEntrada = 0f;
            }
            else
            {
                _tempsSenseEntrada += Time.fixedDeltaTime;

                // Tambe es pot donar impuls amunt/avall mentre es balanceja.
                if (Mathf.Abs(entrada.y) > 0.15f)
                    Carrega += carregaPerSegon * 0.4f * Time.fixedDeltaTime;
            }

            Vector2 v = _rb.linearVelocity;
            if (Mathf.Abs(v.x) > velocitatMaximaBol)
            {
                v.x = Mathf.Sign(v.x) * velocitatMaximaBol;
                _rb.linearVelocity = v;
            }

            if (bol != null)
                bol.SetIntensitat(Carrega);

            if (Carrega >= 1f || _tempsSenseEntrada >= tempsMaximSenseEntrada)
                SortirDelBol();
        }

        void ActualitzaSortida()
        {
            _tempsSortida += Time.fixedDeltaTime;

            // Quan la massa ja es mes amunt que la vora, ha de passar per davant del bol.
            if (!_canviatOrdre && bol != null && transform.position.y > bol.yVora)
            {
                PosaOrdreDeDibuix(30);
                _canviatOrdre = true;
            }

            if (_tempsSortida > 0.4f && _rb.linearVelocity.y <= 0f && transform.position.y < -1.6f && transform.position.x > PosicioBolX + distanciaCarrera)
            {
                EstatActual = Estat.Carrera;
            }

            if (transform.position.y < -7f)
                Reapareixer();
        }

        void ActualitzaCarrera(Vector2 entrada)
        {
            Vector2 v = _rb.linearVelocity;
            float objectiu = velocitatCarrera + Mathf.Max(0f, entrada.x) * empentaExtra;

            if (v.x < objectiu)
            {
                float forca = Mathf.Min(acceleracioCarrera, (objectiu - v.x) * 8f);
                _rb.AddForce(new Vector2(forca, 0f), ForceMode2D.Force);
            }
            else if (v.x > velocitatCarrera + empentaExtra + 1.5f)
            {
                v.x = velocitatCarrera + empentaExtra + 1.5f;
                _rb.linearVelocity = v;
            }

            if (transform.position.y < -7f)
                Reapareixer();
        }

        float PosicioBolX => bol != null ? bol.transform.position.x : transform.position.x;

        void SortirDelBol()
        {
            CarregaEnSortir = Mathf.Clamp01(Carrega);
            EstatActual = Estat.Sortint;
            Carrega = 1f;
            _tempsSortida = 0f;

            if (bol != null)
            {
                bol.ObrirBandaDreta();
                bol.SetIntensitat(0f);
            }

            _rb.angularVelocity = 0f;
            _rb.linearVelocity = impulsSortida;
        }

        void Reapareixer()
        {
            _rb.linearVelocity = Vector2.zero;
            transform.position = new Vector3(PosicioBolX, -1.6f, 0f);
            PosaOrdreDeDibuix(30);
        }

        void PosaOrdreDeDibuix(int ordre)
        {
            if (spriteVisual == null) return;
            SpriteRenderer sr = spriteVisual.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = ordre;
        }

        void LateUpdate()
        {
            Vector2 v = _rb != null ? _rb.linearVelocity : Vector2.zero;

            if (visual != null)
            {
                _angle -= (v.x / Mathf.Max(0.05f, radiRotacio)) * Mathf.Rad2Deg * Time.deltaTime;
                visual.localRotation = Quaternion.Euler(0f, 0f, _angle);
            }

            if (spriteVisual != null)
            {
                // Balanceig dins el bol + estirament quan cau.
                float balanceig = 0f;
                if (EstatActual == Estat.DinsBol)
                    balanceig = Mathf.Sin(Time.time * (7f + 12f * Carrega)) * (0.015f + 0.05f * Carrega);

                float estirament = Mathf.Clamp01(Mathf.Abs(v.y) / 28f) * 0.16f;
                spriteVisual.localScale = new Vector3(1f + balanceig - estirament * 0.5f, 1f - balanceig + estirament, 1f);
            }

            if (ombra != null)
            {
                Vector3 p = transform.position;
                bool dinsBol = EstatActual == Estat.DinsBol;
                float terra = dinsBol ? -2.9f : -2.97f;
                ombra.transform.position = new Vector3(p.x, terra, 0f);

                float alcada = Mathf.Max(0f, p.y - terra);
                float k = Mathf.Clamp01(1f - alcada / 5.5f);
                Color c = ombra.color;
                c.a = 0.30f * k;
                ombra.color = c;

                float escala = Mathf.Lerp(1.25f, 0.6f, k) * escalaOmbra;
                ombra.transform.localScale = new Vector3(escala, escala * 0.85f, 1f);
            }

            // petita respiracio quan ja ha acabat
            if (EstatActual == Estat.Capturada && spriteVisual != null)
            {
                float puls = 1f + Mathf.Sin(Time.time * 5f) * 0.02f;
                spriteVisual.localScale = new Vector3(_escalaAplastada.x * puls, _escalaAplastada.y, 1f);
            }
        }

        /// <summary>Atura la massa (la captura el corró).</summary>
        public void Capturar()
        {
            EstatActual = Estat.Capturada;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic;

            if (visual != null)
                visual.localRotation = Quaternion.identity;
        }

        /// <summary>Aplasta la massa (0 = normal, 1 = ben plana).</summary>
        public void Aplastar(float t)
        {
            if (spriteVisual == null) return;
            t = Mathf.Clamp01(t);
            _escalaAplastada = new Vector3(Mathf.Lerp(1f, 1.75f, t), Mathf.Lerp(1f, 0.26f, t), 1f);
            spriteVisual.localScale = _escalaAplastada;
        }
    }
}
