using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyFormationController : MonoBehaviour
{
    [SerializeField] private List<UnitData> availablePlayers = new();
    [SerializeField] private UnitSpawner spawner;
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text statusText;
    private readonly int[] selected = new int[PlayerParty.MaxMembers];
    private bool loading;

    private void Awake()
    {
        availablePlayers.RemoveAll(data => data == null || data.IsEnemy);
        for (int i = 0; i < selected.Length; i++)
        {
            selected[i] = -1;
            int slot = i;
            slotButtons[i].onClick.AddListener(() => CycleSlot(slot));
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
        }
        confirmButton.interactable = count > 0 && ExplorationEntry.PendingMap != null && !loading;
        statusText.text = ExplorationEntry.PendingMap == null ? "시나리오에서 탐사를 선택해 주세요." :
            $"파티 {count} / {PlayerParty.MaxMembers}명 · 최소 한 명을 편성하세요.\n슬롯을 클릭하면 캐릭터 → 빈 슬롯 순서로 변경됩니다.";
    }

    private void Confirm()
    {
        if (loading || !confirmButton.interactable) return;
        loading = true;
        Refresh();
        var members = new List<Unit>();
        foreach (int index in selected)
        {
            if (index < 0) continue;
            var unit = spawner.Spawn(availablePlayers[index]);
            if (unit == null) break;
            unit.gameObject.SetActive(false);
            members.Add(unit);
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
