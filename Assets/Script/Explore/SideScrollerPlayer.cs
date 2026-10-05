using UnityEngine;
using UnityEngine.InputSystem;

namespace CaveParallaxDemo
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SideScrollerPlayer : MonoBehaviour
    {
        [Min(0f)] public float moveSpeed = 3.5f;
        public float minimumX = 1.7f;
        public float maximumX = 15f;

        private Rigidbody2D body;
        private float horizontal;

        private void Awake() => body = GetComponent<Rigidbody2D>();

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) { horizontal = 0f; return; }
            horizontal = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                       - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
        }

        private void FixedUpdate()
        {
            Vector2 next = body.position + Vector2.right * (horizontal * moveSpeed * Time.fixedDeltaTime);
            next.x = Mathf.Clamp(next.x, minimumX, maximumX);
            body.MovePosition(next);
        }
    }
}
