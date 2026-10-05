using System.Collections.Generic;
using UnityEngine;

public class InventoryTestInitializer : MonoBehaviour
{
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private List<ItemDefinition> items = new();

    private void Start()
    {
        if (inventoryController == null)
            inventoryController = GetComponent<InventoryController>();

        if (inventoryController == null || inventoryController.Inventory == null)
        {
            Debug.LogWarning("테스트 아이템을 추가할 InventoryController를 지정하세요.", this);
            return;
        }

        foreach (var item in items)
        {
            if (item == null) continue;
            if (!inventoryController.AddItem(item))
            {
                Debug.LogWarning($"인벤토리 용량이 부족해 {item.DisplayName}부터 테스트 아이템을 추가하지 못했습니다.", this);
                break;
            }
        }
    }
}
