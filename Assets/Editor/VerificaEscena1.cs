using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Xuixo.Escena1;

namespace Xuixo.EditorTools
{
    /// <summary>
    /// Prova automatica de l'escena 1. Com que en batch no es pot jugar de veritat
    /// (el bucle de joc no avanca i falta el modul de Windows per fer un player),
    /// simulem la fisica pas a pas en mode edicio i capturem imatges dels moments clau.
    ///
    /// Unity -batchmode -projectPath . -executeMethod Xuixo.EditorTools.VerificaEscena1.Executar -quit
    /// </summary>
    public static class VerificaEscena1
    {
        public const string NomCaptures = "xuixo_escena1";
        const float DT = 0.02f;
        const float TempsMaxim = 25f;

        const float X_BOL = -1.65f;
        const float X_RODILLO = 2.90f;

        public static void Executar()
        {
            EditorSceneManager.OpenScene(Escena1Builder.RutaEscena);

            MasaJugador masa = UnityEngine.Object.FindFirstObjectByType<MasaJugador>();
            RodilloFinal final = UnityEngine.Object.FindFirstObjectByType<RodilloFinal>();
            CamaraEscena1 camara = UnityEngine.Object.FindFirstObjectByType<CamaraEscena1>();
            BolCocina bol = UnityEngine.Object.FindFirstObjectByType<BolCocina>();

            if (masa == null || final == null || camara == null || bol == null)
            {
                Fallar("falten objectes a l'escena (masa=" + (masa != null) + ", final=" + (final != null) +
                       ", camera=" + (camara != null) + ", bol=" + (bol != null) + ")");
                return;
            }

            SimulationMode2D modeOriginal = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;

            MethodInfo pasFisic = typeof(MasaJugador).GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo pasVisual = typeof(MasaJugador).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo pasBol = typeof(BolCocina).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo iniciMasa = typeof(MasaJugador).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo iniciBol = typeof(BolCocina).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);

            if (pasFisic == null || pasVisual == null || pasBol == null || iniciMasa == null || iniciBol == null)
            {
                Physics2D.simulationMode = modeOriginal;
                Fallar("no s'han trobat els metodes de simulacio");
                return;
            }

            // En mode edicio no s'executa Awake: l'ha de cridar la prova.
            iniciMasa.Invoke(masa, null);
            iniciBol.Invoke(bol, null);

            Rigidbody2D rb = masa.GetComponent<Rigidbody2D>();
            bool sortit = false;
            bool carrera = false;
            bool arribat = false;
            float t = 0f;
            float yMinima = float.MaxValue;

            Captura(camara, masa, "01_inici");
            Debug.Log("[VERIF] Inici. Masa a " + Text(rb.position) + " | Corro a " + Text(final.transform.position));

            while (t < TempsMaxim && !arribat)
            {
                // Balanceig: primer cap a l'esquerra i despres sempre cap a la dreta.
                EntradaJugador.EntradaForcada = t < 0.8f ? new Vector2(-1f, 0f) : Vector2.right;

                pasFisic.Invoke(masa, null);
                Physics2D.Simulate(DT);
                pasBol.Invoke(bol, null);
                pasVisual.Invoke(masa, null);
                t += DT;

                yMinima = Mathf.Min(yMinima, rb.position.y);

                if (!sortit && masa.HaSortit)
                {
                    sortit = true;
                    Debug.Log("[VERIF] Ha sortit del bol a t=" + t.ToString("0.00") + " s, pos=" + Text(rb.position));
                    Captura(camara, masa, "02_sortint");
                }

                if (!carrera && masa.EstatActual == MasaJugador.Estat.Carrera)
                {
                    carrera = true;
                    Debug.Log("[VERIF] Rodolant per la taula a t=" + t.ToString("0.00") + " s, pos=" + Text(rb.position));
                    Captura(camara, masa, "03_carrera");
                }

                if (!arribat)
                {
                    Collider2D colMasa = masa.GetComponent<Collider2D>();
                    Collider2D colFinal = final.GetComponent<Collider2D>();
                    if (colMasa != null && colFinal != null && colMasa.Distance(colFinal).isOverlapped)
                        arribat = true;
                }
            }

            if (arribat)
                Captura(camara, masa, "04_objectiu");

            Physics2D.simulationMode = modeOriginal;
            EntradaJugador.EntradaForcada = null;

            bool correcte = sortit && carrera && arribat;

            Debug.Log("[VERIF] Resum: sortit=" + sortit + " carrera=" + carrera + " arribat=" + arribat +
                      " temps=" + t.ToString("0.00") + " pos=" + Text(rb.position) + " yMin=" + yMinima.ToString("0.00"));

            if (!correcte)
            {
                Fallar("la masa no ha completat el recorregut");
                return;
            }

            if (yMinima < -5.5f)
            {
                Fallar("la masa ha caigut fora de la taula");
                return;
            }

            Debug.Log("[VERIF] OK: la masa ha sortit del bol i ha arribat al corro");
            EditorApplication.Exit(0);
        }

        static void Fallar(string missatge)
        {
            Debug.LogError("[VERIF] ERROR: " + missatge);
            EditorApplication.Exit(1);
        }

        static string Text(Vector2 v)
        {
            return "(" + v.x.ToString("0.00") + ", " + v.y.ToString("0.00") + ")";
        }

        static void Captura(CamaraEscena1 camara, MasaJugador masa, string nom)
        {
            Camera cam = camara.GetComponent<Camera>();
            if (cam == null) return;
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;

            const int W = 1280;
            const int H = 720;

            // La camera normalment segueix la masa des de LateUpdate, que en mode edicio no s'executa.
            float x = Mathf.Clamp(masa.transform.position.x, camara.minX, camara.maxX);
            camara.transform.position = new Vector3(x, camara.y, -10f);

            float aspectOriginal = cam.aspect;
            cam.aspect = W / (float)H;

            var rt = new RenderTexture(W, H, 24);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
            tex.Apply();

            string carpeta = Path.Combine(Path.GetTempPath(), NomCaptures);
            Directory.CreateDirectory(carpeta);
            string ruta = Path.Combine(carpeta, nom + ".png");
            File.WriteAllBytes(ruta, tex.EncodeToPNG());

            RenderTexture.active = null;
            cam.targetTexture = null;
            cam.aspect = aspectOriginal;

            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);

            Debug.Log("[VERIF] Captura: " + ruta);
        }
    }
}
