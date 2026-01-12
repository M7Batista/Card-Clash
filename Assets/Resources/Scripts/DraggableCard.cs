using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    public Action<CardUI> OnCardPlaced;
    public static bool CanDrag = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
    }
    private int originalSiblingIndex;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag) return;
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // 🔹 Move pro topo do canvas pra ficar acima dos outros elementos
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.9f;

        //AudioManager.Instance?.PlaySFX("card-pickup");
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag) return;

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
        if (!CanDrag) return;
        Debug.Log("OnEndDrag chamado");

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        GameObject target = eventData.pointerEnter;
        Debug.Log("Carta solta sobre: " + (target != null ? target.name : "nada"));

        // 🔹 Verifica se soltou sobre um slot válido
        if (target != null && target.CompareTag("Slot"))
        {
            transform.SetParent(target.transform, false);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            var cardUI = GetComponent<CardUI>();
            OnCardPlaced?.Invoke(cardUI);
            BattleCardScreen.Instance.OnPlayerCardPlaced(cardUI);
            AudioManager.Instance?.PlaySFX("card-slide-1");

            Destroy(this); // impede novo arraste após colocação
        }
        else
        {
            // 🔹 Volta para a mão do jogador com animação suave
            StartCoroutine(ReturnToHand());
            Debug.Log("Carta retornou para a mão.");
        }
    }

    private System.Collections.IEnumerator ReturnToHand()
    {
        transform.SetParent(originalParent, false);

        // 🔹 Garante que a carta volte para o final da fila (ou mantenha a ordem)
        transform.SetAsLastSibling();

        // 🔹 Aguarda um frame para o LayoutGroup recalcular
        yield return null;

        // 🔹 Força atualização manual do layout
        var layoutGroup = originalParent.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(originalParent as RectTransform);
        }

        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);

        //AudioManager.Instance?.PlaySFX("card-return");
    }

}
