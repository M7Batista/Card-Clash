using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SlidePanel : MonoBehaviour
{
    public float duration = 0.35f;
    public float extraMargin = 20f;
    public bool useCanvasGroupFade = true;
    public bool deactivateOnHide = true;

    float positionHideedOffsetY = -350f;
    float positionShowedOffsetY = 350f;

    RectTransform rt;
    RectTransform canvasRect;
    Vector2 shownPos;
    Vector2 hiddenPos;
    Canvas parentCanvas;
    CanvasGroup canvasGroup;
    Coroutine anim;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null) canvasRect = parentCanvas.GetComponent<RectTransform>();
        canvasGroup = useCanvasGroupFade ? (GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>()) : null;
    }

    public void Show()
    {
        if (anim != null) StopCoroutine(anim);
        gameObject.SetActive(true);
        shownPos = new Vector2(rt.anchoredPosition.x, positionShowedOffsetY);
        anim = StartCoroutine(MoveTo(shownPos));
    }

    public void Hide()
    {
        if (anim != null) StopCoroutine(anim);
        hiddenPos = new Vector2(rt.anchoredPosition.x, positionHideedOffsetY);
        anim = StartCoroutine(MoveTo(hiddenPos));
    }

    IEnumerator MoveTo(Vector2 toPos)
    {
        float t = 0f;
        Vector2 from = rt.anchoredPosition;
        Vector2 to = toPos;
        Debug.Log("Moving panel to: " + toPos);
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            rt.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f,1f,t));
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, t);
            yield return null;
        }
        rt.anchoredPosition = to;
        anim = null;
    }

    // uso direto quando você quer esconder instantaneamente (sem animação)
    public void HideImmediate()
    {
        if (anim != null) StopCoroutine(anim);
        rt.anchoredPosition = hiddenPos;
        if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; }
        if (deactivateOnHide) gameObject.SetActive(false);
        anim = null;
    }

    // uso direto para mostrar sem animação
    public void ShowImmediate()
    {
        if (anim != null) StopCoroutine(anim);
        gameObject.SetActive(true);
        rt.anchoredPosition = shownPos;
        if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; }
        anim = null;
    }
}