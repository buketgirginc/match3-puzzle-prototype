using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitToBoard : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private SpriteRenderer boardBackground; // assign BoardBackground

    [Header("Grid Fallback")]
    [SerializeField] private int boardWidth = 8;
    [SerializeField] private int boardHeight = 8;
    [SerializeField] private float cellSize = 0.85f;

    [Header("Padding")]
    [SerializeField] private float padding = 0.2f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        StartCoroutine(FitNextFrame());
    }

    private IEnumerator FitNextFrame()
    {
        // wait 1 frame so BoardView can finish LayoutBackground() sizing
        yield return null;
        Fit();
    }

    [ContextMenu("Fit")]
    public void Fit()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (!cam.orthographic) return;

        if (boardBackground != null)
        {
            Bounds b = boardBackground.bounds;

            cam.transform.position = new Vector3(b.center.x, b.center.y, cam.transform.position.z);

            float aspect = cam.aspect;
            float sizeByHeight = (b.size.y * 0.5f) + padding;
            float sizeByWidth = ((b.size.x * 0.5f) / aspect) + padding;

            cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

            Debug.Log($"[CameraFit] Fit to BACKGROUND bounds size={b.size} center={b.center} ortho={cam.orthographicSize}");
            return;
        }

        // fallback to grid
        float w = (boardWidth - 1) * cellSize;
        float h = (boardHeight - 1) * cellSize;

        if (boardRoot != null)
        {
            Vector3 center = boardRoot.position + new Vector3(w * 0.5f, h * 0.5f, 0f);
            cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
        }

        float aspect2 = cam.aspect;
        float sizeH = (h * 0.5f) + padding;
        float sizeW = ((w * 0.5f) / aspect2) + padding;

        cam.orthographicSize = Mathf.Max(sizeH, sizeW);
        Debug.Log($"[CameraFit] Fit to GRID w={w} h={h} ortho={cam.orthographicSize}");
    }
}
