using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaveParallaxDemo
{
    public class ExplorationCharacterInfo : MonoBehaviour
    {
        [SerializeField] private EquipmentController equipmentController;
        [SerializeField] private Image standingImage;
        [SerializeField] private TMP_Text statsText;
        private Unit displayedUnit;
        private readonly int[] stats = new int[6];
        private int lastHP = -1;
        private bool lastDead;
        private UnitClass lastClass;

        private void OnEnable() => Refresh(true);
        private void Update() => Refresh(false);

        private void Refresh(bool force)
        {
            var unit = equipmentController.SelectedUnit;
            if (unit != displayedUnit || force)
            {
                displayedUnit = unit;
                var renderer = unit != null ? unit.GetComponentInChildren<SpriteRenderer>(true) : null;
                standingImage.sprite = unit != null && unit.Data.StandingIllustration != null ? unit.Data.StandingIllustration :
                    renderer != null ? renderer.sprite : null;
                standingImage.enabled = standingImage.sprite != null;
                force = true;
            }
            if (unit == null)
            {
                statsText.text = "선택한 캐릭터 없음";
                return;
            }
            bool changed = force || lastHP != unit.CurrentHP || lastDead != unit.IsDead || lastClass != unit.Equipment.Class;
            for (int i = 0; i < stats.Length; i++)
            {
                int value = unit.Status.GetFinalStat((UnitStatType)i);
                if (stats[i] != value) changed = true;
                stats[i] = value;
            }
            if (!changed) return;
            lastHP = unit.CurrentHP;
            lastDead = unit.IsDead;
            lastClass = unit.Equipment.Class;
            standingImage.color = unit.IsDead ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
            var text = new StringBuilder();
            text.AppendLine($"클래스   {UnitClassNames.GetName(lastClass)}");
            text.AppendLine(unit.IsDead ? "상태   전투 불능" : "상태   생존");
            text.AppendLine($"\n현재 체력   {unit.CurrentHP} / {stats[(int)UnitStatType.HP]}");
            text.AppendLine($"\n공격력   {stats[(int)UnitStatType.AT]}");
            text.AppendLine($"\n방어력   {stats[(int)UnitStatType.DF]}");
            text.AppendLine($"\n치명타 확률   {stats[(int)UnitStatType.CR]}");
            text.AppendLine($"\n치명타 피해량   {stats[(int)UnitStatType.CD]}");
            text.Append($"\n행동 횟수   {stats[(int)UnitStatType.Count]}");
            statsText.text = text.ToString();
        }
    }
}
