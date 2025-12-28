using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitToBoard : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private SpriteRenderer boardBackground; // assign BoardBackground

    [Header("Behavior")]
    [Tooltip("If enabled, camera will move to center on the board. If disabled, only orthographic size is adjusted.")]
    [SerializeField] private bool fitPosition = false;

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
    private float _lastOrtho = -1f;
    private Vector3 _lastPos;
    private bool _hasLastPos;

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
                Mathf.Approximately(padding, _lastPadding) &&
                (!fitPosition || (_hasLastPos && Vector3.SqrMagnitude(GetTargetPos(b) - _lastPos) < 0.000001f)))
            {
                return;
            }

            float sizeByHeight = (b.size.y * 0.5f) + padding;
            float sizeByWidth = ((b.size.x * 0.5f) / Mathf.Max(0.0001f, aspect)) + padding;
            float newOrtho = Mathf.Max(sizeByHeight, sizeByWidth);

            ApplyOrtho(newOrtho);

            if (fitPosition)
            {
                Vector3 pos = GetTargetPos(b);
                cam.transform.position = pos;
                _lastPos = pos;
                _hasLastPos = true;
            }

            _lastBgBounds = b;
            _lastAspect = aspect;
            _lastPadding = padding;

            if (debugLog)
                Debug.Log($"[CameraFit] Fit BACKGROUND size={b.size} center={b.center} aspect={aspect} ortho={newOrtho} fitPos={fitPosition}");

            return;
        }

        // --- Fallback to grid size ---
        float w = (boardWidth - 1) * cellSize;
        float h = (boardHeight - 1) * cellSize;

        float sizeH = (h * 0.5f) + padding;
        float sizeW = ((w * 0.5f) / Mathf.Max(0.0001f, aspect)) + padding;
        float ortho = Mathf.Max(sizeH, sizeW);

        Vector3 fallbackPos = cam.transform.position;
        if (fitPosition)
        {
            Vector3 center = boardRoot != null
                ? boardRoot.position + new Vector3(w * 0.5f, h * 0.5f, 0f)
                : new Vector3(w * 0.5f, h * 0.5f, 0f);

            fallbackPos = new Vector3(center.x, center.y, cam.transform.position.z);
        }

        // If nothing changed, do nothing
        if (!force &&
            Mathf.Approximately(aspect, _lastAspect) &&
            Mathf.Approximately(padding, _lastPadding) &&
            Mathf.Abs(ortho - _lastOrtho) < 0.0001f &&
            (!fitPosition || (_hasLastPos && Vector3.SqrMagnitude(fallbackPos - _lastPos) < 0.000001f)))
        {
            return;
        }

        ApplyOrtho(ortho);

        if (fitPosition)
        {
            cam.transform.position = fallbackPos;
            _lastPos = fallbackPos;
            _hasLastPos = true;
        }

        _lastAspect = aspect;
        _lastPadding = padding;

        if (debugLog)
            Debug.Log($"[CameraFit] Fit GRID w={w} h={h} aspect={aspect} ortho={ortho} fitPos={fitPosition}");
    }

    private Vector3 GetTargetPos(Bounds b)
    {
        return new Vector3(b.center.x, b.center.y, cam.transform.position.z);
    }

    private void ApplyOrtho(float ortho)
    {
        cam.orthographicSize = ortho;
        _lastOrtho = ortho;
    }

    private static bool BoundsApproximatelyEqual(Bounds a, Bounds b)
    {
        // Bounds equality can be flaky due to float jitter
        return Vector3.SqrMagnitude(a.center - b.center) < 0.000001f &&
               Vector3.SqrMagnitude(a.size - b.size) < 0.000001f;
    }
}
