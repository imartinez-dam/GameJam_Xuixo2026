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
        public bool MissedAllCustardDrops { get; private set; }

        private readonly Dictionary<string, int> stageScores = new Dictionary<string, int>();

        [Header("Stage point limits")]
        [SerializeField, Min(0)] private int bowlPoints = 20;
        [SerializeField, Min(0)] private int stretchPoints = 20;
        [SerializeField, Min(0)] private int custardPoints = 20;
        [SerializeField, Min(0)] private int rollPoints = 20;
        [SerializeField, Min(0)] private int fryingPoints = 20;

        [Header("Ending thresholds")]
        [SerializeField, Min(1)] private int wonkyMinScore = 40;
        [SerializeField, Min(2)] private int wellMadeMinScore = 70;

        public int WonkyMinScore => wonkyMinScore;
        public int WellMadeMinScore => wellMadeMinScore;

        public void BeginRun()
        {
            stageScores.Clear();
            TotalScore = 0;
            MissedAllCustardDrops = false;
        }

        public static int PointLimit(string stage) => Instance == null ? 20 : stage switch
        {
            BowlStage => Instance.bowlPoints,
            StretchStage => Instance.stretchPoints,
            CustardStage => Instance.custardPoints,
            RollStage => Instance.rollPoints,
            FryingStage => Instance.fryingPoints,
            _ => 0
        };

        private void OnValidate()
        {
            wonkyMinScore = Mathf.Clamp(wonkyMinScore, 1, int.MaxValue - 1);
            wellMadeMinScore = Mathf.Max(wonkyMinScore + 1, wellMadeMinScore);
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

        public void RecordCustardResult(int hits) => MissedAllCustardDrops = hits == 0;

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
