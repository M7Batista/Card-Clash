using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Action<CardUI> OnCardPlaced;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root); // move para o topo da UI
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // se for solto no tabuleiro, "fixa"
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("BoardSlot"))
        {
            transform.SetParent(eventData.pointerEnter.transform, false);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            var cardUI = GetComponent<CardUI>();
            OnCardPlaced?.Invoke(cardUI);

            Destroy(this); // não pode ser arrastada de novo
        }
        else
        {
            // volta para a mão se não foi jogada
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}