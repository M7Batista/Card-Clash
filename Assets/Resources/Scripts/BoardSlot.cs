using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    public bool isOccupied = false;

    public void PlaceCard(GameObject card, Owner owner)
    {
        if (isOccupied) return;

        card.transform.SetParent(transform, false);
        card.transform.localPosition = Vector3.zero;
        isOccupied = true;

        var cardUI = card.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.SetCard(cardUI.cardData, owner);
        }
    }
}
