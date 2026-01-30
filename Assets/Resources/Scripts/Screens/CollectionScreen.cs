using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CollectionScreen : MonoBehaviour
{
    [Header("Scroll View")]
    public ScrollRect scrollRect;
    public Transform scrollContent;
    public GameObject cardPrefab;

    [Header("UI Extra")]
    public TextMeshProUGUI totalCardsText;
    public TMP_Dropdown sortDropdown;   // 🔹 Dropdown para escolher a ordenação
    private int totalCards = 0;
    public List<CardData> playerOwnedCards = new List<CardData>();

    private enum SortMode { ByID, ByName, ByRarity }
    private SortMode currentSort = SortMode.ByID;
    public GameObject cardViewPanel;

    void OnEnable()
    {
        Debug.Log("CollectionScreen Start");
        cardViewPanel.SetActive(false);

        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }

        LoadCards();
    }

    private void OnDisable()
    {
        if (sortDropdown != null)
            sortDropdown.onValueChanged.RemoveListener(OnSortChanged);
    }

    private void OnSortChanged(int index)
    {
        currentSort = (SortMode)index;
        ReloadCards();
    }

    private void ReloadCards()
    {
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
        LoadCards();
    }

    private void LoadCards()
    {
        // Carregar cartas que o jogador possui
        playerOwnedCards.Clear();
        List<int> ownedIds = PlayerDeckManager.GetOwnedCards();

        foreach (int id in ownedIds)
        {
            var card = PlayerDeckManager.GetCardById(id);
            if (card != null)
                playerOwnedCards.Add(card);
        }

        // 🔹 Carregar todas as cartas do jogo
        CardData[] allCards = Resources.LoadAll<CardData>("cards");
        totalCards = allCards.Length;

        // 🔹 Aplicar ordenação
        System.Array.Sort(allCards, (a, b) =>
        {
            switch (currentSort)
            {
                case SortMode.ByName:
                    return a.cardName.CompareTo(b.cardName);
                case SortMode.ByRarity:
                    int rarityCompare = a.rarity.CompareTo(b.rarity);
                    if (rarityCompare == 0)
                        return a.id.CompareTo(b.id);
                    return rarityCompare;
                case SortMode.ByID:
                default:
                    return a.id.CompareTo(b.id);
            }
        });
        // Limpa o scroll content
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
        // 🔹 Instanciar cartas na ordem
        foreach (CardData card in allCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, scrollContent);
            cardGO.name = $"{card.id}";
            CardUI cardUI = cardGO.GetComponent<CardUI>();

            // ✅ Corrigido: compara pelo ID e não pela referência
            bool playerHasThisCard = ownedIds.Contains(card.id);

            if (playerHasThisCard)
                cardUI.SetCard(card, Owner.None);
            else
                cardUI.ShowBack();

            Button btn = cardGO.GetComponent<Button>();
            if (btn == null) btn = cardGO.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCollectionCardClicked(cardUI));
        }

        if (totalCardsText != null)
            totalCardsText.text = $"{playerOwnedCards.Count} / {totalCards}";
    }

    private void OnCollectionCardClicked(CardUI cardUI)
    {
        if (playerOwnedCards.Contains(cardUI.cardData))
        {
            cardViewPanel.SetActive(true);
            CardView.Instance.ShowCard(cardUI.cardData);
        }
    }

}
