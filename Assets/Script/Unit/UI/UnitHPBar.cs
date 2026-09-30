using UnityEngine;
using UnityEngine.UI;

public class UnitHPBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void Init(int maxHP, int currentHP)
    {
        slider.maxValue = maxHP;
        slider.value = currentHP;
    }

    public void SetHP(int currentHP)
    {
        slider.value = currentHP;
    }
}