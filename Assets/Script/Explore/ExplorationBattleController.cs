using System;
using UnityEngine;
using UnityEngine.UI;

namespace CaveParallaxDemo
{
    public class ExplorationBattleController : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private UnitManager unitManager;
        [SerializeField] private UnitSpawner spawner;
        [SerializeField] private GameObject battleUI;
        [SerializeField] private Button nextTurnButton;
        // Battle 카메라 높이 20 대비 탐사 카메라 높이 10.8의 비율입니다.
        [SerializeField, Min(0.01f)] private float formationScale = 0.54f;
        public bool IsActive { get; private set; }
        public event Action Started;
        public event Action<bool> Finished;

        private void Awake()
        {
            spawner.visualScale = formationScale;
            spawner.sortingOrderOffset = 30;
            battleManager.BattleFinished += EndBattle;
            nextTurnButton.onClick.AddListener(AdvanceTurn);
            battleUI.SetActive(false);
        }

        public void Begin(BattleData data, Vector2 roomCenter)
        {
            if (IsActive || data == null) return;
            battleManager.Init(data, false, true);
            BeginCombat(roomCenter);
        }

        public void BeginEncounter(UnitData enemy, Vector2 roomCenter)
        {
            if (IsActive || enemy == null || !enemy.IsEnemy) return;
            battleManager.InitEncounter(enemy);
            BeginCombat(roomCenter);
        }

        private void BeginCombat(Vector2 roomCenter)
        {
            IsActive = true;
            Started?.Invoke();
            unitManager.SetFormation(roomCenter + Vector2.up * (2.5f * formationScale), formationScale);
            battleUI.SetActive(true);
            battleManager.BattleStart();
        }

        public void AdvanceTurn()
        {
            if (IsActive && !battleManager.IsFinished && !EffectProcessor.Instance.IsProcessing)
            {
                unitManager.OnTurnEnd();
                turnManager.AdvanceTurn();
            }
        }

        private void Update()
        {
            nextTurnButton.interactable = IsActive && !battleManager.IsFinished && !EffectProcessor.Instance.IsProcessing;
        }

        private void EndBattle(bool victory)
        {
            battleUI.SetActive(false);
            IsActive = false;
            Finished?.Invoke(victory);
        }

        private void OnDestroy()
        {
            if (battleManager != null) battleManager.BattleFinished -= EndBattle;
            if (nextTurnButton != null) nextTurnButton.onClick.RemoveListener(AdvanceTurn);
        }
    }
}
