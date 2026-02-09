using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PuzzlePieceDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private PuzzlePiece piece;

    private Vector2 startPosition;
    private Transform startParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        piece = GetComponent<PuzzlePiece>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (piece.isFixed) return;

        startPosition = rectTransform.anchoredPosition;
        startParent = rectTransform.parent;

        rectTransform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (piece.isFixed) return;

        rectTransform.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (piece.isFixed) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        PuzzleSlot slot = GetSlotUnderPointer(eventData);
        //if (slot != null && slot == piece.correctSlot && !slot.isOccupied)
        if (slot != null && slot.index == piece.index && !slot.isOccupied)
        {
            Debug.Log("Peça colocada no slot correto!");
            SnapToSlot(slot);
        }
        else
        {
            Debug.Log("Peça não encaixada. Retornando à posição inicial.");
            ReturnToStart();
        }
       
    }

    // 🔍 Detecta slot via raycast de UI
    PuzzleSlot GetSlotUnderPointer(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            PuzzleSlot slot =
                result.gameObject.GetComponent<PuzzleSlot>();

            if (slot != null)
                return slot;
        }

        return null;
    }

    void SnapToSlot(PuzzleSlot slot)
    {
        rectTransform.SetParent(slot.transform);
        ResetAnchorsToCenter();
        rectTransform.anchoredPosition = Vector2.zero;

        piece.isFixed = true;
        slot.isOccupied = true;

        Debug.Log("Peça encaixada corretamente!");
        PuzzleCompletionController.Instance.OnPieceFixed();
    }

    void ReturnToStart()
    {
        rectTransform.SetParent(startParent);
        rectTransform.anchoredPosition = startPosition;
    }
    void ResetAnchorsToCenter()
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
   

}
