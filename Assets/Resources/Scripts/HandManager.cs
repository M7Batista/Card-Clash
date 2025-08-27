using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public Transform handArea;       // referência ao layout da mão (UI)
    public GameObject cardPrefab;    // prefab da carta
    public List<CardUI> cardsInHand = new List<CardUI>();

    public void AddCard(CardData cardData, bool isDraggable = true)
    {
        Debug.Log("adicionando as cartas");
        var cardObj = Instantiate(cardPrefab, handArea);
        var cardUI = cardObj.GetComponent<CardUI>();
        cardUI.SetCard(cardData);

        if (isDraggable)
        {
            cardObj.AddComponent<DraggableCard>();
        }

        cardsInHand.Add(cardUI);
    }

    public void ClearHand()
    {
        foreach (var card in cardsInHand)
        {
            Destroy(card.gameObject);
        }
        cardsInHand.Clear();
    }
}
