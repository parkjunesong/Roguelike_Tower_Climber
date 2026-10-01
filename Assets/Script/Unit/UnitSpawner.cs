using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

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

    private Unit SpawnInternal(UnitData data, GameObject prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject obj = Instantiate(prefab, transform);

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
            AdjustBoxCollider(obj, spriteRenderer.sprite);
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

    private void AdjustBoxCollider(GameObject targetObj, Sprite sprite)
    {
        if (sprite == null) return;

        if (!targetObj.TryGetComponent<BoxCollider2D>(out var collider))
        {
            collider = targetObj.AddComponent<BoxCollider2D>();
        }

        collider.size = sprite.bounds.size;
        collider.offset = sprite.bounds.center;
    }
}
