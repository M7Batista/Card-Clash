using UnityEngine;
using UnityEngine.EventSystems;

public class CardZoom : MonoBehaviour, IScrollHandler, IDragHandler
{
    public RectTransform target;      // O card exibido em tela cheia
    public RectTransform viewport;    // Área visível (ex: o painel que mostra a carta)
    public float zoomSpeed = 0.1f;
    public float minScale = 1f;
    public float maxScale = 3f;

    void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();
    }

    void Update()
    {
        // --- Pinch Zoom no Mobile ---
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0Prev = touch0.position - touch0.deltaPosition;
            Vector2 touch1Prev = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0Prev - touch1Prev).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            ApplyZoom(difference * 0.01f);
        }
    }

    // --- Zoom com scroll (PC) ---
    public void OnScroll(PointerEventData eventData)
    {
        ApplyZoom(eventData.scrollDelta.y * zoomSpeed);
    }

    // --- Arrastar a carta ---
    public void OnDrag(PointerEventData eventData)
    {
        if (target.localScale.x > 1f) // só arrasta se tiver zoom
        {
            target.anchoredPosition += eventData.delta;
            ClampToBounds();
        }
    }

    // --- Aplica zoom ---
    private void ApplyZoom(float increment)
    {
        float newScale = Mathf.Clamp(target.localScale.x + increment, minScale, maxScale);
        target.localScale = new Vector3(newScale, newScale, 1f);
        ClampToBounds();
    }

    // --- Limita posição para não sair da viewport ---
    private void ClampToBounds()
    {
        if (viewport == null) return;

        Vector2 viewportSize = viewport.rect.size;
        Vector2 targetSize = target.rect.size * target.localScale.x;

        Vector2 minPos = (targetSize - viewportSize) * -0.5f;
        Vector2 maxPos = (targetSize - viewportSize) * 0.5f;

        Vector2 clampedPos = target.anchoredPosition;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minPos.x, maxPos.x);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minPos.y, maxPos.y);

        target.anchoredPosition = clampedPos;
    }
    public void ResetZoom()
    {
        target.localScale = Vector3.one;
        target.anchoredPosition = Vector2.zero;
    }
}
