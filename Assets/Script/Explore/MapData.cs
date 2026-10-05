using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveParallaxDemo
{
    public enum MapNodeType { Empty, Fixed, Event }
    public enum FixedNodeType { None, Start, Boss }

    [Serializable]
    public class NodeEventChoice
    {
        public string label;
        [Tooltip("맵의 잡몹 목록에서 하나를 선택해 현재 방에서 전투합니다.")]
        public bool randomEncounter;
        [Tooltip("기존 고정 전투 입력입니다. Random Encounter가 켜져 있으면 잡몹 목록을 사용합니다.")]
        public BattleData battle;
    }

    [Serializable]
    public class NodeEventData
    {
        public string eventId;
        public string title;
        [TextArea] public string description;
        public List<NodeEventChoice> choices = new();
    }

    public class MapNode
    {
        public Vector2Int position;
        public MapNodeType type;
        public FixedNodeType fixedType;
        public NodeEventData eventData;
        public readonly List<Vector2Int> connections = new();
        public string DisplayName => fixedType == FixedNodeType.Start ? "시작방" :
            fixedType == FixedNodeType.Boss ? "보스방" : type == MapNodeType.Event ? eventData.title : "빈 방";
    }

    [CreateAssetMenu(menuName = "Game/Data/Map")]
    public class MapData : ScriptableObject
    {
        [Min(2)] public int size = 3;
        public ExplorationMap mapPrefab;
        [Range(0f, 1f)] public float extraConnectionChance = 0.2f;
        [Range(0f, 1f)] public float eventNodeChance = 0.4f;
        public bool useFixedSeed;
        public int seed;
        public BattleData bossBattle;
        public List<UnitData> encounterEnemies = new();
        public List<NodeEventData> events = new();

        public UnitData GetRandomEncounterEnemy() => encounterEnemies != null && encounterEnemies.Count > 0
            ? encounterEnemies[UnityEngine.Random.Range(0, encounterEnemies.Count)] : null;

        public ExplorationMapLayout Generate(int? overrideSeed = null)
        {
            if (!Validate(out var message)) throw new InvalidOperationException(message);
            int runSeed = overrideSeed ?? (useFixedSeed ? seed : UnityEngine.Random.Range(0, int.MaxValue));
            return new ExplorationMapLayout(this, runSeed);
        }

        public bool Validate(out string message)
        {
            if (size < 2 || mapPrefab == null || !IsValidBattle(bossBattle))
            {
                message = "맵 크기(최소 2), 방 프리팹, 유효한 보스 전투 데이터를 지정하세요.";
                return false;
            }
            if (eventNodeChance > 0f && (events == null || events.Count == 0))
            {
                message = "이벤트 노드에 사용할 이벤트 데이터를 지정하세요.";
                return false;
            }
            if (encounterEnemies != null)
            foreach (var enemy in encounterEnemies)
                if (enemy == null || !enemy.IsEnemy)
                {
                    message = "잡몹 목록에는 적 UnitData를 지정하세요.";
                    return false;
                }
            if (events != null)
            foreach (var data in events)
            {
                if (data == null || data.choices == null || data.choices.Count == 0)
                {
                    message = "이벤트에는 선택지가 최소 한 개 필요합니다.";
                    return false;
                }
                foreach (var choice in data.choices)
                {
                    if (choice == null || string.IsNullOrWhiteSpace(choice.label) ||
                        (choice.randomEncounter && (encounterEnemies == null || encounterEnemies.Count == 0)) ||
                        (!choice.randomEncounter && choice.battle != null && !IsValidBattle(choice.battle)))
                    {
                        message = "이벤트 선택지의 이름과 전투 데이터를 확인하세요.";
                        return false;
                    }
                }
            }
            message = null;
            return true;
        }

        private static bool IsValidBattle(BattleData data)
        {
            if (data == null || data.Waves == null || data.Waves.Count == 0)
                return false;
            foreach (var wave in data.Waves)
            {
                if (wave == null || wave.UnitData == null || wave.UnitData.Count == 0) return false;
                foreach (var enemy in wave.UnitData) if (enemy == null || !enemy.IsEnemy) return false;
            }
            return true;
        }
    }
}
