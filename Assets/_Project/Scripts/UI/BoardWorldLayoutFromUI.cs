using UnityEngine;

public class BoardWorldLayoutFromUI : MonoBehaviour
{
    [Header("UI (Canvas)")]
    [SerializeField] private RectTransform objectivesPanel;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private float gapPixels = 12f;

    [Header("World")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Renderer boardFrameRenderer;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    public void ApplyLayout()
    {
        if (objectivesPanel == null ||
            rootCanvas == null ||
            boardRoot == null ||
            worldCamera == null ||
            boardFrameRenderer == null)
        {
            Debug.LogWarning("[BoardWorldLayoutFromUI] Missing references, layout skipped.");
            return;
        }

        // UI panelin world corner'larını al
        Vector3[] corners = new Vector3[4];
        objectivesPanel.GetWorldCorners(corners);

        // corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right
        Vector3 panelBottomWorld = corners[0];

        // Panel alt kenarını SCREEN SPACE'e çevir
        Camera uiCam =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        Vector2 panelBottomScreen =
            RectTransformUtility.WorldToScreenPoint(uiCam, panelBottomWorld);

        // Hedef: panelin altından gapPixels kadar aşağı
        float targetScreenY = panelBottomScreen.y - gapPixels;

        // Screen -> World (board düzlemi Z'sine göre)
        float planeDistance = Mathf.Abs(boardRoot.position.z - worldCamera.transform.position.z);
        Vector3 targetWorld =
            worldCamera.ScreenToWorldPoint(
                new Vector3(Screen.width * 0.5f, targetScreenY, planeDistance)
            );

        // Board frame'in mevcut üst Y sınırı
        float frameTopY = boardFrameRenderer.bounds.max.y;

        // Frame top'u hedef world Y'ye hizala
        float deltaY = targetWorld.y - frameTopY;

        if (Mathf.Abs(deltaY) > 0.0001f)
            boardRoot.position += new Vector3(0f, deltaY, 0f);

        if (debugLog)
            Debug.Log($"[BoardWorldLayoutFromUI] Applied deltaY={deltaY:0.####} targetY={targetWorld.y:0.####} frameTopY={frameTopY:0.####}");
    }
}