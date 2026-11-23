using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Xuixo.Escena1;
using Xuixo.UI;
using Object = UnityEngine.Object;

namespace Xuixo.EditorTools
{
    public static class AuthorSceneMigration
    {
        private const string PrefabFolder = ContentPaths.Shared + "UI/Prefabs";
        private const string BowlPrefabFolder = ContentPaths.Bowl + "Prefabs";
        private const string Art = ContentPaths.UiArt;

        public static void Run()
        {
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory(BowlPrefabFolder);
            AssetDatabase.Refresh();
            InstruccionsUI.FonsPanell = ImportSprite("panell_9slice");
            InstruccionsUI.FonsXip = ImportSprite("boto_normal");
            InstruccionsUI.Hover = ImportSprite("boto_hover");
            InstruccionsUI.Pressed = ImportSprite("boto_premut");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject quit = new GameObject("Quit Dialog", typeof(SortidaJoc));
            Invoke(quit.GetComponent<SortidaJoc>(), "AuthorDialog", InstruccionsUI.FonsPanell, InstruccionsUI.FonsXip);
            GameObject quitPrefab = SavePrefab(quit);
            GameObject instructionPrefab = SavePrefab(InstruccionsUI.CreaBarra());
            GameObject headerPrefab = SavePrefab(CreateHeader());
            GameObject effectPrefab = SavePrefab(new GameObject("Bowl Sprite Effect", typeof(SpriteRenderer), typeof(AnimacioSprite)), BowlPrefabFolder);

            int stage = -1;
            foreach (EditorBuildSettingsScene setup in EditorBuildSettings.scenes)
            {
                if (!setup.enabled) continue;
                Scene scene = EditorSceneManager.OpenScene(setup.path, OpenSceneMode.Single);
                InstruccionsUI.AplicaEstilBotons();
                if (Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
                    new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                PrefabUtility.InstantiatePrefab(quitPrefab, scene);
                var instruction = (GameObject)PrefabUtility.InstantiatePrefab(instructionPrefab, scene);
                if (!InstruccionsUI.AuthorSteps(scene.name, instruction.transform.Find("Panell")))
                    Object.DestroyImmediate(instruction);

                if (stage >= 0)
                {
                    var header = (GameObject)PrefabUtility.InstantiatePrefab(headerPrefab, scene);
                    var current = header.transform.Find("Stage " + (Mathf.Min(stage, 4) + 1)).GetComponent<RawImage>();
                    current.texture = Texture("indicador_actiu");
                    PrefabUtility.RecordPrefabInstancePropertyModifications(current);
                }

                switch (scene.name)
                {
                    case SceneNames.Intro:
                        new GameObject("GameManager", typeof(GameManager));
                        break;
                    case SceneNames.BowlEscape:
                        HUD_Escena1 hud = Find<HUD_Escena1>();
                        Invoke(hud, "CreaIndicadorBol");
                        AudioEscena1 audio = Find<AudioEscena1>();
                        SetReferences(audio,
                            ("_sfx", CreateAudio(audio.transform, "One-shot Effects", false, 1f, null)),
                            ("_bucle", CreateAudio(audio.transform, "Dough Loop", true, 1f, null)),
                            ("_musica", CreateAudio(audio.transform, "Music", true, 0.30f,
                                AssetDatabase.LoadAssetAtPath<AudioClip>(ContentPaths.BowlAudio + "musica_joc.wav"))));
                        SetReferences(Find<VfxEscena1>(), ("effectPrefab", effectPrefab.GetComponent<AnimacioSprite>()));
                        break;
                    case SceneNames.RollDough:
                        EnrotllarMassa rolling = Find<EnrotllarMassa>();
                        Invoke(rolling, "PreparaPanells", InstruccionsUI.FonsPanell);
                        var model = new SerializedObject(rolling);
                        Invoke(rolling, "DecoraMarca", model.FindProperty("marcaInici").objectReferenceValue, true);
                        Invoke(rolling, "DecoraMarca", model.FindProperty("marcaFinal").objectReferenceValue, false);
                        break;
                    case SceneNames.FryPastry:
                        Invoke(Find<FryingFinale>(), "AuthorVisuals", InstruccionsUI.FonsPanell);
                        break;
                    case SceneNames.Credits:
                        Invoke(Find<CreditsControls>(), "AuthorControls");
                        break;
                }
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new IOException("Could not save " + setup.path);
                Debug.Log("Authored " + setup.path);
                stage++;
            }
            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateHeader()
        {
            var root = new GameObject("Stage Header", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 30;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;
            RawImage logo = Raw(root.transform, "Logo", Texture("logo_xuixo"), Vector2.up, new Vector2(184f, -101f), new Vector2(300f, 166f));
            var fit = logo.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            // Keep the logo artwork's aspect ratio within the original 300 × 166 box.
            Object.DestroyImmediate(fit);
            float scale = Mathf.Min(300f / logo.texture.width, 166f / logo.texture.height);
            logo.rectTransform.sizeDelta = new Vector2(logo.texture.width, logo.texture.height) * scale;
            Vector2 topRight = Vector2.one;
            for (int i = 0; i < 5; i++)
                Raw(root.transform, "Stage " + (i + 1), Texture("indicador_inactiu"), topRight,
                    new Vector2(-175f + 32f * i, -41f), new Vector2(22f, 22f));
            return root;
        }

        private static RawImage Raw(Transform parent, string name, Texture texture, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
            var rect = image.rectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            image.texture = texture;
            image.raycastTarget = false;
            return image;
        }

        private static AudioSource CreateAudio(Transform parent, string name, bool loop, float volume, AudioClip clip)
        {
            var source = new GameObject(name, typeof(AudioSource)).GetComponent<AudioSource>();
            source.transform.SetParent(parent, false);
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = volume;
            source.clip = clip;
            return source;
        }

        private static GameObject SavePrefab(GameObject go, string folder = PrefabFolder)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{folder}/{go.name}.prefab");
            Object.DestroyImmediate(go);
            return prefab != null ? prefab : throw new IOException("Could not save prefab.");
        }

        private static Texture2D Texture(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>(Art + name + ".png");

        private static Sprite ImportSprite(string name)
        {
            string path = Art + name + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new IOException("Could not import " + path);
        }

        private static T Find<T>() where T : Component =>
            Object.FindFirstObjectByType<T>() ?? throw new InvalidOperationException("Missing " + typeof(T).Name);

        private static void Invoke(Component component, string method, params object[] args)
        {
            component.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(component, args);
            EditorUtility.SetDirty(component);
        }

        private static void SetReferences(Component component, params (string property, Object value)[] references)
        {
            var so = new SerializedObject(component);
            foreach (var (property, value) in references)
                so.FindProperty(property).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
