using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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
    public RadarPolygon radarPolygon;   // 🔹 Referência ao RadarPolygon no painel de preview

    [Header("UI Extra")]
    public TextMeshProUGUI totalCardsText;

    [Header("Mobile")]
    [Tooltip("Pixels de movimento para o tap ser considerado clique (menor = mais sensível)")]
    public float tapMoveThreshold = 20f;
    [Tooltip("Tempo máx (segundos) para considerar um tap")]
    public float tapTimeThreshold = 0.3f;

    private int totalCards = 0;

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
        CardData[] allCards = Resources.LoadAll<CardData>("cards");
        totalCards = allCards.Length;

        foreach (CardData data in allCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, gridParent);

            // Configura os dados visuais do card
            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.SetCard(data, Owner.None);

            // Garante um Image para receber eventos (Raycast)
            Image img = cardGO.GetComponent<Image>();
            if (img == null) img = cardGO.AddComponent<Image>();
            img.raycastTarget = true;
            if (img.sprite == null) img.color = Color.white;

            // 🔹 Passa o CardData para o handler (não só o sprite)
            var touch = cardGO.AddComponent<CardTouchHandler>();
            touch.Setup(this, scrollRect, data, tapMoveThreshold, tapTimeThreshold);
        }

        if (totalCardsText != null)
            totalCardsText.text = $"Characters {totalCards}";
    }

    // 🔹 Agora recebe o CardData completo
    public void ShowCard(CardData cardData)
    {
        previewImage.sprite = cardData.artwork;
        previewPanel.SetActive(true);

        // Atualiza os textos
        if (numTop) numTop.text = cardData.top.ToString();
        if (numRight) numRight.text = cardData.right.ToString();
        if (numBottom) numBottom.text = cardData.bottom.ToString();
        if (numLeft) numLeft.text = cardData.left.ToString();
        if (characterName) characterName.text = cardData.cardName;

        // Atualiza RadarPolygon
        if (radarPolygon != null)
        {
            radarPolygon.top = cardData.top;
            radarPolygon.right = cardData.right;
            radarPolygon.bottom = cardData.bottom;
            radarPolygon.left = cardData.left;
            radarPolygon.SetVerticesDirty(); // 🔹 Força redesenho
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
