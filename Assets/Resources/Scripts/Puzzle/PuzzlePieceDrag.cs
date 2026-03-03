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

    private Vector2 pointerOffset; // 🔥 offset do dedo
    private const float dragThreshold = 15f;

    [Header("Magnetismo")]
    [SerializeField] private float magnetRadius = 3f; // distância em pixels
    [SerializeField] private float magnetStrength = 25f; // suavidade
    private PuzzleSlot highlightedSlot;


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

        // repassa para o scroll inicialmente
        pieceScroll.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (piece.isFixed) return;

        Vector2 delta = eventData.position - dragStartPos;

        // 👉 Horizontal = scroll
        if (!isDraggingPiece && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            pieceScroll.OnDrag(eventData);
            return;
        }

        // 👉 Decide virar drag da peça
        if (!isDraggingPiece && Mathf.Abs(delta.y) > dragThreshold)
        {
            StartPieceDrag(eventData);
            isDraggingPiece = true;
            return;
        }

        if (isDraggingPiece)
        {
            DragPiece(eventData);
            ApplyMagnetism();
        }
    }
    void ApplyMagnetism()
    {
        if (piece.correctSlot == null) return;
        RectTransform slotRect =
            piece.correctSlot.GetComponent<RectTransform>();

        Vector2 distanceVector = slotRect.localPosition - rectTransform.localPosition;
        float distance = distanceVector.magnitude;

        bool insideMagnet = distance <= magnetRadius && distance > 0.1f;

        // 🔆 Highlight
        if (insideMagnet && highlightedSlot != piece.correctSlot)
        {
            ClearHighlight();
            highlightedSlot = piece.correctSlot;
            highlightedSlot.SetHighlight(true);
        }
        else if (!insideMagnet && highlightedSlot != null)
        {
            ClearHighlight();
        }

        // 🧲 Atração suave apenas quando muito perto (últimos pixels)
        if (insideMagnet && distance < magnetRadius * 0.5f)
        {
            rectTransform.localPosition = Vector2.Lerp(
                rectTransform.localPosition,
                slotRect.localPosition,
                magnetStrength * Time.deltaTime
            );
        }
    }

    void ClearHighlight()
    {
        if (highlightedSlot != null)
            highlightedSlot.SetHighlight(false);

        highlightedSlot = null;
    }



    void StartPieceDrag(PointerEventData eventData)
    {
        startParent = rectTransform.parent;
        startSiblingIndex = rectTransform.GetSiblingIndex();
        startPosition = rectTransform.anchoredPosition;

        // ignora layout durante drag
        if (layoutElement != null)
            layoutElement.ignoreLayout = true;

        rectTransform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;

        // 🔥 calcula offset correto do toque em relação ao canvas
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out pointerOffset
        );
        // Subtrai a posição atual para obter o offset relativo ao centro da peça
        pointerOffset -= (Vector2)rectTransform.localPosition;
    }

    void DragPiece(PointerEventData eventData)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        ))
        {
            rectTransform.localPosition = localPoint - pointerOffset;
        }
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
        if (slot != null && slot == piece.correctSlot && !slot.isOccupied)
        //if (slot != null && slot.index == piece.index && !slot.isOccupied)
        {
            SnapToSlot(slot);
            // O tamanho da peça recebe o mesmo tamanho do slot para garantir encaixe perfeito
            rectTransform.sizeDelta = slot.GetComponent<RectTransform>().sizeDelta;
        }
        else
        {
            ReturnToStart();
        }
        ClearHighlight();

    }

    // 🔍 Detecta slot via raycast de UI
    PuzzleSlot GetSlotUnderPointer(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            PuzzleSlot slot = result.gameObject.GetComponent<PuzzleSlot>();
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

        // 📳 VIBRAÇÃO LEVE AO ENCAIXAR
        TriggerHapticFeedback();
        PuzzleCompletionController.Instance.OnPieceFixed();
    }

    public void ReturnToStart()
    {
        rectTransform.SetParent(startParent);
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
    void TriggerHapticFeedback()
    {
#if UNITY_ANDROID || UNITY_IOS
    Handheld.Vibrate();
#endif
    }
}
