using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SlidePanel : MonoBehaviour
{
    public float duration = 0.35f;
    public float extraMargin = 20f;
    public bool startHidden = true;
    public bool useCanvasGroupFade = true;
    public bool deactivateOnHide = true;

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

    void Start()
    {
        // assume panel is placed at desired shown position in editor
        shownPos = rt.anchoredPosition;

        // calcula posição escondida (abaixo da área visível do canvas)
        float offY = 0f;
        if (canvasRect != null)
        {
            offY = - (canvasRect.rect.height * 0.5f) - (rt.rect.height * 0.5f) - extraMargin;
        }
        else
        {
            offY = -Screen.height - rt.rect.height - extraMargin;
        }
        hiddenPos = new Vector2(shownPos.x, offY);

        if (startHidden)
        {
            rt.anchoredPosition = hiddenPos;
            if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; }
            if (deactivateOnHide) gameObject.SetActive(false);
        }
    }

    public void Show()
    {
        if (anim != null) StopCoroutine(anim);
        gameObject.SetActive(true);
        anim = StartCoroutine(DoShow());
    }

    IEnumerator DoShow()
    {
        float t = 0f;
        Vector2 from = rt.anchoredPosition;
        Vector2 to = shownPos;
        if (canvasGroup != null) { canvasGroup.blocksRaycasts = true; }

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            rt.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f,1f,t));
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, t);
            yield return null;
        }
        rt.anchoredPosition = to;
        if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; }
        anim = null;
    }

    public void Hide()
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(DoHide());
    }

    IEnumerator DoHide()
    {
        float t = 0f;
        Vector2 from = rt.anchoredPosition;
        Vector2 to = hiddenPos;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            rt.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f,1f,t));
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, t);
            yield return null;
        }
        rt.anchoredPosition = to;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (deactivateOnHide) gameObject.SetActive(false);
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