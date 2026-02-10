using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class PuzzlePieceDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private int startSiblingIndex;
    private LayoutElement layoutElement;
    public ScrollRect pieceScroll;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private PuzzlePiece piece;
    private Vector2 startPosition;
    private Transform startParent;
    private Vector2 dragStartPos;
    private bool isDraggingPiece;
    private const float dragThreshold = 15f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        piece = GetComponent<PuzzlePiece>();
        canvas = GetComponentInParent<Canvas>();

        layoutElement = GetComponent<LayoutElement>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (piece.isFixed) return;

        dragStartPos = eventData.position;
        isDraggingPiece = false;
        // repassa para o scroll
        pieceScroll.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (piece.isFixed) return;

        Vector2 delta = eventData.position - dragStartPos;

        // Horizontal → scroll
        if (!isDraggingPiece && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            pieceScroll.OnDrag(eventData);
            return;
        }

        // Decide virar drag da peça
        if (!isDraggingPiece && Mathf.Abs(delta.y) > dragThreshold)
        {
            StartPieceDrag();
            isDraggingPiece = true;
            return;
        }

        if (isDraggingPiece)
        {
            rectTransform.anchoredPosition +=
                eventData.delta / canvas.scaleFactor;
        }
    }

    void StartPieceDrag()
    {
        startParent = rectTransform.parent;
        startSiblingIndex = rectTransform.GetSiblingIndex();
        startPosition = rectTransform.anchoredPosition;

        // evita que o layout influencie durante o drag
        if (layoutElement != null)
            layoutElement.ignoreLayout = true;

        rectTransform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (piece.isFixed) return;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        if (!isDraggingPiece)
        {
            pieceScroll.OnEndDrag(eventData);
            return;
        }
        PuzzleSlot slot = GetSlotUnderPointer(eventData);
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
    public void ReturnToStart()
    {
        rectTransform.SetParent(startParent);

        // restaura posição correta no layout
        rectTransform.SetSiblingIndex(startSiblingIndex);

        if (layoutElement != null)
            layoutElement.ignoreLayout = false;

        ResetAnchorsToCenter();
        rectTransform.anchoredPosition = startPosition;
    }

    void ResetAnchorsToCenter()
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
}
