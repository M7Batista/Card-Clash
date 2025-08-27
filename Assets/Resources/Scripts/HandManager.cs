using UnityEngine;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public CardData[] startingHand; // Defina no Inspector quais cartas iniciais

    void Start()
    {
        foreach (CardData card in startingHand)
        {
            var cardObj = Instantiate(cardPrefab, transform);
            var cardUI = cardObj.GetComponent<CardUI>();
            cardUI.SetCard(card);
        }
    }
}
