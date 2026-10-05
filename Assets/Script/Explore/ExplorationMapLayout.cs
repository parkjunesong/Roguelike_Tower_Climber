using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveParallaxDemo
{
    // 탐사 한 번의 맵입니다. 생성된 노드와 길은 원본 SO에 저장하지 않습니다.
    public class ExplorationMapLayout
    {
        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        private readonly Dictionary<Vector2Int, MapNode> nodes = new();
        public int Size { get; }
        public int Seed { get; }
        public Vector2Int Start { get; }
        public Vector2Int Boss { get; }
        public IEnumerable<MapNode> Nodes => nodes.Values;

        public ExplorationMapLayout(MapData data, int seed)
        {
            Size = data.size;
            Seed = seed;
            var random = new System.Random(seed);
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var position = new Vector2Int(x, y);
                nodes.Add(position, new MapNode { position = position });
            }
            Start = new Vector2Int(random.Next(Size), random.Next(Size));
            var stack = new Stack<Vector2Int>();
            var visited = new HashSet<Vector2Int> { Start };
            stack.Push(Start);
            // 랜덤 깊이 우선 탐색으로 모든 방을 연결한 뒤 추가 길을 만듭니다.
            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var candidates = new List<Vector2Int>();
                foreach (var direction in Directions)
                {
                    var target = current + direction;
                    if (nodes.ContainsKey(target) && !visited.Contains(target)) candidates.Add(target);
                }
                if (candidates.Count == 0) { stack.Pop(); continue; }
                var next = candidates[random.Next(candidates.Count)];
                Connect(current, next);
                visited.Add(next);
                stack.Push(next);
            }
            foreach (var node in nodes.Values)
            foreach (var direction in new[] { Vector2Int.up, Vector2Int.right })
            {
                var target = node.position + direction;
                if (nodes.ContainsKey(target) && !node.connections.Contains(target) && random.NextDouble() < data.extraConnectionChance)
                    Connect(node.position, target);
            }
            // 시작방에서 실제 길을 따라 가장 먼 방을 보스방으로 선택합니다.
            var distances = new Dictionary<Vector2Int, int> { [Start] = 0 };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(Start);
            var farthest = Start;
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var next in nodes[current].connections)
                {
                    if (distances.ContainsKey(next)) continue;
                    distances[next] = distances[current] + 1;
                    if (distances[next] > distances[farthest]) farthest = next;
                    queue.Enqueue(next);
                }
            }
            Boss = farthest;
            foreach (var node in nodes.Values)
            {
                if (node.position == Start || node.position == Boss)
                {
                    node.type = MapNodeType.Fixed;
                    node.fixedType = node.position == Start ? FixedNodeType.Start : FixedNodeType.Boss;
                }
                else if (data.events != null && data.events.Count > 0 && random.NextDouble() < data.eventNodeChance)
                {
                    node.type = MapNodeType.Event;
                    node.eventData = data.events[random.Next(data.events.Count)];
                }
            }
        }

        private void Connect(Vector2Int from, Vector2Int to)
        {
            nodes[from].connections.Add(to);
            nodes[to].connections.Add(from);
        }

        public MapNode GetNode(Vector2Int position) => nodes.TryGetValue(position, out var node) ? node : null;

        public bool CanMove(Vector2Int from, Vector2Int to) =>
            Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y) == 1 &&
            nodes.TryGetValue(from, out var node) && node.connections.Contains(to);

        public bool IsVisible(Vector2Int current, Vector2Int target) =>
            nodes.TryGetValue(target, out var node) &&
            (node.type == MapNodeType.Fixed || current == target || CanMove(current, target));
    }
}
