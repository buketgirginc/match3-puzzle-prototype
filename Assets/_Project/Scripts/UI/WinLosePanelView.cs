using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinLosePanelView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root; // Content


    [Header("UI")]
    [SerializeField] private TMP_Text levelHeaderText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    private void Reset()
    {
        root = transform.Find("Content")?.gameObject;
    }
    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void Show(int levelNumber, bool win, bool isFinalLevel)
    {
        if (root != null)
            root.SetActive(true);

        if (levelHeaderText != null)
            levelHeaderText.text = $"LEVEL {levelNumber}";

        if (resultText != null)
            resultText.text = win ? "YOU WIN" : "YOU LOSE";

        if (actionButtonText != null)
        {
            if (!win) actionButtonText.text = "RESTART";
            else actionButtonText.text = isFinalLevel ? "RESTART" : "NEXT LEVEL";
        }
    }

    public void SetButtonAction(System.Action onClick)
    {
        if (actionButton == null) return;

        actionButton.onClick.RemoveAllListeners();

        if (onClick != null)
            actionButton.onClick.AddListener(() => onClick());
    }
}
