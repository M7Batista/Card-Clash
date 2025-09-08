using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardGallery : MonoBehaviour
{
    [Header("Galeria")]
    public Transform gridParent;
    public GameObject cardPrefab;
    public ScrollRect scrollRect;

    [Header("Preview")]
    public GameObject previewPanel;
    public Image previewImage;
    public Button closeButton;
    public Button statusButton;

    [Header("Preview Status")]
    public GameObject statusPanel;
    public TextMeshProUGUI numTop, numRight, numBottom, numLeft;
    public TextMeshProUGUI characterName;
    public RadarPolygon radarPolygon;

    [Header("UI Extra")]
    public TextMeshProUGUI totalCardsText;

    [Header("Mobile")]
    public float tapMoveThreshold = 20f;
    public float tapTimeThreshold = 0.3f;

    private int totalCards = 0;

    // 🔹 Lista de cartas que o jogador possui
    private List<CardData> playerOwnedCards = new List<CardData>();

    private void Start()
    {
        if (statusButton != null)
            statusButton.onClick.AddListener(ToggleStatusPanel);
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePreview);

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

        // 🔹 Carregar do PlayerDeckManager as cartas que o jogador possui
        List<int> ownedIds = PlayerDeckManager.GetOrCreateDeck(new List<CardData>(allCards));
        playerOwnedCards = PlayerDeckManager.ConvertToCards(ownedIds, new List<CardData>(allCards));

        foreach (CardData data in allCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, gridParent);

            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.SetCard(data, Owner.None);

            Image img = cardGO.GetComponent<Image>();
            if (img == null) img = cardGO.AddComponent<Image>();
            img.raycastTarget = true;
            if (img.sprite == null) img.color = Color.white;

            bool playerHasCard = playerOwnedCards.Exists(c => c.cardId == data.cardId);
            cardUI.SetEnabledState(playerHasCard);

            if (playerHasCard)
            {
                var touch = cardGO.AddComponent<CardTouchHandler>();
                touch.Setup(this, scrollRect, data, tapMoveThreshold, tapTimeThreshold);
            }
        }

        // 🔹 Atualiza o contador X/Y
        if (totalCardsText != null)
            totalCardsText.text = $"{playerOwnedCards.Count}/{totalCards}";
    }


    public void ShowCard(CardData cardData)
    {
        previewImage.sprite = cardData.artwork;
        previewPanel.SetActive(true);

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

    public void ClosePreview()
    {
        previewPanel.SetActive(false);
    }

    public void ToggleStatusPanel()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(!statusPanel.activeSelf);
        }
    }
}


public class CardTouchHandler : MonoBehaviour,
    IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    private ScrollRect parentScroll;
    private CardGallery gallery;
    private CardData cardData;   // 🔹 Agora guarda o CardData completo

    private bool dragging;
    private Vector2 downPos;
    private float downTime;
    private float moveThreshold = 20f;
    private float timeThreshold = 0.3f;

    public void Setup(CardGallery gallery, ScrollRect scroll, CardData cardData, float movePx, float timeSecs)
    {
        this.gallery = gallery;
        this.parentScroll = scroll != null ? scroll : GetComponentInParent<ScrollRect>();
        this.cardData = cardData;
        this.moveThreshold = movePx;
        this.timeThreshold = timeSecs;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = false;
        downPos = eventData.position;
        downTime = Time.unscaledTime;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (dragging) return;

        float dist = Vector2.Distance(downPos, eventData.position);
        float dt = Time.unscaledTime - downTime;

        if (dist <= moveThreshold && dt <= timeThreshold)
        {
            if (gallery != null && cardData != null)
                gallery.ShowCard(cardData);
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        parentScroll?.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        parentScroll?.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        parentScroll?.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        parentScroll?.OnEndDrag(eventData);
    }
}
