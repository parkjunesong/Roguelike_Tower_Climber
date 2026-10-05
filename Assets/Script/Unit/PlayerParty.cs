using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerParty : MonoBehaviour
{
    public static PlayerParty Instance { get; private set; }
    private readonly List<Unit> units = new();
    public IReadOnlyList<Unit> Units => units;
    public bool IsInitialized => units.Count > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instance = null;

    public const int MaxMembers = 3;

    // 편성 화면에서 생성한 런타임 유닛을 전달받습니다.
    public static PlayerParty Create(IReadOnlyList<Unit> members)
    {
        if (members == null || members.Count == 0 || members.Count > MaxMembers) return null;
        var unique = new HashSet<Unit>();
        foreach (var unit in members)
            if (unit == null || unit.Data == null || unit.Data.IsEnemy || unit.IsDead ||
                !unique.Add(unit) || (Instance != null && Instance.units.Contains(unit))) return null;
        if (Instance == null) new GameObject("Player Party").AddComponent<PlayerParty>();
        Instance.ResetParty();
        foreach (var unit in members)
        {
            unit.KeepAfterDeath = true;
            unit.gameObject.SetActive(false);
            unit.transform.SetParent(Instance.transform, true);
            unit.name = $"파티 {Instance.units.Count + 1} · {unit.Data.DisplayName}";
            Instance.units.Add(unit);
        }
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void Deploy(UnitSpawner spawner, UnitManager manager)
    {
        foreach (var unit in units)
        {
            if (unit == null || unit.IsDead) continue;
            spawner.ConfigurePlayerVisual(unit);
            unit.gameObject.SetActive(true);
            manager.Register(unit);
        }
    }

    public void Suspend()
    {
        foreach (var unit in units)
            if (unit != null) unit.gameObject.SetActive(false);
    }

    // 새 게임 등 명시적인 초기화에서만 호출합니다.
    public void ResetParty()
    {
        Suspend();
        foreach (var unit in units)
            if (unit != null) Destroy(unit.gameObject);
        units.Clear();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Suspend();

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }
}
