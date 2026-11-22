using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Xuixo.Escena1
{
    /// <summary>So de l'escena 1: efectes de la massa, del corró i musica de fons.</summary>
    public class AudioEscena1 : MonoBehaviour
    {
        [Header("Referencies")]
        public MasaJugador masa;
        public RodilloFinal final;

        [Header("Clips")]
        public AudioClip remenar;
        public AudioClip sortida;
        public AudioClip rodar;
        public AudioClip aterrar;
        public AudioClip corro;
        public AudioClip victoria;
        public AudioClip clic;
        public AudioClip musica;

        [Header("Volums")]
        [Range(0f, 1f)] public float volumMusica = 0.30f;
        [Range(0f, 1f)] public float volumEfectes = 0.80f;

        AudioSource _sfx;
        AudioSource _bucle;
        AudioSource _musica;

        MasaJugador.Estat _estatAnterior = MasaJugador.Estat.Capturada;
        bool _silenci;

        void Awake()
        {
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;

            _bucle = gameObject.AddComponent<AudioSource>();
            _bucle.playOnAwake = false;
            _bucle.loop = true;

            _musica = gameObject.AddComponent<AudioSource>();
            _musica.playOnAwake = false;
            _musica.loop = true;
        }

        void Start()
        {
            if (musica != null)
            {
                _musica.clip = musica;
                _musica.volume = volumMusica;
                _musica.Play();
            }
        }

        void Update()
        {
            if (SortidaJoc.BloquejaControls || masa == null) return;

            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                _silenci = !_silenci;
                AudioListener.volume = _silenci ? 0f : 1f;
            }

            MasaJugador.Estat estat = masa.EstatActual;
            if (estat != _estatAnterior) CanviEstat(_estatAnterior, estat);
            _estatAnterior = estat;

            // bucle d'esforç dins el bol
            if (estat == MasaJugador.Estat.DinsBol && masa.Carrega > 0.02f)
            {
                if (!_bucle.isPlaying && remenar != null)
                {
                    _bucle.clip = remenar;
                    _bucle.Play();
                }
                _bucle.volume = 0.14f + 0.16f * masa.Carrega;
                _bucle.pitch = 0.85f + 0.7f * masa.Carrega;
            }
            else if (estat == MasaJugador.Estat.Carrera)
            {
                if (!_bucle.isPlaying && rodar != null)
                {
                    _bucle.clip = rodar;
                    _bucle.pitch = 1f;
                    _bucle.Play();
                }
                _bucle.volume = 0.16f;
            }
            else if (_bucle.isPlaying)
            {
                _bucle.Stop();
            }
        }

        void CanviEstat(MasaJugador.Estat anterior, MasaJugador.Estat nou)
        {
            if (nou == MasaJugador.Estat.Sortint)
            {
                _bucle.Stop();
                So(sortida, 1f, 1f);
            }
            else if (nou == MasaJugador.Estat.Carrera)
            {
                So(aterrar, 0.9f, 1f);
            }
            else if (nou == MasaJugador.Estat.Capturada)
            {
                _bucle.Stop();
                So(corro, 1f, 1f);
                StartCoroutine(VictoriaAmbRetard(0.75f));
            }
        }

        IEnumerator VictoriaAmbRetard(float segons)
        {
            yield return new WaitForSeconds(segons);
            So(victoria, 0.9f, 1f);
        }

        void So(AudioClip clip, float volum, float to)
        {
            if (clip == null) return;
            _sfx.pitch = to;
            _sfx.PlayOneShot(clip, volum * volumEfectes);
        }

        public void SoClic()
        {
            So(clic, 0.7f, 1f);
        }
    }
}
