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

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private Camera cam;

    // Cache to avoid re-fitting when nothing changed
    private Bounds _lastBgBounds;
    private float _lastAspect = -1f;
    private float _lastPadding = -1f;
    private Vector3 _lastCamPos;
    private float _lastOrtho = -1f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        // In case objects enable/disable during play
        StartCoroutine(FitEndOfFrame());
    }

    private void Start()
    {
        StartCoroutine(FitEndOfFrame());
    }

    private IEnumerator FitEndOfFrame()
    {
        // BoardBackground bounds / layout genelde bu noktaya kadar oturmuş olur
        yield return new WaitForEndOfFrame();
        Fit(force: true);
    }

    [ContextMenu("Fit")]
    public void Fit()
    {
        Fit(force: true);
    }

    public void RequestFit()
    {
        // dışarıdan güvenli çağır: spam olmayacak
        Fit(force: false);
    }

    private void Fit(bool force)
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (!cam.orthographic) return;

        float aspect = cam.aspect;

        // --- Prefer background bounds ---
        if (boardBackground != null)
        {
            Bounds b = boardBackground.bounds;

            // If nothing meaningful changed, do nothing
            if (!force &&
                BoundsApproximatelyEqual(b, _lastBgBounds) &&
                Mathf.Approximately(aspect, _lastAspect) &&
                Mathf.Approximately(padding, _lastPadding))
            {
                return;
            }

            Vector3 newPos = new Vector3(b.center.x, b.center.y, cam.transform.position.z);

            float sizeByHeight = (b.size.y * 0.5f) + padding;
            float sizeByWidth = ((b.size.x * 0.5f) / Mathf.Max(0.0001f, aspect)) + padding;
            float newOrtho = Mathf.Max(sizeByHeight, sizeByWidth);

            ApplyCamera(newPos, newOrtho);

            _lastBgBounds = b;
            _lastAspect = aspect;
            _lastPadding = padding;

            if (debugLog)
                Debug.Log($"[CameraFit] Fit BACKGROUND size={b.size} center={b.center} aspect={aspect} ortho={newOrtho}");

            return;
        }

        // --- Fallback to grid size ---
        float w = (boardWidth - 1) * cellSize;
        float h = (boardHeight - 1) * cellSize;

        Vector3 center = boardRoot != null
            ? boardRoot.position + new Vector3(w * 0.5f, h * 0.5f, 0f)
            : new Vector3(w * 0.5f, h * 0.5f, 0f);

        Vector3 pos = new Vector3(center.x, center.y, cam.transform.position.z);

        float sizeH = (h * 0.5f) + padding;
        float sizeW = ((w * 0.5f) / Mathf.Max(0.0001f, aspect)) + padding;
        float ortho = Mathf.Max(sizeH, sizeW);

        // If nothing changed, do nothing
        if (!force &&
            Mathf.Approximately(aspect, _lastAspect) &&
            Mathf.Approximately(padding, _lastPadding) &&
            Vector3.Distance(pos, _lastCamPos) < 0.0001f &&
            Mathf.Abs(ortho - _lastOrtho) < 0.0001f)
        {
            return;
        }

        ApplyCamera(pos, ortho);

        _lastAspect = aspect;
        _lastPadding = padding;

        if (debugLog)
            Debug.Log($"[CameraFit] Fit GRID w={w} h={h} aspect={aspect} ortho={ortho}");
    }

    private void ApplyCamera(Vector3 pos, float ortho)
    {
        cam.transform.position = pos;
        cam.orthographicSize = ortho;

        _lastCamPos = pos;
        _lastOrtho = ortho;
    }

    private static bool BoundsApproximatelyEqual(Bounds a, Bounds b)
    {
        // Bounds equality can be flaky due to float jitter
        return Vector3.SqrMagnitude(a.center - b.center) < 0.000001f &&
               Vector3.SqrMagnitude(a.size - b.size) < 0.000001f;
    }
}
