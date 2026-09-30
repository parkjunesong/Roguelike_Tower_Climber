using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleSkillTestController : MonoBehaviour
{
    [Serializable]
    public class SkillButton
    {
        public Button button;
        public SkillDefinition skill;
    }

    [SerializeField] private Button[] allyButtons;
    [SerializeField] private List<SkillButton> skillButtons = new();
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text statusText;

    public Unit SelectedAlly { get; private set; }
    public bool IsTargeting => pendingSkill != null;

    private SkillDefinition pendingSkill;
    private Unit pendingCaster;

    private void Awake()
    {
        for (int i = 0; i < allyButtons.Length; i++)
        {
            int slot = i;
            allyButtons[i].onClick.AddListener(() => SelectAllySlot(slot));
        }
        foreach (var entry in skillButtons)
        {
            var skill = entry.skill;
            entry.button.onClick.AddListener(() => UseSkill(skill));
        }
        cancelButton.onClick.AddListener(CancelTargeting);
        SetStatus("아군을 선택하고 스킬 버튼을 누르세요.");
    }

    private void OnEnable()
    {
        UnitClickable.OnUnitClicked += SelectUnit;
    }

    private void OnDisable()
    {
        UnitClickable.OnUnitClicked -= SelectUnit;
        pendingSkill = null;
        pendingCaster = null;
    }

    private void Update()
    {
        var manager = UnitManager.Instance;
        if (manager == null) return;

        if (IsTargeting && !manager.IsAvailable(pendingCaster))
            CancelTargeting();
        if (SelectedAlly != null && !manager.IsAvailable(SelectedAlly))
            SelectedAlly = null;

        var allies = GetAllies();
        if (SelectedAlly == null && allies.Count > 0)
            SelectAllySlot(0);

        for (int i = 0; i < allyButtons.Length; i++)
        {
            var ally = i < allies.Count ? allies[i] : null;
            allyButtons[i].interactable = ally != null;
            var label = allyButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = ally == null ? $"아군 {i + 1}: 없음" :
                    $"{(ally == SelectedAlly ? "▶ " : "")}아군 {i + 1} · {ally.Data.DisplayName}\nHP {ally.CurrentHP}/{ally.MaxHP}";
        }
        foreach (var entry in skillButtons)
            entry.button.interactable = SelectedAlly != null && entry.skill != null;
        cancelButton.interactable = IsTargeting;

        if (IsTargeting && ((Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)))
            CancelTargeting();
    }

    private List<Unit> GetAllies()
    {
        var allies = new List<Unit>();
        if (UnitManager.Instance == null) return allies;
        foreach (var unit in UnitManager.Instance.GetAllUnits())
            if (!unit.Data.IsEnemy) allies.Add(unit);
        return allies;
    }

    private void SelectAllySlot(int slot)
    {
        var allies = GetAllies();
        if (slot < 0 || slot >= allies.Count) return;
        pendingSkill = null;
        pendingCaster = null;
        SelectedAlly = allies[slot];
        SetStatus($"아군 {slot + 1} 선택 · 사용할 스킬을 누르세요.");
    }

    public void UseSkill(SkillDefinition skill)
    {
        if (skill == null || UnitManager.Instance == null || !UnitManager.Instance.IsAvailable(SelectedAlly))
        {
            SetStatus("사용할 아군과 스킬을 확인하세요.");
            return;
        }

        pendingSkill = skill;
        pendingCaster = SelectedAlly;
        if (skill.RequiresSingleTarget)
        {
            string team = skill.RequiresEnemyTarget ? "적" : "아군";
            SetStatus($"{skill.DisplayName}: {team}을 클릭하세요. (우클릭 / Esc: 취소)");
        }
        else
            ExecuteSkill(null);
    }

    public void SelectUnit(Unit unit)
    {
        if (UnitManager.Instance == null || !UnitManager.Instance.IsAvailable(unit)) return;
        if (IsTargeting)
        {
            if (!pendingSkill.CanSelectTarget(pendingCaster, unit))
            {
                SetStatus("이 스킬에 맞는 진영의 대상을 클릭하세요.");
                return;
            }
            ExecuteSkill(unit);
        }
        else if (!unit.Data.IsEnemy)
            SelectAllySlot(GetAllies().IndexOf(unit));
    }

    private void ExecuteSkill(Unit target)
    {
        bool used = SkillService.UseSkill(pendingSkill, pendingCaster, target);
        SetStatus(used ? $"{pendingSkill.DisplayName} 사용 완료" : "스킬을 사용할 수 없습니다.");
        pendingSkill = null;
        pendingCaster = null;
    }

    public void CancelTargeting()
    {
        pendingSkill = null;
        pendingCaster = null;
        SetStatus("대상 선택을 취소했습니다.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
