using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveRowView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private GameObject tickObject; // Tick Image'in parent objesi
    [SerializeField] private Image tickImage;           // Tick Image component (disabled başlıyor)

    [Header("Colors")]
    [SerializeField] private Color completedTextColor = new Color(0.086f, 0.733f, 0.467f, 1f);

    [Header("Complete Feedback")]
    [SerializeField] private float punchScale = 1.12f;
    [SerializeField] private float punchUpDuration = 0.08f;
    [SerializeField] private float punchDownDuration = 0.10f;
    [SerializeField] private float colorDuration = 0.12f;

    private Coroutine _feedbackRoutine;
    private Color _baseTextColor;


    private bool _completed;

    void Awake()
    {
        if (labelText != null)
            _baseTextColor = labelText.color;
    }
    public void SetIcon(Sprite sprite)
    {
        if (iconImage == null) return;
        iconImage.enabled = (sprite != null);
        iconImage.sprite = sprite;
    }

    public void SetText(string text)
    {
        if (labelText == null) return;
        labelText.text = text;
    }

    public void SetCompleted(bool completed, bool playFeedbackIfJustCompleted = true)
    {
        bool justCompleted = !_completed && completed;
        _completed = completed;

        // Tick: görünürlük
        if (tickObject != null) tickObject.SetActive(true);
        if (tickImage != null) tickImage.enabled = completed;

        if (!completed)
        {
            // reset
            if (labelText != null) labelText.color = _baseTextColor;
            if (labelText != null) labelText.rectTransform.localScale = Vector3.one;
            return;
        }

        if (justCompleted && playFeedbackIfJustCompleted)
            PlayCompleteFeedback();
        else
        {
            // Completed ama feedback oynatma istemiyorsak direkt final state
            if (labelText != null) labelText.color = completedTextColor;
            if (labelText != null) labelText.rectTransform.localScale = Vector3.one;
        }
    }

    public void PlayCompleteFeedback()
    {
        if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(CompleteFeedbackRoutine());
    }

    private IEnumerator CompleteFeedbackRoutine()
    {
        if (labelText == null)
            yield break;

        var tr = labelText.rectTransform;

        // Tick aynı anda görünsün (zaten SetCompleted(true) içinde enabled oluyor ama garanti)
        if (tickObject != null) tickObject.SetActive(true);
        if (tickImage != null) tickImage.enabled = true;

        // Başlangıç state
        tr.localScale = Vector3.one;
        Color startColor = labelText.color;

        float total = Mathf.Max(punchUpDuration + punchDownDuration, colorDuration);
        float t = 0f;

        while (t < total)
        {
            t += Time.unscaledDeltaTime;

            // SCALE (1 -> punchScale -> 1)
            float s;
            if (t <= punchUpDuration)
            {
                float k = Mathf.Clamp01(t / punchUpDuration);
                s = Mathf.Lerp(1f, punchScale, EaseOutCubic(k));
            }
            else
            {
                float k = Mathf.Clamp01((t - punchUpDuration) / Mathf.Max(0.0001f, punchDownDuration));
                s = Mathf.Lerp(punchScale, 1f, EaseOutCubic(k));
            }
            tr.localScale = new Vector3(s, s, 1f);

            // COLOR (start -> completedTextColor)
            float kc = Mathf.Clamp01(t / Mathf.Max(0.0001f, colorDuration));
            labelText.color = Color.Lerp(startColor, completedTextColor, EaseOutCubic(kc));

            yield return null;
        }

        // Final state
        tr.localScale = Vector3.one;
        labelText.color = completedTextColor;

        _feedbackRoutine = null;
    }

    private float EaseOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        float t = 0f;
        Vector3 start = transform.localScale;

        if (duration <= 0f)
        {
            transform.localScale = target;
            yield break;
        }

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // UI için güzel
            float k = Mathf.Clamp01(t / duration);

            // basit easeOutBack hissi (hafif)
            float eased = EaseOutBack(k);

            transform.localScale = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }

        transform.localScale = target;
    }
    private float EaseOutBack(float x)
    {

        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
