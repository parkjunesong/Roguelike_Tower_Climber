using UnityEngine;
using UnityEngine.UI;

public class UnitActionCounter : MonoBehaviour
{
    [SerializeField] private Text counterText;

    private Color defaultColor;

    private void Awake()
    {
        if (counterText != null)
            defaultColor = counterText.color;
    }

    public void SetCounter(int remainingTurns)
    {
        if (counterText == null)
            return;

        counterText.text = remainingTurns.ToString();

        if (remainingTurns == 0)
            counterText.color = Color.red;
        else
            counterText.color = defaultColor;
    }
}
