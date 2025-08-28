
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform parentBeforeDrag;
    public Action<CardUI> OnCardPlaced;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        parentBeforeDrag = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
       
        transform.SetParent(canvas.transform, true); // move pro topo do canvas durante o drag
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // se for solto no tabuleiro
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
            Debug.Log("Card returned to hand");

            // volta para a mão corretamente
            transform.SetParent(parentBeforeDrag, false); // false = reposiciona relativo ao novo parent
            rectTransform.localScale = Vector3.one;       // garante escala correta
            rectTransform.anchoredPosition = Vector2.zero; // centraliza no slot da mão
        }
    }


}
