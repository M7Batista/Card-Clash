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
    public TextMeshProUGUI characterName, txtPower, txtID;
    public RadarPolygon radarPolygon;

    [Header("UI Extra")]
    public TextMeshProUGUI totalCardsText;
    private int totalCards = 0;
    public List<CardData> playerOwnedCards = new List<CardData>();
    public static CollectionScreen Instance;
    private void Awake() => Instance = this;

    void OnEnable()
    {

        Debug.Log("CollectionScreen Start");
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

        foreach (CardData card in allCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, scrollContent);
            cardGO.name = $"{card.id}";
            CardUI cardUI = cardGO.GetComponent<CardUI>();

            // 🔹 Se o jogador possui a carta → mostra normal
            if (playerOwnedCards.Contains(card))
            {
                cardUI.SetCard(card, Owner.None);
            }
            else
            {
                // 🔹 Caso contrário → mostra verso
                //cardUI.ShowBack();
                cardUI.SetCard(card, Owner.None);
            }

            // clique no card da coleção
            Button btn = cardGO.GetComponent<Button>();
            if (btn == null) btn = cardGO.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCollectionCardClicked(cardUI));
        }

        // 🔹 Atualiza o contador
        if (totalCardsText != null)
            totalCardsText.text = $"Cards colleted {playerOwnedCards.Count} / {totalCards}";
    }

    private void OnCollectionCardClicked(CardUI cardUI)
    {
        // só abre preview se o jogador tiver a carta
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
        txtPower.text = $"{power}";
        txtID.text = $"{cardData.id}";
        var zoom = previewPanel.transform.GetChild(0).GetComponent<CardZoom>();
        if (zoom != null) zoom.ResetZoom();
    }
    string ConvertToString(int value)
    {
        string result = "";
        if (value == 10)
        {
            result = "A";
        }
        else if (value == 11)
        {
            result = "B";
        }

        else
        {
            result = value.ToString();
        }
        return result;
    }
}
