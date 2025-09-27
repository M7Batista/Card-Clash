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

    [Header("Preview")]
    public GameObject previewPanel;
    public Image previewImage;

    [Header("Preview Status")]
    public GameObject statusPanel;
    public TextMeshProUGUI numTop, numRight, numBottom, numLeft;
    public TextMeshProUGUI characterName, txtPower, txtID;
    public RadarPolygon radarPolygon;

    [Header("UI Extra")]
    public TextMeshProUGUI totalCardsText;
    public TMP_Dropdown sortDropdown;   // 🔹 Dropdown para escolher a ordenação
    private int totalCards = 0;
    public List<CardData> playerOwnedCards = new List<CardData>();

    private enum SortMode { ByID, ByName, ByRarity }
    private SortMode currentSort = SortMode.ByID;

    void OnEnable()
    {
        Debug.Log("CollectionScreen Start");
        previewPanel.SetActive(false);

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
            if (card != null) playerOwnedCards.Add(card);
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

        // 🔹 Instanciar cartas na ordem
        foreach (CardData card in allCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, scrollContent);
            cardGO.name = $"{card.id}";
            CardUI cardUI = cardGO.GetComponent<CardUI>();

            if (playerOwnedCards.Contains(card))
                cardUI.SetCard(card, Owner.None);
            else
                cardUI.ShowBack();
                //cardUI.SetCard(card, Owner.None); // aqui pode exibir bloqueada

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
            ShowCard(cardUI.cardData);
    }

    public void ShowCard(CardData cardData)
    {
        previewImage.sprite = cardData.artwork;
        previewPanel.SetActive(true);
        statusPanel.SetActive(true);

        if (numTop) numTop.text = ConvertToString(cardData.top);
        if (numRight) numRight.text = ConvertToString(cardData.right);
        if (numBottom) numBottom.text = ConvertToString(cardData.bottom);
        if (numLeft) numLeft.text = ConvertToString(cardData.left);
        if (characterName) characterName.text = cardData.cardName;

        if (radarPolygon != null)
        {
            radarPolygon.top = cardData.top;
            radarPolygon.right = cardData.right;
            radarPolygon.bottom = cardData.bottom;
            radarPolygon.left = cardData.left;
            radarPolygon.SetVerticesDirty();
        }

        int power = cardData.top + cardData.right + cardData.bottom + cardData.left;
        //txtPower.text = $"{power}";
        txtID.text = $"{cardData.id}";

        var zoom = previewPanel.transform.GetChild(0).GetComponent<CardZoom>();
        if (zoom != null) zoom.ResetZoom();
    }

    string ConvertToString(int value)
    {
        if (value == 10) return "A";
        if (value == 11) return "B";
        return value.ToString();
    }
}
