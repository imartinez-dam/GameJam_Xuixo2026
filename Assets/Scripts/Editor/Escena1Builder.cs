using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Xuixo.Escena1;

namespace Xuixo.EditorTools
{
    /// <summary>
    /// Construeix l'escena 1 del joc (el bol de la massa) des de zero.
    /// Menu: Xuixo > Generar Escena 1 (El Bol)
    /// Tambe es pot executar en batch: -executeMethod Xuixo.EditorTools.Escena1Builder.Generar
    ///
    /// ENCARA AMB EL FONS (Escena2.jpg), PERO LA MECÀNICA ES FA A LA TAULA GRAN
    /// DEL DAVANT, amb el bol i el corró com a sprites propis i visibles.
    /// </summary>
    public static class Escena1Builder
    {
        public const string RutaEscena = "Assets/Scenes/" + SceneNames.BowlEscape + ".unity";
        const string CarpetaFisica = ContentPaths.Bowl + "Physics";

        const float FRACCIO_TERRA_FONS = 0.275f;
        const float Y_TERRA_FONS = -2.69f;

        // Taula gran del davant (on es juga)
        const float Y_TAULA = -2.30f;
        const float X_BOL = -3.60f;
        const float X_RODILLO = 2.90f;

        // Geometria dels sprites (unitats)
        const float BOL_OFFSET_CENTRE = 0.475f;
        const float RODILLO_OFFSET_CENTRE = 0.15f;
        const float Y_VORA_BOL = Y_TAULA + BOL_OFFSET_CENTRE + 0.05f;

        public static void Generar()
        {
            ArtEscena1.GeneraTots();

            Sprite taulaGran = CarregaSprite("taula_gran");
            Sprite bolDarrere = CarregaSprite("bol_gran_darrere");
            Sprite bolVora = CarregaSprite("bol_gran_vora");
            Sprite corroGran = CarregaSprite("corro_gran");

            PhysicsMaterial2D matMasa = Material("Masa", 0.55f, 0.04f);
            PhysicsMaterial2D matBol = Material("Bol", 0.40f, 0.02f);
            PhysicsMaterial2D matTaula = Material("Taula", 0.65f, 0.02f);

            var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ------------------------------------------------------------- camera
            var camGO = new GameObject("Camera Principal");
            var cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 3.8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.96f, 0.91f, 0.82f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            camGO.transform.position = new Vector3(0f, -0.8f, -10f);
            camGO.AddComponent<AudioListener>();
            var camScript = camGO.AddComponent<CamaraEscena1>();

            // ---------------------------------------------------------------- fons
            var fonsGO = new GameObject("Fons");
            var fonsSR = fonsGO.AddComponent<SpriteRenderer>();
            fonsSR.sprite = ArtEscena1.Fons;
            fonsSR.sortingOrder = -100;
            var fonsScript = fonsGO.AddComponent<FonsEscena1>();
            fonsScript.fons = fonsSR;
            fonsScript.fraccioTerra = FRACCIO_TERRA_FONS;
            fonsScript.yTerra = Y_TERRA_FONS;
            fonsScript.parallax = 0f;
            fonsScript.Ajusta();

            // -------------------------------------------------------- taula gran
            var taulaGO = new GameObject("Taula gran");
            var taulaSR = taulaGO.AddComponent<SpriteRenderer>();
            taulaSR.sprite = taulaGran;
            taulaSR.sortingOrder = -10;
            taulaGO.transform.position = new Vector3(2f, Y_TAULA - 0.75f, 0f);

            var taulaCol = taulaGO.AddComponent<BoxCollider2D>();
            taulaCol.size = new Vector2(20f, 1.5f);
            taulaCol.sharedMaterial = matTaula;

            // ---------------------------------------------------------------- bol
            // El bol es un sprite propi (darrere + vora davant), així es veu clar.
            var bolGO = new GameObject("Bol de cuina");
            bolGO.transform.position = new Vector3(X_BOL, Y_TAULA + BOL_OFFSET_CENTRE, 0f);

            SpriteRenderer interior = CreaSprite(bolGO.transform, "Interior", bolDarrere, Vector3.zero, 5);
            SpriteRenderer vora = CreaSprite(bolGO.transform, "Vora", bolVora, Vector3.zero, 20);

            var colisio = NouFill(bolGO.transform, "Colisio");
            var superficie = colisio.gameObject.AddComponent<EdgeCollider2D>();
            superficie.sharedMaterial = matBol;
            superficie.edgeRadius = 0.01f;
            superficie.points = PuntsBol(41, 0.65f, 0.475f);

            var guardaEsquerra = colisio.gameObject.AddComponent<BoxCollider2D>();
            guardaEsquerra.offset = new Vector2(-0.68f, 0.18f);
            guardaEsquerra.size = new Vector2(0.06f, 0.30f);

            var guardaDreta = colisio.gameObject.AddComponent<BoxCollider2D>();
            guardaDreta.offset = new Vector2(0.68f, 0.18f);
            guardaDreta.size = new Vector2(0.06f, 0.30f);

            var bol = bolGO.AddComponent<BolCocina>();
            bol.visuals = new[] { interior.transform, vora.transform };
            bol.guardaDreta = guardaDreta;
            bol.yVora = Y_VORA_BOL;

            // --------------------------------------------------------------- massa
            var masaGO = new GameObject("Masa");
            masaGO.transform.position = new Vector3(X_BOL, Y_TAULA + 0.32f, 0f);

            var rb = masaGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.linearDamping = 0.08f;
            rb.angularDamping = 0.05f;

            var cercle = masaGO.AddComponent<CircleCollider2D>();
            cercle.radius = 0.17f;
            cercle.sharedMaterial = matMasa;

            Transform visual = NouFill(masaGO.transform, "Visual");
            SpriteRenderer cos = CreaSprite(visual, "Cos", ArtEscena1.Masa, Vector3.zero, 10);

            var ombra = CreaSprite(null, "Ombra Masa", ArtEscena1.Ombra, new Vector3(X_BOL, Y_TAULA + 0.005f, 0f), 6);
            ombra.color = new Color(1f, 1f, 1f, 0.4f);
            ombra.transform.localScale = new Vector3(0.55f, 0.45f, 1f);

            var masa = masaGO.AddComponent<MasaJugador>();
            masa.bol = bol;
            masa.visual = visual;
            masa.spriteVisual = cos.transform;
            masa.ombra = ombra;

            masa.forcaRemenar = 12f;
            masa.velocitatMaximaBol = 2.2f;
            masa.impulsSortida = new Vector2(5.4f, 7.5f);
            masa.velocitatCarrera = 3.0f;
            masa.acceleracioCarrera = 17f;
            masa.empentaExtra = 2.5f;
            masa.radiRotacio = 0.6f;
            masa.escalaOmbra = 0.55f;
            masa.distanciaCarrera = 0.85f;
            masa.yTerraDinsBol = Y_TAULA + 0.05f;
            masa.yTerraFora = Y_TAULA + 0.005f;

            // -------------------------------------------------------------- corró
            var rodilloGO = new GameObject("Corró del cuiner");
            rodilloGO.transform.position = new Vector3(X_RODILLO, Y_TAULA + RODILLO_OFFSET_CENTRE, 0f);
            CreaSprite(rodilloGO.transform, "Art", corroGran, Vector3.zero, 35);

            var trigger = rodilloGO.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.95f, 0.40f);

            var final = rodilloGO.AddComponent<RodilloFinal>();
            final.masa = masa;
            final.camara = camScript;
            final.escenaSeguent = SceneNames.StretchDough;

            // ---------------------------------------------------------------- HUD
            var hudGO = new GameObject("HUD");
            var hud = hudGO.AddComponent<HUD_Escena1>();
            hud.masa = masa;
            hud.final = final;
            hud.logotip = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "logo_xuixo.png");
            hud.barraMarc = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "barra_marc.png");
            hud.barraFarciment = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "barra_farciment.png");
            hud.panell = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "panell_9slice.png");
            hud.iconaEsquerra = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "icona_fletxa_esquerra.png");
            hud.iconaDreta = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "icona_fletxa_dreta.png");
            hud.indicadorActiu = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "indicador_actiu.png");
            hud.indicadorInactiu = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "indicador_inactiu.png");
            hud.botoPremut = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "boto_premut.png");
            hud.botoNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.UiArt + "boto_normal.png");

            // ------------------------------------------------------------ audio
            var audioGO = new GameObject("Audio");
            var audio = audioGO.AddComponent<AudioEscena1>();
            audio.masa = masa;
            audio.final = final;
            audio.remenar = Clip("massa_remenar");
            audio.sortida = Clip("massa_sortida");
            audio.rodar = Clip("massa_rodar");
            audio.aterrar = Clip("massa_aterrar");
            audio.corro = Clip("corro");
            audio.victoria = Clip("victoria");
            audio.clic = Clip("ui_clic");

            // -------------------------------------------------------------- vfx
            var vfxGO = new GameObject("VFX");
            var vfx = vfxGO.AddComponent<VfxEscena1>();
            vfx.masa = masa;
            vfx.final = final;
            vfx.farina = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.BowlWorld + "Effects/farina.png");
            vfx.estrelles = AssetDatabase.LoadAssetAtPath<Texture2D>(ContentPaths.BowlWorld + "Effects/estrelles.png");

            camScript.objectiu = masaGO.transform;
            camScript.minX = 0f;
            camScript.maxX = 0f;
            camScript.y = -0.8f;

            Directory.CreateDirectory(Path.GetFullPath("Assets/Scenes"));
            EditorSceneManager.SaveScene(escena, RutaEscena);
            AfegeixABuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Xuixo] Escena 1 generada a " + RutaEscena);
        }

        // ------------------------------------------------------------------ utils

        static Sprite CarregaSprite(string nom)
        {
            string path = "Props/" + nom + ".png";
            return AssetDatabase.LoadAssetAtPath<Sprite>(ContentPaths.BowlWorld + path)
                ?? AssetDatabase.LoadAssetAtPath<Sprite>(ContentPaths.WorldArt + path);
        }

        /// <summary>Punts de la cara interior del bol (semi-el·lipse) en espai local.</summary>
        static Vector2[] PuntsBol(int quants, float migAmplada, float fondaria)
        {
            var punts = new Vector2[quants];
            for (int i = 0; i < quants; i++)
            {
                float t = i / (quants - 1f);
                float x = Mathf.Lerp(-migAmplada, migAmplada, t);
                float y = 0.05f - fondaria * Mathf.Sqrt(Mathf.Max(0f, 1f - (x / migAmplada) * (x / migAmplada)));
                punts[i] = new Vector2(x, y);
            }
            return punts;
        }

        static Transform NouFill(Transform pare, string nom)
        {
            var go = new GameObject(nom);
            go.transform.SetParent(pare, false);
            return go.transform;
        }

        static SpriteRenderer CreaSprite(Transform pare, string nom, Sprite sprite, Vector3 posicio, int ordre)
        {
            var go = new GameObject(nom);
            if (pare != null)
            {
                go.transform.SetParent(pare, false);
                go.transform.localPosition = posicio;
            }
            else
            {
                go.transform.position = posicio;
            }

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = ordre;
            return sr;
        }

        static AudioClip Clip(string nom)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(ContentPaths.BowlAudio + nom + ".wav")
                ?? AssetDatabase.LoadAssetAtPath<AudioClip>(ContentPaths.Shared + "Audio/" + nom + ".wav");
        }

        static PhysicsMaterial2D Material(string nom, float friccio, float rebot)
        {
            Directory.CreateDirectory(Path.GetFullPath(CarpetaFisica));
            string ruta = CarpetaFisica + "/" + nom + ".physicsMaterial2D";

            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(ruta);
            if (mat == null)
            {
                mat = new PhysicsMaterial2D(nom) { friction = friccio, bounciness = rebot };
                AssetDatabase.CreateAsset(mat, ruta);
                return mat;
            }

            mat.friction = friccio;
            mat.bounciness = rebot;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void AfegeixABuildSettings()
        {
            List<EditorBuildSettingsScene> escenes = EditorBuildSettings.scenes.ToList();

            int index = escenes.FindIndex(e => e.path == RutaEscena);
            if (index < 0)
            {
                int intro = escenes.FindIndex(e => e.path == "Assets/Scenes/" + SceneNames.Intro + ".unity");
                escenes.Insert(intro < 0 ? escenes.Count : intro + 1, new EditorBuildSettingsScene(RutaEscena, true));
            }
            else
                escenes[index].enabled = true;

            EditorBuildSettings.scenes = escenes.ToArray();
        }
    }
}
