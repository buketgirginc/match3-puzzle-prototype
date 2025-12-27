using UnityEngine;

public class BoardWorldLayoutFromUI : MonoBehaviour
{
    [Header("UI (Canvas)")]
    [SerializeField] private RectTransform objectivesPanel; // UI panel
    [SerializeField] private Canvas rootCanvas;             // en üst Canvas
    [SerializeField] private float gapPixels = 12f;         // panel ile board arasında px boşluk

    [Header("World")]
    [SerializeField] private Transform boardRoot;           // BoardRoot
    [SerializeField] private Camera worldCamera;            // Main Camera
    [SerializeField] private Renderer boardFrameRenderer;   // BoardBackground SpriteRenderer (Renderer)

    private void LateUpdate()
    {
        if (objectivesPanel == null || rootCanvas == null || boardRoot == null || worldCamera == null || boardFrameRenderer == null)
            return;

        // UI panelin world corner'larını al
        Vector3[] corners = new Vector3[4];
        objectivesPanel.GetWorldCorners(corners);

        // corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right
        Vector3 panelBottomLeft = corners[0];

        // Panel alt kenarını SCREEN PIXEL'e çevir
        Vector2 panelBottomScreen = RectTransformUtility.WorldToScreenPoint(rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
                                                                           panelBottomLeft);

        // Hedef: panelin altından gapPixels kadar aşağısı
        float targetScreenY = panelBottomScreen.y - gapPixels;

        // Bu screen Y'nin world'deki karşılığı (kamera ortho)
        float z = Mathf.Abs(worldCamera.transform.position.z); // 2D'de genelde camera z negatif
        Vector3 targetWorldPoint = worldCamera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, targetScreenY, z));

        // Board frame'in mevcut üst Y'si
        float frameTopY = boardFrameRenderer.bounds.max.y;

        // Frame top'u hedef world Y'ye hizala
        float deltaY = targetWorldPoint.y - frameTopY;
        if (Mathf.Abs(deltaY) > 0.0001f)
            boardRoot.position += new Vector3(0f, deltaY, 0f);
    }
}
