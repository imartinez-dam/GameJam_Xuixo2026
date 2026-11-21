using System.Collections.Generic;
using UnityEngine;

namespace Xuixo
{
    public sealed class GameManager : MonoBehaviour
    {
        public const string BowlStage = "bowl";
        public const string StretchStage = "stretch";
        public const string CustardStage = "custard";
        public const string RollStage = "roll";
        public const string FryingStage = "frying";

        public static GameManager Instance { get; private set; }
        public int TotalScore { get; private set; }

        private readonly Dictionary<string, int> stageScores = new Dictionary<string, int>();

        public static void BeginRun()
        {
            if (Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>();

            Instance.stageScores.Clear();
            Instance.TotalScore = 0;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool HasScore(string stage) => stageScores.ContainsKey(stage);

        public bool TryAddScore(string stage, int points)
        {
            if (stageScores.ContainsKey(stage)) return false;
            points = Mathf.Max(0, points);
            stageScores.Add(stage, points);
            TotalScore += points;
            return true;
        }
    }
}
