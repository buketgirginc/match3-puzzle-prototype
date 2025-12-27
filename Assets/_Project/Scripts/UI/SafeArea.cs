using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private RectTransform rectTransform;

    private Rect _lastSafeArea = Rect.zero;
    private Vector2Int _lastScreenSize = Vector2Int.zero;

    private Vector2 _lastAnchorMin = new Vector2(-1f, -1f);
    private Vector2 _lastAnchorMax = new Vector2(-1f, -1f);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea(force: true);
    }

    private void OnEnable()
    {
        // UI enable/disable döngülerinde güvenli olsun
        ApplySafeArea(force: true);
    }

    private void Update()
    {
        // SafeArea veya ekran boyutu değiştiyse yeniden uygula
        if (Screen.safeArea != _lastSafeArea ||
            Screen.width != _lastScreenSize.x ||
            Screen.height != _lastScreenSize.y)
        {
            ApplySafeArea(force: false);
        }
    }

    private void ApplySafeArea(bool force)
    {
        Rect safeArea = Screen.safeArea;

        float w = Screen.width;
        float h = Screen.height;

        // Guard: Editor’da bazen 0 gelebiliyor
        if (w <= 0f || h <= 0f) return;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= w;
        anchorMin.y /= h;
        anchorMax.x /= w;
        anchorMax.y /= h;

        // Eğer anchorlar değişmediyse (jitter olsa bile), tekrar set etme + log basma
        if (!force && anchorMin == _lastAnchorMin && anchorMax == _lastAnchorMax)
        {
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            return;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _lastAnchorMin = anchorMin;
        _lastAnchorMax = anchorMax;

        if (debugLog)
            Debug.Log($"[SafeArea] Applied {safeArea} on {Screen.width}x{Screen.height}");
    }
}
