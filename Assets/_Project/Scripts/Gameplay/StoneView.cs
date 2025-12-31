using System.Collections;
using Match3.Core;
using UnityEngine;

namespace Match3.Gameplay
{
    public class StoneView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Vector2Int GridPos { get; private set; }

        private Vector3 _baseLocalPos;
        private Vector3 _baseScale;
        private Color _baseColor;

        private Coroutine _hitRoutine;

        private void Awake()
        {
            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

            _baseLocalPos = transform.localPosition;
            _baseScale = transform.localScale;
            _baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
        }

        public void Init(Vector2Int pos, Sprite sprite)
        {
            GridPos = pos;

            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = sprite;
            }

            // In case Init runs after placement
            _baseLocalPos = transform.localPosition;
            _baseScale = transform.localScale;
            _baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
        }

        public void PlayHitFeedback(bool breaking)
        {
            if (!gameObject.activeInHierarchy) return;

            if (_hitRoutine != null) StopCoroutine(_hitRoutine);
            _hitRoutine = StartCoroutine(HitRoutine(breaking));
        }

        private IEnumerator HitRoutine(bool breaking)
        {
            // stronger if it will break
            float punchScale = breaking ? 1.18f : 1.10f;
            float flashAmount = breaking ? 1.25f : 1.15f;
            float shake = breaking ? 0.06f : 0.04f;

            float dur = breaking ? 0.16f : 0.12f;
            dur = Mathf.Max(0.0001f, dur);

            // cache bases (in case something moved it)
            _baseLocalPos = transform.localPosition;
            _baseScale = transform.localScale;
            if (spriteRenderer) _baseColor = spriteRenderer.color;

            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / dur);

                // scale: 1 -> punch -> 1
                float s;
                if (a < 0.5f)
                {
                    float k = a / 0.5f;
                    s = Mathf.Lerp(1f, punchScale, EaseOutCubic(k));
                }
                else
                {
                    float k = (a - 0.5f) / 0.5f;
                    s = Mathf.Lerp(punchScale, 1f, EaseOutCubic(k));
                }
                transform.localScale = _baseScale * s;

                // tiny shake (local)
                float x = (Mathf.PerlinNoise(Time.time * 60f, 0f) - 0.5f) * 2f;
                float y = (Mathf.PerlinNoise(0f, Time.time * 60f) - 0.5f) * 2f;
                transform.localPosition = _baseLocalPos + new Vector3(x, y, 0f) * shake * (1f - a);

                // flash (color brighten)
                if (spriteRenderer)
                {
                    Color target = _baseColor * flashAmount;
                    spriteRenderer.color = Color.Lerp(target, _baseColor, a);
                }

                yield return null;
            }

            // snap back
            transform.localScale = _baseScale;
            transform.localPosition = _baseLocalPos;
            if (spriteRenderer) spriteRenderer.color = _baseColor;

            _hitRoutine = null;
        }

        private float EaseOutCubic(float x)
        {
            x = Mathf.Clamp01(x);
            return 1f - Mathf.Pow(1f - x, 3f);
        }
    }
}
