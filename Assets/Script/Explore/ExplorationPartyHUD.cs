using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaveParallaxDemo
{
    public class ExplorationPartyHUD : MonoBehaviour
    {
        [SerializeField] private ExploreSceneController controller;
        [SerializeField] private Button[] memberButtons;
        [SerializeField] private Image[] portraits;
        [SerializeField] private Image[] hpFills;
        [SerializeField] private TMP_Text[] hpTexts;
        [SerializeField] private TMP_Text[] nameTexts;
        [SerializeField] private Image[] armorIcons;
        [SerializeField] private GameObject[] armorIconFrames;
        private IReadOnlyList<Unit> units;

        private void Awake()
        {
            for (int i = 0; i < memberButtons.Length; i++)
            {
                int slot = i;
                memberButtons[i].onClick.AddListener(() => OpenMember(slot));
            }
            Refresh();
        }

        public void SetUnits(IReadOnlyList<Unit> party)
        {
            units = party;
            for (int i = 0; i < portraits.Length; i++)
            {
                var unit = GetUnit(i);
                var renderer = unit != null ? unit.GetComponentInChildren<SpriteRenderer>(true) : null;
                portraits[i].sprite = unit != null && unit.Data.Portrait != null ? unit.Data.Portrait :
                    renderer != null ? renderer.sprite : null;
                portraits[i].enabled = portraits[i].sprite != null;
            }
            Refresh();
        }

        private Unit GetUnit(int slot) => units != null && slot < units.Count ? units[slot] : null;

        private void OnEnable() => Refresh();

        private void OpenMember(int slot)
        {
            controller.OpenEquipment(GetUnit(slot));
        }

        private void Update() => Refresh();

        private void Refresh()
        {
            for (int i = 0; i < memberButtons.Length; i++)
            {
                var unit = GetUnit(i);
                memberButtons[i].interactable = unit != null && controller.CanOpenEquipment;
                nameTexts[i].text = unit != null ? unit.Data.DisplayName : "빈 슬롯";
                hpFills[i].fillAmount = unit != null ? unit.HPPercent : 0f;
                hpFills[i].color = unit != null && !unit.IsDead ? new Color(0.22f, 0.72f, 0.4f) : Color.gray;
                hpTexts[i].text = unit != null ? $"{unit.CurrentHP} / {unit.MaxHP}" : "—";
                portraits[i].color = unit != null && unit.IsDead ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
                for (int part = 1; part < UnitEquipment.SlotCount; part++)
                {
                    int iconIndex = i * (UnitEquipment.SlotCount - 1) + part - 1;
                    var item = unit != null ? unit.Equipment.GetItem(part) : null;
                    armorIconFrames[iconIndex].SetActive(item != null);
                    armorIcons[iconIndex].sprite = item?.Icon;
                    armorIcons[iconIndex].enabled = item?.Icon != null;
                }
            }
        }
    }
}
