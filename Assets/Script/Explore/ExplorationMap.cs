using UnityEngine;

namespace CaveParallaxDemo
{
    public class ExplorationMap : MonoBehaviour
    {
        [Min(1f)] public float width = 38.4f;
        [Min(1f)] public float height = 10.8f;
        [Min(1f)] public float viewportWidth = 19.2f;
        public Vector2 playerSpawn = new Vector2(4.8f, 3f);
        [Min(0f)] public float edgePadding = 1.7f;

        public void BindCamera(Transform cameraTransform)
        {
            foreach (var layer in GetComponentsInChildren<ParallaxLayer>(true))
                layer.targetCamera = cameraTransform;
        }
    }
}
