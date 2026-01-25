using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VREmoteWheel
{
    /// <summary>
    /// Individual emote menu item with selection animation.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class EmoteMenuItem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image emoteImage;

        private int emoteIndex;
        private EmoteData emoteData;
        private EmoteWheelConfig config;
        private RectTransform rectTransform;
        private Coroutine animationCoroutine;

        private bool isSelected;
        private Color originalColor;

        /// <summary>
        /// Gets the emote index this item represents.
        /// </summary>
        public int EmoteIndex => emoteIndex;

        /// <summary>
        /// Gets whether this item is currently selected.
        /// </summary>
        public bool IsSelected => isSelected;

        private void Awake()
        {
            if (emoteImage == null)
            {
                emoteImage = GetComponent<Image>();
            }
            rectTransform = GetComponent<RectTransform>();

            if (emoteImage != null)
            {
                originalColor = emoteImage.color;
            }
        }

        /// <summary>
        /// Initializes the menu item with emote data.
        /// </summary>
        public void Initialize(int index, EmoteData data, EmoteWheelConfig wheelConfig)
        {
            emoteIndex = index;
            emoteData = data;
            config = wheelConfig;

            if (emoteImage != null && data.emoteSprite != null)
            {
                emoteImage.sprite = data.emoteSprite;
            }

            // Set initial unselected state
            SetSelected(false, true);
        }

        /// <summary>
        /// Sets the selected state with animation.
        /// </summary>
        public void SetSelected(bool selected, bool immediate = false)
        {
            if (isSelected == selected && !immediate) return;

            isSelected = selected;

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            if (immediate)
            {
                ApplyState(selected);
            }
            else
            {
                animationCoroutine = StartCoroutine(AnimateSelection(selected));
            }
        }

        private void ApplyState(bool selected)
        {
            if (config == null) return;

            float targetScale = selected ? config.selectedItemScale : config.itemScale;
            float targetAlpha = selected ? config.selectedAlpha : config.unselectedAlpha;

            if (rectTransform != null)
            {
                rectTransform.localScale = new Vector3(targetScale, targetScale, 1f);
            }

            if (emoteImage != null)
            {
                Color color = emoteImage.color;
                color.a = targetAlpha;
                emoteImage.color = color;
            }
        }

        private IEnumerator AnimateSelection(bool selected)
        {
            if (config == null) yield break;

            float duration = config.selectionAnimDuration;
            float startScale = selected ? config.itemScale : config.selectedItemScale;
            float endScale = selected ? config.selectedItemScale : config.itemScale;
            float startAlpha = selected ? config.unselectedAlpha : config.selectedAlpha;
            float endAlpha = selected ? config.selectedAlpha : config.unselectedAlpha;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Smooth easing
                float smoothT = t * t * (3f - 2f * t);

                float currentScale = Mathf.Lerp(startScale, endScale, smoothT);
                float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);

                if (rectTransform != null)
                {
                    rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
                }

                if (emoteImage != null)
                {
                    Color color = emoteImage.color;
                    color.a = currentAlpha;
                    emoteImage.color = color;
                }

                yield return null;
            }

            // Ensure final state
            ApplyState(selected);
            animationCoroutine = null;
        }

        /// <summary>
        /// Plays a "confirm" animation when the emote is selected and menu closes.
        /// </summary>
        public void PlayConfirmAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(ConfirmAnimation());
        }

        private IEnumerator ConfirmAnimation()
        {
            if (config == null) yield break;

            float duration = config.selectionAnimDuration * 0.5f;
            float peakScale = config.selectedItemScale * 1.2f;

            // Quick scale up
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.Lerp(config.selectedItemScale, peakScale, t);

                if (rectTransform != null)
                {
                    rectTransform.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }

            // Quick scale down
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.Lerp(peakScale, config.selectedItemScale, t);

                if (rectTransform != null)
                {
                    rectTransform.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }

            animationCoroutine = null;
        }

        private void OnDestroy()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
        }
    }
}
