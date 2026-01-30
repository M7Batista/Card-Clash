using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardZoom : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform target;      // O card exibido em tela cheia
    public RectTransform viewport;    // Área visível (ex: o painel que mostra a carta)
    public float zoomSpeed = 0.1f;
    public float minScale = 1f;
    public float maxScale = 3f;
    
    private bool isDragging = false;
    private Dictionary<int, Vector2> touchPositions = new Dictionary<int, Vector2>();
    private float lastTouchDistance = 0f;

    void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        ResetZoom();
        touchPositions.Clear();
    }

    void Update()
    {
        // --- Detectar Pinch Zoom com múltiplos toques ---
        if (touchPositions.Count == 2)
        {
            Vector2[] positions = new Vector2[2];
            int index = 0;
            foreach (var pos in touchPositions.Values)
            {
                positions[index++] = pos;
            }

            float currentDistance = Vector2.Distance(positions[0], positions[1]);
            
            if (lastTouchDistance > 0)
            {
                float difference = currentDistance - lastTouchDistance;
                Debug.Log($"Pinch zoom detected - LastDistance: {lastTouchDistance}, CurrentDistance: {currentDistance}, Difference: {difference}");
                ApplyZoom(difference * zoomSpeed * 0.01f);
            }

            lastTouchDistance = currentDistance;
        }
        else
        {
            lastTouchDistance = 0;
        }

        // --- Simular Pinch Zoom no Editor com Ctrl + Mouse Wheel ---
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (scrollDelta != 0)
            {
                Debug.Log("Simulating pinch zoom with Ctrl + Mouse Wheel: " + scrollDelta);
                ApplyZoom(scrollDelta * zoomSpeed * 5f);
            }
        }
#endif
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"PointerDown - PointerId: {eventData.pointerId}, Position: {eventData.position}");
        touchPositions[eventData.pointerId] = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"PointerUp - PointerId: {eventData.pointerId}");
        touchPositions.Remove(eventData.pointerId);
        lastTouchDistance = 0;
    }

    // --- Zoom com scroll (PC) ---
    public void OnScroll(PointerEventData eventData)
    {
        ApplyZoom(eventData.scrollDelta.y * zoomSpeed);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    // --- Arrastar a carta (SEM clamping durante o drag para evitar tremendo) ---
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"OnDrag - PointerId: {eventData.pointerId}, Delta: {eventData.delta}, ActiveTouches: {touchPositions.Count}");
        
        // Só arrasta se houver apenas 1 toque
        if (touchPositions.Count == 1)
        {
            target.anchoredPosition += eventData.delta;
            Debug.Log("New position: " + target.anchoredPosition);
        }
        
        // Atualizar posição do toque
        touchPositions[eventData.pointerId] = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        // Clamp ao terminar o drag
        ClampToBounds();
    }

    // --- Aplica zoom ---
    private void ApplyZoom(float increment)
    {
        if (Mathf.Abs(increment) < 0.001f) return; // Ignora mudanças muito pequenas

        Debug.Log("Applying zoom: " + increment);
        float newScale = Mathf.Clamp(target.localScale.x + increment, minScale, maxScale);
        target.localScale = new Vector3(newScale, newScale, 1f);
        if (!isDragging)
        {
            ClampToBounds();
        }
    }

    // --- Limita posição para não sair da viewport ---
    private void ClampToBounds()
    {
        if (viewport == null)
        {
            Debug.LogWarning("CardZoom: Viewport não está atribuído!");
            return;
        }

        Vector2 viewportSize = viewport.rect.size;
        Vector2 targetSize = target.rect.size * target.localScale.x;

        Debug.Log($"Viewport Size: {viewportSize}, Target Size: {targetSize}, Current Scale: {target.localScale.x}");

        // Calcula os limites máximos de movimento
        // Se a imagem é menor que a viewport, não pode sair do centro
        float maxOffsetX = Mathf.Max(0, (targetSize.x - viewportSize.x) * 0.5f);
        float maxOffsetY = Mathf.Max(0, (targetSize.y - viewportSize.y) * 0.5f);

        Vector2 clampedPos = target.anchoredPosition;
        clampedPos.x = Mathf.Clamp(clampedPos.x, -maxOffsetX, maxOffsetX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, -maxOffsetY, maxOffsetY);

        target.anchoredPosition = clampedPos;
        Debug.Log($"ClampToBounds - Max Offset: ({maxOffsetX}, {maxOffsetY}), Clamped Pos: {clampedPos}");
    }
    
    public void ResetZoom()
    {
        target.localScale = Vector3.one;
        target.anchoredPosition = Vector2.zero;
        touchPositions.Clear();
        lastTouchDistance = 0;
    }
}
