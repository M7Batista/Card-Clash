using UnityEngine;
using UnityEngine.EventSystems;

public class DeckSlot : MonoBehaviour, IDropHandler
{
    public CardUI CurrentCard { get; private set; }

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var cardUI = dragged.GetComponent<CardUI>();
        if (cardUI == null) return;

        // Se já tem carta no slot, substitui
        if (CurrentCard != null)
        {
            Destroy(CurrentCard.gameObject);
        }

        // Coloca a carta neste slot
        cardUI.transform.SetParent(transform, false);
        CurrentCard = cardUI;
    }

    public void SetCard(CardData data)
    {
        if (CurrentCard != null) Destroy(CurrentCard.gameObject);

        var go = Instantiate(DeckEditorUI.Instance.cardPrefab, transform);
        var cardUI = go.GetComponent<CardUI>();
        cardUI.SetCard(data, Owner.Player);

        CurrentCard = cardUI;
    }
}
