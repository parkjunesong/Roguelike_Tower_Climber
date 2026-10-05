using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [Min(0.01f)] public float visualScale = 1f;
    public int sortingOrderOffset;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Unit Spawn(UnitData data)
    {
        if (data == null) return null;

        var targetPrefab = data.IsEnemy ? enemyPrefab : playerPrefab;
        return SpawnInternal(data, targetPrefab);
    }

    public void ConfigurePlayerVisual(Unit unit)
    {
        if (unit == null || unit.Data.IsEnemy || playerPrefab == null) return;
        unit.transform.localScale = Vector3.Scale(playerPrefab.transform.localScale, new Vector3(visualScale, visualScale, 1f));
        if (unit.TryGetComponent<SpriteRenderer>(out var renderer) && playerPrefab.TryGetComponent<SpriteRenderer>(out var prefabRenderer))
            renderer.sortingOrder = prefabRenderer.sortingOrder + sortingOrderOffset;
    }

    private Unit SpawnInternal(UnitData data, GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, new Vector3(visualScale, visualScale, 1f));

        if (data.Artwork != null)
        {
            if (!obj.TryGetComponent<Animator>(out var animator))
                animator = obj.AddComponent<Animator>();

            animator.runtimeAnimatorController = data.Artwork;
            animator.Rebind();
            animator.Update(0f);
        }

        if (obj.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            spriteRenderer.flipX = data.IsEnemy;
            spriteRenderer.sortingOrder += sortingOrderOffset;
        }

        if (!obj.TryGetComponent<UnitClickable>(out _))
            obj.AddComponent<UnitClickable>();

        if (obj.TryGetComponent<Unit>(out var unit))
        {
            unit.Init(data);
            UnitManager.Instance.Register(unit);
            return unit;
        }

        return null;
    }   
}
