using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitClickable : MonoBehaviour
{
    public static event Action<Unit> OnUnitClicked;

    [Header("References")]
    [SerializeField] private Unit unit;

    [Header("Outline Visual")]
    [SerializeField] private GameObject outlineObject;

    private void Awake()
    {
        if (unit == null)
            unit = GetComponent<Unit>();

        SetOutline(false);
    }

    private void OnMouseEnter()
    {
        if (IsPointerOverUI())
            return;

        SetOutline(true);
    }

    private void OnMouseExit()
    {
        SetOutline(false);
    }

    private void OnMouseOver()
    {
        if (IsPointerOverUI())
            return;

        if (unit == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            OnUnitClicked?.Invoke(unit);
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void SetOutline(bool enable)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(enable);
        }
    }
}