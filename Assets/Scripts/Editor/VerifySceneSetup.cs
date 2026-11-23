using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Xuixo.Escena1;
using Xuixo.UI;
using Object = UnityEngine.Object;

namespace Xuixo.EditorTools
{
    public static class VerifySceneSetup
    {
        // Unity -batchmode -quit -projectPath . -executeMethod Xuixo.EditorTools.VerifySceneSetup.Run
        public static void Run()
        {
            string[] flow =
            {
                SceneNames.Intro, SceneNames.BowlEscape, SceneNames.StretchDough, SceneNames.AddCustard,
                SceneNames.RollDough, SceneNames.FryPastry, SceneNames.Credits
            };
            int index = 0;
            foreach (var entry in EditorBuildSettings.scenes)
            {
                if (!entry.enabled) continue;
                var scene = EditorSceneManager.OpenScene(entry.path);
                Require(index < flow.Length && scene.name == flow[index], $"Unexpected build scene {scene.name} at index {index}.");
                Require(AssetDatabase.AssetPathToGUID(entry.path) == entry.guid.ToString(), $"Build scene GUID mismatch at {entry.path}.");
                index++;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                        Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0,
                            $"Missing script on {child.name}.");
                }

                One<EventSystem>();
                One<InputSystemUIInputModule>();
                var quit = One<SortidaJoc>();
                var panel = CheckObject<GameObject>(quit, "dialeg");
                Require(!panel.activeSelf, "The quit panel should start hidden.");
                CheckObject<Button>(quit, "continua");
                CheckObject<Button>(quit, "surt");
                Require(PrefabUtility.IsPartOfPrefabInstance(quit), "The quit dialog must be a prefab instance.");

                if (scene.name != SceneNames.Intro)
                {
                    Require(Count<GameManager>() == 0, "Only the intro should own a GameManager.");
                    var header = GameObject.Find("Stage Header");
                    Require(header && PrefabUtility.IsPartOfPrefabInstance(header), "Missing stage header prefab.");
                }

                if (scene.name != SceneNames.FryPastry && scene.name != SceneNames.Credits)
                {
                    var instructions = One<EntradaInstruccions>();
                    var instructionsPanel = CheckObject<RectTransform>(instructions, "panell");
                    Require(instructionsPanel.childCount > 0, "Instruction panel has no scene content.");
                    CheckObject<RectTransform>(instructions, "inici");
                    Require(instructionsPanel.GetComponent<Image>().sprite != null, "Missing instruction panel sprite.");
                }

                switch (scene.name)
                {
                    case SceneNames.Intro:
                        CheckNextScene(One<ObradorIntroController>(), "nextSceneName", SceneNames.BowlEscape);
                        var manager = One<GameManager>();
                        manager.BeginRun();
                        Require(manager.TryAddScore(GameManager.BowlStage, 20), "Stage points were not recorded.");
                        Require(!manager.TryAddScore(GameManager.BowlStage, 20) && manager.TotalScore == 20,
                            "Stage points should only be counted once.");
                        manager.RecordCustardResult(0);
                        Require(manager.MissedAllCustardDrops, "Custard miss was not recorded.");
                        manager.BeginRun();
                        Require(!manager.HasScore(GameManager.BowlStage) && manager.TotalScore == 0 && !manager.MissedAllCustardDrops,
                            "Run reset left behind old results.");
                        break;
                    case SceneNames.BowlEscape:
                        CheckNextScene(One<RodilloFinal>(), "escenaSeguent", SceneNames.StretchDough);
                        CheckObject<FryerActionCue>(One<HUD_Escena1>(), "_cueBol");
                        var audio = One<AudioEscena1>();
                        CheckObject<AudioSource>(audio, "_sfx");
                        CheckObject<AudioSource>(audio, "_bucle");
                        Require(CheckObject<AudioSource>(audio, "_musica").clip != null, "Missing bowl music.");
                        CheckObject<AnimacioSprite>(One<VfxEscena1>(), "effectPrefab");
                        break;
                    case SceneNames.StretchDough:
                        CheckNextScene(One<MassaScript>(), "escenaSeguent", SceneNames.AddCustard);
                        break;
                    case SceneNames.AddCustard:
                        CheckNextScene(One<MovimentXuixo>(), "escenaSeguent", SceneNames.RollDough);
                        break;
                    case SceneNames.RollDough:
                        var roll = One<EnrotllarMassa>();
                        CheckNextScene(roll, "escenaSeguent", SceneNames.FryPastry);
                        Require(!CheckObject<GameObject>(roll, "panellResultat").activeSelf,
                            "The rolling result should start hidden.");
                        CheckObject<TMPro.TMP_Text>(roll, "textProgresLateral");
                        CheckObject<TMPro.TMP_Text>(roll, "textEtiquetaInici");
                        CheckObject<RectTransform>(roll, "fletxaInici");
                        break;
                    case SceneNames.FryPastry:
                        var fryer = One<FryingFinale>();
                        foreach (string field in new[] { "promptParchment", "hintParchment", "meterParchment" })
                            Require(CheckObject<Image>(fryer, field).sprite != null, $"Missing sprite on {field}.");
                        var bubbles = new SerializedObject(fryer).FindProperty("bubbles");
                        Require(bubbles.arraySize == 7, "Expected seven authored fryer bubbles.");
                        for (int i = 0; i < bubbles.arraySize; i++)
                            Require(bubbles.GetArrayElementAtIndex(i).objectReferenceValue is Image,
                                $"Missing fryer bubble {i + 1}.");
                        break;
                    case SceneNames.Credits:
                        var credits = One<CreditsControls>();
                        Require(!CheckObject<GameObject>(credits, "controls").activeSelf,
                            "Credits hit areas must wait for the video.");
                        CheckObject<AspectRatioFitter>(credits, "videoArea");
                        CheckObject<Button>(credits, "restartButton");
                        CheckObject<Button>(credits, "quitButton");
                        break;
                }

                Debug.Log($"Verified scene setup: {scene.name}");
            }
            Require(index == flow.Length, "Not all scenes are enabled in the build.");
        }

        private static void CheckNextScene(Component component, string property, string expected)
        {
            var field = new SerializedObject(component).FindProperty(property);
            Require(field != null && field.stringValue == expected, $"{component.name}.{property} must point to {expected}.");
        }

        private static int Count<T>() where T : Component =>
            Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        private static T One<T>() where T : Component
        {
            Require(Count<T>() == 1, $"Expected exactly one {typeof(T).Name}.");
            return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        }

        private static T CheckObject<T>(Component component, string property) where T : Object
        {
            var field = new SerializedObject(component).FindProperty(property);
            var value = field?.objectReferenceValue as T;
            Require(value != null, $"Missing {component.name}.{property}.");
            return value;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
