using UnityEngine;

namespace CaveParallaxDemo
{
    /// <summary>A replaceable image layer that scrolls relative to the camera.</summary>
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [Range(0f, 2f)] public float scrollFactor = 1f;
        public bool repeatHorizontally;
        public bool mirrorEveryOtherTile;
        public Transform targetCamera;

        private Vector3 origin;
        private float cameraOriginX;
        private SpriteRenderer source;
        private SpriteRenderer[] tiles;
        private float spriteWidth;

        private void Start()
        {
            source = GetComponent<SpriteRenderer>();
            origin = transform.position;
            if (targetCamera == null && Camera.main != null) targetCamera = Camera.main.transform;
            if (targetCamera != null) cameraOriginX = targetCamera.position.x;
            if (repeatHorizontally && source.sprite != null) CreateTiles();
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;
            float cameraTravel = targetCamera.position.x - cameraOriginX;
            transform.position = origin + Vector3.right * cameraTravel * (1f - scrollFactor);
            if (tiles == null) return;

            float worldWidth = spriteWidth * Mathf.Abs(transform.lossyScale.x);
            if (worldWidth <= 0.001f) return;
            int centerTile = Mathf.RoundToInt((targetCamera.position.x - transform.position.x) / worldWidth);
            for (int i = 0; i < tiles.Length; i++)
            {
                int tileIndex = centerTile + i - 1;
                tiles[i].transform.localPosition = new Vector3(tileIndex * spriteWidth, 0f, 0f);
                tiles[i].flipX = source.flipX ^ (mirrorEveryOtherTile && (tileIndex & 1) != 0);
            }
        }

        private void CreateTiles()
        {
            spriteWidth = source.sprite.bounds.size.x;
            if (spriteWidth <= 0f) return;
            tiles = new SpriteRenderer[3];
            for (int i = 0; i < tiles.Length; i++)
            {
                GameObject tile = new GameObject("Repeat " + (i - 1));
                tile.transform.SetParent(transform, false);
                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = source.sprite;
                renderer.color = source.color;
                renderer.sharedMaterial = source.sharedMaterial;
                renderer.sortingLayerID = source.sortingLayerID;
                renderer.sortingOrder = source.sortingOrder;
                renderer.flipX = source.flipX;
                renderer.flipY = source.flipY;
                tiles[i] = renderer;
            }
            source.enabled = false;
        }

        private void OnDisable()
        {
            if (source != null) source.enabled = true;
            if (tiles == null) return;
            foreach (SpriteRenderer tile in tiles)
            {
                if (tile != null) Destroy(tile.gameObject);
            }
            tiles = null;
        }
    }
}
