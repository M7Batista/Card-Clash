using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CollectionScreen : MonoBehaviour
{
    [Header("Galeria")]
    public Transform scrollContent;
    public GameObject cardPrefab;
    public ScrollRect scrollRect;

    [Header("Preview")]
    public GameObject previewPanel;
    public Image previewImage;

    [Header("Preview Status")]
    public GameObject statusPanel;
    public TextMeshProUGUI numTop, numRight, numBottom, numLeft;
    public TextMeshProUGUI characterName;
    public RadarPolygon radarPolygon;

    [Header("UI Extra")]
    public TextMeshProUGUI totalCardsText;

    private int totalCards = 0;

    // 🔹 Lista de cartas que o jogador possui
    private List<CardData> playerOwnedCards = new List<CardData>();

    private void Start()
    {
        previewPanel.SetActive(false);

        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

        LoadCards();
    }

    private void LoadCards()
    {
        // 🔹 Carregar todas as cartas do jogo
        CardData[] allCards = Resources.LoadAll<CardData>("cards");
        totalCards = allCards.Length;


        foreach (CardData card in allCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, scrollContent);
            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.SetCard(card, Owner.None);

            // clique no card da coleção
            Button btn = cardGO.GetComponent<Button>();
            if (btn == null) btn = cardGO.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCollectionCardClicked(cardUI));   
        }

        // 🔹 Atualiza o contador X/Y
        if (totalCardsText != null)
            totalCardsText.text = $"{playerOwnedCards.Count}/{totalCards}";
    }
    private void OnCollectionCardClicked(CardUI cardUI)
    {
        ShowCard(cardUI.cardData);
    }

    public void ShowCard(CardData cardData)
    {
        previewImage.sprite = cardData.artwork;
        previewPanel.SetActive(true);
        statusPanel.SetActive(true);
        if (numTop) numTop.text = cardData.top.ToString();
        if (numRight) numRight.text = cardData.right.ToString();
        if (numBottom) numBottom.text = cardData.bottom.ToString();
        if (numLeft) numLeft.text = cardData.left.ToString();
        if (characterName) characterName.text = cardData.cardName;

        if (radarPolygon != null)
        {
            radarPolygon.top = cardData.top;
            radarPolygon.right = cardData.right;
            radarPolygon.bottom = cardData.bottom;
            radarPolygon.left = cardData.left;
            radarPolygon.SetVerticesDirty();
        }

        var zoom = previewPanel.transform.GetChild(0).GetComponent<CardZoom>();
        if (zoom != null) zoom.ResetZoom();
    }

}
