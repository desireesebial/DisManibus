using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class UIButtonHoverFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float animDuration = 0.12f;

    [Header("Black Highlight (optional)")]
    [Tooltip("Assign the black Image child (disabled by default). It toggles on hover.")]
    [SerializeField] private Graphic highlightGraphic;

    private RectTransform rt;
    private Coroutine scaleRoutine;
    private Vector3 baseScale;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;
        if (highlightGraphic) highlightGraphic.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData e) => Hover(true);
    public void OnPointerExit(PointerEventData e) => Hover(false);
    public void OnSelect(BaseEventData e) => Hover(true);   // keyboard/controller
    public void OnDeselect(BaseEventData e) => Hover(false);

    private void Hover(bool on)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(on ? hoverScale : 1f));

        if (highlightGraphic)
            highlightGraphic.gameObject.SetActive(on);
    }

    private IEnumerator ScaleTo(float target)
    {
        Vector3 start = rt.localScale;
        Vector3 end = baseScale * target;
        float t = 0f;

        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime; // still works during pause
            float k = Mathf.Clamp01(t / animDuration);
            rt.localScale = Vector3.Lerp(start, end, k);
            yield return null;
        }
        rt.localScale = end;
    }
}
