using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    public CardUI placedCard;

    public bool IsEmpty => placedCard == null;

    public void PlaceCard(CardData cardData, GameObject cardPrefab)
    {
        if (!IsEmpty)
            return; // já ocupado

        var cardObj = Instantiate(cardPrefab, transform);
        placedCard = cardObj.GetComponent<CardUI>();
        placedCard.SetCard(cardData);

        // Centraliza dentro do slot
        cardObj.transform.localPosition = Vector3.zero;
    }
}
