using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightBatteryUI : MonoBehaviour
{
    [Header("References")]
    public FlashlightController flashlight;   // Reference to your FlashlightController
    public Slider batterySlider;              // The battery slider UI
    public Image fillImage;                   // The Fill image of the slider
    public GameObject hintTextObject;         // Optional: "Press T to turn on flashlight" text

    [Header("Hint Settings")]
    public float hintFadeDuration = 2f;

    private bool hintShown;
    private bool hintFading;

    void Start()
    {
        // Hide the slider and hint text at start
        if (batterySlider != null)
            batterySlider.gameObject.SetActive(false);

        if (hintTextObject != null)
            hintTextObject.SetActive(false);
    }

    void Update()
    {
        if (flashlight == null || batterySlider == null)
            return;

        // Show the battery bar once the player has the flashlight
        if (flashlight.HasFlashlight())
        {
            if (!batterySlider.gameObject.activeSelf)
            {
                batterySlider.gameObject.SetActive(true);

                // Show hint once
                if (!hintShown && hintTextObject != null)
                {
                    hintTextObject.SetActive(true);
                    hintShown = true;
                }
            }

            // Fade out hint when flashlight key is pressed
            if (hintShown && Input.GetKeyDown(flashlight.flashlightKey))
                StartCoroutine(FadeOutHint());

            UpdateBatteryUI();
        }
    }

    private void UpdateBatteryUI()
    {
        if (flashlight == null || batterySlider == null)
            return;

        // 🧮 Calculate percentage based on time remaining vs total capacity
        float percent = flashlight.batterySecondsRemaining / flashlight.batteryCapacitySeconds;
        percent = Mathf.Clamp01(percent);

        // Update slider value
        batterySlider.value = percent;

        // 🌈 Color shift from green → yellow → red
        if (fillImage != null)
        {
            if (percent > 0.5f)
                fillImage.color = Color.Lerp(Color.yellow, Color.green, (percent - 0.5f) * 2f);
            else
                fillImage.color = Color.Lerp(Color.red, Color.yellow, percent * 2f);
        }
    }

    private System.Collections.IEnumerator FadeOutHint()
    {
        if (hintFading || hintTextObject == null)
            yield break;

        hintFading = true;

        TextMeshProUGUI tmpText = hintTextObject.GetComponent<TextMeshProUGUI>();
        Text uiText = hintTextObject.GetComponent<Text>();
        float elapsed = 0f;

        while (elapsed < hintFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / hintFadeDuration);

            if (tmpText != null)
            {
                var c = tmpText.color;
                tmpText.color = new Color(c.r, c.g, c.b, alpha);
            }

            if (uiText != null)
            {
                var c = uiText.color;
                uiText.color = new Color(c.r, c.g, c.b, alpha);
            }

            yield return null;
        }

        hintTextObject.SetActive(false);
    }
}
