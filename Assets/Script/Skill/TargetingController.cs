using UnityEngine;

public class TargetingController : MonoBehaviour
{
    public static TargetingController Instance { get; private set; }

    private SkillDefinition selectedItem;
    private Unit playerUnit;

    public bool IsTargetingMode => selectedItem != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        UnitClickable.OnUnitClicked += HandleUnitClicked;
    }

    private void OnDisable()
    {
        UnitClickable.OnUnitClicked -= HandleUnitClicked;
    }

    private void Update()
    {
        if (IsTargetingMode && Input.GetMouseButtonDown(1))
        {
            CancelTargeting();
        }
    }

    public void StartItemUse(Unit user, SkillDefinition item)
    {
        if (user == null || item == null) return;

        playerUnit = user;
        selectedItem = item;

        if (RequiresSingleTarget(item))
        {
            // Ŀ�� ���� �� Ÿ���� ���� ����
            Debug.Log($"[{item.DisplayName}] ���(��)�� Ŭ���ϼ���. (��Ŭ��: ���)");
        }
        else
        {
            // Self / ������ ���� Ŭ�� ��� ���� ��� ���
            ExecuteUseItem(null);
        }
    }

    private void HandleUnitClicked(Unit clickedUnit)
    {
        // UnitClickable���� ������ Ŭ���Ǿ��� �� ����
        if (!IsTargetingMode) return;

        ExecuteUseItem(clickedUnit);
    }

    private void ExecuteUseItem(Unit target)
    {
        SkillService.UseSkill(selectedItem, playerUnit, target);
        CancelTargeting();
    }

    public void CancelTargeting()
    {
        selectedItem = null;
        playerUnit = null;
    }

    private bool RequiresSingleTarget(SkillDefinition item)
    {
        foreach (var binding in item.Effects)
        {
            if (binding != null && binding.Target == EffectTarget.Single)
            {
                return true;
            }
        }
        return false;
    }
}