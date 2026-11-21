using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Xuixo
{
    internal static class StageTransition
    {
        private static GUIStyle resultStyle;

        internal static IEnumerator Advance(string stage, int points, float pause, string nextScene, MonoBehaviour context)
        {
            GameManager.Instance?.TryAddScore(stage, points);

            yield return new WaitForSecondsRealtime(pause);

            if (string.IsNullOrWhiteSpace(nextScene) || !Application.CanStreamedLevelBeLoaded(nextScene))
            {
                Debug.LogError($"Cannot load '{nextScene}'. Check the scene name and Build Profiles.", context);
                yield break;
            }

            SceneManager.LoadScene(nextScene);
        }

        internal static void DrawResult(string message)
        {
            resultStyle ??= new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter };
            resultStyle.fontSize = Mathf.Clamp(Screen.height / 30, 14, 36);
            float width = Mathf.Min(Screen.width * 0.9f, resultStyle.fontSize * 24f);
            float height = resultStyle.fontSize * 2.5f;
            GUI.Box(new Rect((Screen.width - width) * 0.5f, Screen.height * 0.08f, width, height), message, resultStyle);
        }
    }
}
