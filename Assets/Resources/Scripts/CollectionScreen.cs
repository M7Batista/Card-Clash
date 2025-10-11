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
    public GameObject floatingMessagePrefab;
    public Transform uiCanvas;

    [Header("Preview")]
    public GameObject previewPanel;
    public Image previewImage;
    public GameObject panelTop, panelBottom;
    public TextMeshProUGUI numTop, numRight, numBottom, numLeft;
    public TextMeshProUGUI characterName, characterRarity, txtID;
    public RadarPolygon radarPolygon;
    public Button fullScreenButton;
    public Button setBackgroundButton;
    string currentCardName;
    public Image backgroundImage;

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
            ShowCard(cardUI.cardData);
    }

    public void ShowCard(CardData cardData)
    {
        previewImage.sprite = cardData.artwork;
        previewPanel.SetActive(true);
        panelTop.SetActive(true);
        panelBottom.SetActive(true);

        if (numTop) numTop.text = ConvertToString(cardData.top);
        if (numRight) numRight.text = ConvertToString(cardData.right);
        if (numBottom) numBottom.text = ConvertToString(cardData.bottom);
        if (numLeft) numLeft.text = ConvertToString(cardData.left);
        if (characterName) characterName.text = cardData.cardName;
        if (characterRarity)
        {
            characterRarity.text = cardData.rarity.ToString();

            switch (cardData.rarity)
            {
                case CardRarity.Common:
                    backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_common");
                    break;
                case CardRarity.Uncommon:
                    backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_uncommon");
                    break;
                case CardRarity.Rare:
                    backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_rare");
                    break;
                case CardRarity.Epic:
                    backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_epic");
                    break;
                case CardRarity.Legendary:
                    backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_legendary");
                    break;
                default:
                    backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_common");
                    break;
            }
        }

        if (radarPolygon != null)
        {
            radarPolygon.top = cardData.top;
            radarPolygon.right = cardData.right;
            radarPolygon.bottom = cardData.bottom;
            radarPolygon.left = cardData.left;
            radarPolygon.SetVerticesDirty();
        }

        currentCardName = cardData.cardName;

        txtID.text = $"{cardData.id}";

        var zoom = previewPanel.transform.GetChild(0).GetComponent<CardZoom>();
        if (zoom != null) zoom.ResetZoom();
        fullScreenButton.onClick.AddListener(FullScreen);
        setBackgroundButton.onClick.AddListener(AssignCharacterToHome);
    }

    string ConvertToString(int value)
    {
        if (value == 10) return "A";
        if (value == 11) return "B";
        return value.ToString();
    }
    void FullScreen()
    {
        panelTop.SetActive(!panelTop.activeSelf);
        panelBottom.SetActive(!panelBottom.activeSelf);

    }
    void AssignCharacterToHome()
    {
        if (string.IsNullOrEmpty(currentCardName))
        {
            Debug.LogWarning("Nenhum card selecionado para definir como personagem inicial!");
            return;
        }

        // Salva o card escolhido
        PlayerPrefs.SetString("HomeCharacterID", currentCardName);
        PlayerPrefs.Save();

        Debug.Log($"Card '{currentCardName}' set on home screen");
        GameObject go = Instantiate(floatingMessagePrefab, uiCanvas);
        go.transform.localPosition = Vector3.zero; // aparece no centro
        go.GetComponent<FloatingMessage>().Show($"Card '{currentCardName}' set on home screen");
    }
}
