using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    private void Update()
    {
        var mouse = Mouse.current;
        var camera = Camera.main;
        if (mouse == null || camera == null || unit == null ||
            !UnitManager.Instance.IsAvailable(unit) || IsPointerOverUI())
        {
            SetOutline(false);
            return;
        }

        var hit = Physics2D.GetRayIntersection(camera.ScreenPointToRay(mouse.position.ReadValue()));
        bool hovered = hit.collider != null && hit.collider.GetComponentInParent<Unit>() == unit;
        SetOutline(hovered);
        if (hovered && mouse.leftButton.wasPressedThisFrame)
            OnUnitClicked?.Invoke(unit);
    }

    private void OnDisable()
    {
        SetOutline(false);
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void SetOutline(bool enable)
    {
        if (outlineObject != null && outlineObject.activeSelf != enable)
            outlineObject.SetActive(enable);
    }
}