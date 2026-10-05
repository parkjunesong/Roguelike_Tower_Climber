using UnityEngine;

namespace CaveParallaxDemo
{
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(100)]
    public sealed class SideScrollerCamera : MonoBehaviour
    {
        public Transform player;
        [Range(0f, 1f)] public float playerScreenX = 0.25f;
        public float leftEdge = 0f;
        public float rightEdge = 38.4f;
        public float mapHeight = 10.8f;
        public float viewportWidth = 19.2f;
        public bool fitWholeMap;
        public float followSharpness = 8f;

        private Camera viewCamera;

        private void Awake()
        {
            viewCamera = GetComponent<Camera>();
            float widthToFit = fitWholeMap ? rightEdge - leftEdge : viewportWidth;
            viewCamera.orthographicSize = Mathf.Max(mapHeight * 0.5f, widthToFit / (2f * viewCamera.aspect));
            if (!fitWholeMap) return;
            Vector3 position = transform.position;
            position.x = (leftEdge + rightEdge) * 0.5f;
            position.y = mapHeight * 0.5f;
            transform.position = position;
        }

        private void LateUpdate()
        {
            if (player == null) return;
            if (fitWholeMap) return;
            float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            Vector3 position = transform.position;
            position.x = Mathf.Lerp(position.x, GetFollowPositionX(), blend);
            transform.position = position;
        }

        public void SnapToPlayer()
        {
            if (player == null) return;
            Vector3 position = transform.position;
            position.x = fitWholeMap ? (leftEdge + rightEdge) * 0.5f : GetFollowPositionX();
            position.y = mapHeight * 0.5f;
            transform.position = position;
        }

        private float GetFollowPositionX()
        {
            float halfWidth = viewCamera.orthographicSize * viewCamera.aspect;
            float desired = player.position.x + halfWidth * (1f - 2f * playerScreenX);
            float minimum = leftEdge + halfWidth;
            float maximum = Mathf.Max(minimum, rightEdge - halfWidth);
            return Mathf.Clamp(desired, minimum, maximum);
        }
    }
}
