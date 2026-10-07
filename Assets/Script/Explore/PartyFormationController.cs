using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyFormationController : MonoBehaviour
{
    [SerializeField] private List<UnitData> availablePlayers = new();
    [SerializeField] private UnitSpawner spawner;
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private List<ItemDefinition> startingWeapons = new();
    [SerializeField] private Button[] weaponButtons;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text statusText;
    private readonly int[] selected = new int[PlayerParty.MaxMembers];
    private readonly int[] selectedWeapons = new int[PlayerParty.MaxMembers];
    private bool loading;

    private void Awake()
    {
        availablePlayers.RemoveAll(data => data == null || data.IsEnemy);
        startingWeapons.RemoveAll(data => data == null || data.ActionType != ItemActionType.Equip ||
            data.WeaponClass == UnitClass.None);
        for (int i = 0; i < selected.Length; i++)
        {
            selected[i] = -1;
            int slot = i;
            slotButtons[i].onClick.AddListener(() => CycleSlot(slot));
            weaponButtons[i].onClick.AddListener(() => CycleWeapon(slot));
        }
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(ExplorationEntry.Cancel);
        Refresh();
    }

    private void CycleSlot(int slot)
    {
        if (loading) return;
        selected[slot]++;
        if (selected[slot] >= availablePlayers.Count) selected[slot] = -1;
        Refresh();
    }

    public void Open()
    {
        loading = false;
        for (int i = 0; i < selected.Length; i++)
        {
            selected[i] = -1;
            selectedWeapons[i] = 0;
        }
        Refresh();
    }

    private void CycleWeapon(int slot)
    {
        if (loading || selected[slot] < 0 || startingWeapons.Count == 0) return;
        selectedWeapons[slot] = (selectedWeapons[slot] + 1) % startingWeapons.Count;
        Refresh();
    }

    private void Refresh()
    {
        int count = 0;
        for (int i = 0; i < selected.Length; i++)
        {
            var data = selected[i] >= 0 ? availablePlayers[selected[i]] : null;
            if (data != null) count++;
            slotButtons[i].GetComponentInChildren<TMP_Text>().text =
                $"슬롯 {i + 1}\n{(data != null ? data.DisplayName : "비어 있음")}\n클릭하여 변경";
            slotButtons[i].interactable = availablePlayers.Count > 0 && !loading;
            var weapon = startingWeapons.Count > 0 ? startingWeapons[selectedWeapons[i]] : null;
            weaponButtons[i].interactable = data != null && weapon != null && !loading;
            weaponButtons[i].GetComponentInChildren<TMP_Text>().text = data == null ? "캐릭터를 선택하세요" :
                weapon == null ? "시작 무기 미설정" : $"{weapon.DisplayName} · {UnitClassNames.GetName(weapon.WeaponClass)}\n클릭하여 무기 변경";
        }
        confirmButton.interactable = count > 0 && startingWeapons.Count > 0 && ExplorationEntry.PendingMap != null && !loading;
        statusText.text = ExplorationEntry.PendingMap == null ? "시나리오에서 탐사를 선택해 주세요." :
            startingWeapons.Count == 0 ? "편성 화면에 클래스별 시작 무기를 설정하세요." :
            $"파티 {count} / {PlayerParty.MaxMembers}명 · 최소 한 명을 편성하세요.\n캐릭터와 시작 무기를 선택하세요. 탐사 중에는 선택한 클래스의 무기만 장착 가능합니다.";
    }

    private void Confirm()
    {
        if (loading || !confirmButton.interactable) return;
        loading = true;
        Refresh();
        var members = new List<Unit>();
        for (int i = 0; i < selected.Length; i++)
        {
            int index = selected[i];
            if (index < 0) continue;
            var unit = spawner.Spawn(availablePlayers[index]);
            if (unit == null) break;
            unit.gameObject.SetActive(false);
            members.Add(unit);
            var gear = new InventoryGrid(1, 1);
            var weapon = new ItemInstance(startingWeapons[selectedWeapons[i]]);
            gear.TryAdd(weapon);
            if (!unit.Equipment.TryEquip(weapon, gear, out _)) break;
        }
        int expected = 0;
        foreach (int index in selected) if (index >= 0) expected++;
        if (members.Count == expected && ExplorationEntry.Confirm(members)) return;
        foreach (var unit in members) Destroy(unit.gameObject);
        loading = false;
        Refresh();
        statusText.text = "파티를 생성할 수 없습니다. 플레이어 프리팹 설정을 확인하세요.";
    }
}
