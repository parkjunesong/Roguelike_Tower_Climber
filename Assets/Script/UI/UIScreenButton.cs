using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIScreenButton : MonoBehaviour
{
    [SerializeField] private GameObject currentScreen;
    [SerializeField] private GameObject targetScreen;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(ShowScreen);
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(ShowScreen);
    }

    public void ShowScreen()
    {
        if (currentScreen == null || targetScreen == null || currentScreen == targetScreen)
            return;

        targetScreen.SetActive(true);
        currentScreen.SetActive(false);
    }
}
