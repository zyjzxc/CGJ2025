using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    [System.Serializable]
    public class ShakePreset
    {
        public string name;
        public float intensity = 0.5f;
        public float duration = 0.3f;
        public bool useCurve = false;
        public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    }

    public ShakePreset[] presets;
    public Transform cameraTransform; // 如果为空，将使用主相机

    private Vector3 originalPosition;
    private Coroutine currentShake;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        originalPosition = cameraTransform.localPosition;
    }

    /// <summary>
    /// 使用预设触发屏幕抖动
    /// </summary>
    public void Shake(int presetIndex)
    {
        if (presetIndex >= 0 && presetIndex < presets.Length)
        {
            var preset = presets[presetIndex];
            Shake(preset.intensity, preset.duration, preset.useCurve, preset.shakeCurve);
        }
    }

    /// <summary>
    /// 自定义参数触发屏幕抖动
    /// </summary>
    public void Shake(float intensity, float duration, bool useCurve = false, AnimationCurve curve = null)
    {
        if (currentShake != null)
            StopCoroutine(currentShake);

        currentShake = StartCoroutine(DoShake(intensity, duration, useCurve, curve));
    }

    private System.Collections.IEnumerator DoShake(float intensity, float duration, bool useCurve, AnimationCurve curve)
    {
        float elapsed = 0;
        Vector3 startPosition = cameraTransform.localPosition;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentIntensity = useCurve ? intensity * curve.Evaluate(t) : intensity * (1 - t);

            cameraTransform.localPosition = startPosition + Random.insideUnitSphere * currentIntensity;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
        currentShake = null;
    }
}