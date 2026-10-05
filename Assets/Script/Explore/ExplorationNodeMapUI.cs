using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CaveParallaxDemo
{
    public class ExplorationNodeMapUI : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private ExploreSceneController controller;
        [SerializeField] private GameObject panel;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform mapContent;
        [SerializeField, Min(1f)] private float maximumZoom = 2.5f;
        private float minimumZoom;
        private bool dragging;
        private Vector2 previousDragPosition;
        [SerializeField] private Button nodeTemplate;
        [SerializeField] private RectTransform connectionsRoot;
        [SerializeField] private Image connectionTemplate;
        [SerializeField] private TMP_Text description;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button finishButton;
        private readonly Dictionary<Vector2Int, Button> buttons = new();
        private readonly List<(Vector2Int from, Vector2Int to, Image image)> lines = new();

        public bool IsOpen => panel.activeSelf;

        private void Awake()
        {
            closeButton.onClick.AddListener(controller.CloseMap);
            finishButton.onClick.AddListener(controller.CompleteExploration);
            nodeTemplate.gameObject.SetActive(false);
            connectionTemplate.gameObject.SetActive(false);
            panel.SetActive(false);
        }

        public void Initialize(ExplorationMapLayout layout)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = layout.Size;
            grid.spacing = Vector2.one * 32f;
            grid.cellSize = Vector2.one * 160f;
            float extent = layout.Size * grid.cellSize.x + (layout.Size - 1) * grid.spacing.x + 80f;
            mapContent.sizeDelta = Vector2.one * extent;
            var viewportSize = viewport.rect.size;
            minimumZoom = Mathf.Min(1f, viewportSize.x / extent, viewportSize.y / extent);
            mapContent.localScale = Vector3.one;
            for (int y = layout.Size - 1; y >= 0; y--)
            for (int x = 0; x < layout.Size; x++)
            {
                var position = new Vector2Int(x, y);
                var button = Instantiate(nodeTemplate, grid.transform);
                button.gameObject.SetActive(true);
                button.name = $"Node {x}, {y}";
                button.GetComponentInChildren<TMP_Text>().fontSize = 24f;
                button.onClick.AddListener(() => controller.MoveToNode(position));
                buttons.Add(position, button);
            }
            foreach (var node in layout.Nodes)
            foreach (var target in node.connections)
            {
                // 양방향 길을 한 번만 그립니다.
                if (node.position.y > target.y || (node.position.y == target.y && node.position.x > target.x)) continue;
                var from = NodePosition(node.position, layout.Size);
                var to = NodePosition(target, layout.Size);
                var line = Instantiate(connectionTemplate, connectionsRoot);
                line.gameObject.SetActive(false);
                var arrow = line.transform.Find("Direction");
                if (arrow != null) arrow.gameObject.SetActive(false);
                line.rectTransform.anchoredPosition = (from + to) * 0.5f;
                float nodeSize = node.position.x == target.x ? grid.cellSize.y : grid.cellSize.x;
                line.rectTransform.sizeDelta = new Vector2(Mathf.Max(0f, Vector2.Distance(from, to) - nodeSize), 3f);
                line.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
                lines.Add((node.position, target, line));
            }
        }

        private Vector2 NodePosition(Vector2Int position, int size) =>
            new Vector2((position.x - (size - 1) * 0.5f) * (grid.cellSize.x + grid.spacing.x),
                (position.y - (size - 1) * 0.5f) * (grid.cellSize.y + grid.spacing.y));

        public void Show()
        {
            var layout = controller.Layout;
            var current = controller.CurrentNode;
            foreach (var entry in buttons)
            {
                var node = layout.GetNode(entry.Key);
                bool selected = entry.Key == current.position;
                bool visible = controller.HasVisited(entry.Key) || layout.IsVisible(current.position, entry.Key);
                bool available = layout.CanMove(current.position, entry.Key);
                entry.Value.interactable = available;
                entry.Value.GetComponent<Image>().color = !visible ? new Color(0.08f, 0.09f, 0.12f) :
                    selected ? new Color(0.2f, 0.65f, 1f) : available ? new Color(0.2f, 0.65f, 0.35f) :
                    node.fixedType == FixedNodeType.Boss ? new Color(0.65f, 0.25f, 0.25f) : new Color(0.25f, 0.27f, 0.32f);
                entry.Value.GetComponentInChildren<TMP_Text>().text = !visible ? string.Empty :
                    $"({entry.Key.x}, {entry.Key.y})\n{node.DisplayName}\n" +
                    (selected ? "현재 방" : controller.IsResolved(entry.Key) ? "해결함" : available ? "이동 가능" : controller.HasVisited(entry.Key) ? "방문함" : "고정 노드");
            }
            foreach (var line in lines)
                line.image.gameObject.SetActive(line.from == current.position || line.to == current.position ||
                    controller.HasTraversedPath(line.from, line.to));
            description.text = $"현재 방: {current.DisplayName}\n녹색: 이동 가능 / 휠: 확대·축소 / 좌클릭 드래그: 지도 이동";
            finishButton.gameObject.SetActive(controller.CanCompleteExploration);
            panel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            mapContent.anchoredPosition = -NodePosition(current.position, layout.Size) * mapContent.localScale.x;
            ClampMapPosition();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!IsOpen || minimumZoom <= 0f ||
                !RectTransformUtility.RectangleContainsScreenPoint(viewport, eventData.position, eventData.enterEventCamera)) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position,
                eventData.enterEventCamera, out var pointer)) return;
            float previous = mapContent.localScale.x;
            float zoom = Mathf.Clamp(previous * Mathf.Pow(1.15f, eventData.scrollDelta.y), minimumZoom, maximumZoom);
            // 커서가 가리키는 지도 위치를 기준으로 확대합니다.
            mapContent.anchoredPosition = pointer - (pointer - mapContent.anchoredPosition) * (zoom / previous);
            mapContent.localScale = new Vector3(zoom, zoom, 1f);
            ClampMapPosition();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsOpen || eventData.button != PointerEventData.InputButton.Left ||
                !RectTransformUtility.RectangleContainsScreenPoint(viewport, eventData.pressPosition, eventData.pressEventCamera)) return;
            dragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.pressPosition,
                eventData.pressEventCamera, out previousDragPosition);
            if (dragging) eventData.eligibleForClick = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging || !IsOpen || eventData.button != PointerEventData.InputButton.Left) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position,
                eventData.pressEventCamera, out var position)) return;
            eventData.eligibleForClick = false;
            mapContent.anchoredPosition += position - previousDragPosition;
            previousDragPosition = position;
            ClampMapPosition();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) dragging = false;
        }

        private void ClampMapPosition()
        {
            var overflow = (mapContent.rect.size * mapContent.localScale.x - viewport.rect.size) * 0.5f;
            var position = mapContent.anchoredPosition;
            position.x = Mathf.Clamp(position.x, -Mathf.Max(0f, overflow.x), Mathf.Max(0f, overflow.x));
            position.y = Mathf.Clamp(position.y, -Mathf.Max(0f, overflow.y), Mathf.Max(0f, overflow.y));
            mapContent.anchoredPosition = position;
        }

        public void Hide()
        {
            dragging = false;
            panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(controller.CloseMap);
            if (finishButton != null) finishButton.onClick.RemoveListener(controller.CompleteExploration);
        }
    }
}
