using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.Gameplay
{
    public class InputController : MonoBehaviour
    {
        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 world = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            Vector2 pos2D = new Vector2(world.x, world.y);

            RaycastHit2D hit = Physics2D.Raycast(pos2D, Vector2.zero);
            if (!hit.collider) return;

            var tile = hit.collider.GetComponent<TileView>();
            if (tile == null) return;

            Debug.Log($"Tile selected at {tile.Cell.Pos} ({tile.Cell.Tile})");
        }

    }
}
