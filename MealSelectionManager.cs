using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable 2-button "toggle" selector with clean visuals:
/// - only one button can be selected at a time
/// - smooth color transition
/// - small pop (scale) when selected
/// - small bounce on tap
/// </summary>
public class MealSelectionManager : MonoBehaviour
{
    [Header("Button References")]
    public Button beforeMealButton;
    public Button afterMealButton;

    [Header("Saved Values")]
    public string beforeMealValue = "Before Meal";
    public string afterMealValue = "After Meal";

    [Header("Visual Style")]
    [Tooltip("Color used for the selected meal button.")]
    public Color selectedColor = new Color(0.35f, 0.85f, 0.95f, 1f);

    [Tooltip("How much the selected button pops bigger (e.g. 1.05 = 5% bigger).")]
    public float selectedScaleMultiplier = 1.06f;

    [Tooltip("Seconds for smooth color transition.")]
    public float colorTransitionSeconds = 0.18f;

    [Tooltip("Seconds for scale pop animation.")]
    public float popSeconds = 0.18f;

    [Tooltip("Seconds for small tap bounce animation.")]
    public float bounceSeconds = 0.10f;

    [Tooltip("How much to shrink during bounce (e.g. 0.97).")]
    public float bounceShrinkMultiplier = 0.97f;

    private int _selectedIndex = -1; // 0 = before, 1 = after

    private RectTransform _beforeRect;
    private RectTransform _afterRect;
    private Image _beforeImage;
    private Image _afterImage;

    private Vector3 _beforeBaseScale;
    private Vector3 _afterBaseScale;
    private Color _beforeNormalColor;
    private Color _afterNormalColor;

    private Coroutine _beforeColorRoutine;
    private Coroutine _afterColorRoutine;
    private Coroutine _beforeScaleRoutine;
    private Coroutine _afterScaleRoutine;
    private Coroutine _bounceRoutine;

    private bool _initialized;

    /// <summary>Empty string means nothing selected yet.</summary>
    public string SelectedMealValue
    {
        get
        {
            if (_selectedIndex == 0) return beforeMealValue;
            if (_selectedIndex == 1) return afterMealValue;
            return string.Empty;
        }
    }

    public bool IsBeforeSelected => _selectedIndex == 0;
    public bool IsAfterSelected => _selectedIndex == 1;
    public bool HasSelection => _selectedIndex >= 0;

    private void Awake()
    {
        if (beforeMealButton != null && afterMealButton != null)
        {
            InitializeVisuals();
        }
    }

    /// <summary>
    /// Call this if you want to set buttons from another script at runtime.
    /// (Still beginner-friendly; just assigns + caches the UI components.)
    /// </summary>
    public void Initialize(Button before, Button after)
    {
        beforeMealButton = before;
        afterMealButton = after;
        InitializeVisuals();
    }

    private void InitializeVisuals()
    {
        _initialized = true;

        _beforeRect = beforeMealButton != null ? beforeMealButton.GetComponent<RectTransform>() : null;
        _afterRect = afterMealButton != null ? afterMealButton.GetComponent<RectTransform>() : null;

        _beforeImage = beforeMealButton != null ? beforeMealButton.GetComponent<Image>() : null;
        _afterImage = afterMealButton != null ? afterMealButton.GetComponent<Image>() : null;

        if (_beforeRect != null) _beforeBaseScale = _beforeRect.localScale;
        if (_afterRect != null) _afterBaseScale = _afterRect.localScale;

        if (_beforeImage != null) _beforeNormalColor = _beforeImage.color;
        if (_afterImage != null) _afterNormalColor = _afterImage.color;

        // Start with no selection.
        _selectedIndex = -1;
        ApplyImmediateState(0, false);
        ApplyImmediateState(1, false);
    }

    /// <summary>Call from BeforeMeal button OnClick.</summary>
    public void SelectBeforeMeal()
    {
        SelectMeal(0);
    }

    /// <summary>Call from AfterMeal button OnClick.</summary>
    public void SelectAfterMeal()
    {
        SelectMeal(1);
    }

    private void SelectMeal(int index)
    {
        if (!_initialized)
        {
            // If you forgot to assign the buttons in Inspector, this helps diagnose.
            Debug.LogWarning("[MealSelectionManager] Not initialized. Assign Before/After Meal buttons in Inspector.");
            return;
        }

        _selectedIndex = index;

        // Animate selected and unselected states.
        AnimateButtonState(0, index == 0, playedBounce: index == 0);
        AnimateButtonState(1, index == 1, playedBounce: index == 1);
    }

    private void AnimateButtonState(int which, bool selected, bool playedBounce)
    {
        if (which == 0)
        {
            // Color
            if (_beforeImage != null)
            {
                if (_beforeColorRoutine != null) StopCoroutine(_beforeColorRoutine);
                _beforeColorRoutine = StartCoroutine(TransitionColor(_beforeImage, selected ? selectedColor : _beforeNormalColor, colorTransitionSeconds));
            }

            // Scale (pop)
            if (_beforeRect != null)
            {
                if (_beforeScaleRoutine != null) StopCoroutine(_beforeScaleRoutine);
                Vector3 targetScale = selected ? _beforeBaseScale * selectedScaleMultiplier : _beforeBaseScale;
                _beforeScaleRoutine = StartCoroutine(ScaleTo(_beforeRect, targetScale, popSeconds));
            }

            // Bounce only on the tapped one.
            if (playedBounce && _beforeRect != null)
                StartBounce(_beforeRect, selected ? _beforeBaseScale * selectedScaleMultiplier : _beforeBaseScale);
        }
        else
        {
            if (_afterImage != null)
            {
                if (_afterColorRoutine != null) StopCoroutine(_afterColorRoutine);
                _afterColorRoutine = StartCoroutine(TransitionColor(_afterImage, selected ? selectedColor : _afterNormalColor, colorTransitionSeconds));
            }

            if (_afterRect != null)
            {
                if (_afterScaleRoutine != null) StopCoroutine(_afterScaleRoutine);
                Vector3 targetScale = selected ? _afterBaseScale * selectedScaleMultiplier : _afterBaseScale;
                _afterScaleRoutine = StartCoroutine(ScaleTo(_afterRect, targetScale, popSeconds));
            }

            if (playedBounce && _afterRect != null)
                StartBounce(_afterRect, selected ? _afterBaseScale * selectedScaleMultiplier : _afterBaseScale);
        }
    }

    private void ApplyImmediateState(int which, bool selected)
    {
        if (which == 0)
        {
            if (_beforeImage != null) _beforeImage.color = selected ? selectedColor : _beforeNormalColor;
            if (_beforeRect != null) _beforeRect.localScale = selected ? _beforeBaseScale * selectedScaleMultiplier : _beforeBaseScale;
        }
        else
        {
            if (_afterImage != null) _afterImage.color = selected ? selectedColor : _afterNormalColor;
            if (_afterRect != null) _afterRect.localScale = selected ? _afterBaseScale * selectedScaleMultiplier : _afterBaseScale;
        }
    }

    private void StartBounce(RectTransform rect, Vector3 targetScale)
    {
        if (_bounceRoutine != null) StopCoroutine(_bounceRoutine);
        _bounceRoutine = StartCoroutine(BounceScale(rect, targetScale));
    }

    private IEnumerator BounceScale(RectTransform rect, Vector3 targetScale)
    {
        float half = Mathf.Max(0.001f, bounceSeconds * 0.5f);

        Vector3 start = rect.localScale;
        Vector3 bouncedDown = targetScale * bounceShrinkMultiplier;

        // Down
        yield return ScaleToInternal(rect, bouncedDown, half);
        // Up
        yield return ScaleToInternal(rect, targetScale, Mathf.Max(0.001f, bounceSeconds - half));
    }

    private IEnumerator TransitionColor(Image img, Color to, float duration)
    {
        duration = Mathf.Max(0.001f, duration);
        Color from = img.color;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            u = Mathf.SmoothStep(0f, 1f, u);
            img.color = Color.Lerp(from, to, u);
            yield return null;
        }

        img.color = to;
    }

    private IEnumerator ScaleTo(RectTransform rect, Vector3 target, float duration)
    {
        yield return ScaleToInternal(rect, target, duration);
    }

    private IEnumerator ScaleToInternal(RectTransform rect, Vector3 target, float duration)
    {
        duration = Mathf.Max(0.001f, duration);
        Vector3 from = rect.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            u = Mathf.SmoothStep(0f, 1f, u);
            rect.localScale = Vector3.LerpUnclamped(from, target, u);
            yield return null;
        }

        rect.localScale = target;
    }
}

