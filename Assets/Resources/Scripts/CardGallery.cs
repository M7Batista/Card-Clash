using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardGallery : MonoBehaviour
{
    [Header("Galeria")]
    public Transform gridParent;      // GridLayoutGroup (filho de Viewport/Content)
    public GameObject cardPrefab;     // Prefab do card (tem CardUI)
    public ScrollRect scrollRect;     // ScrollRect vertical da galeria

    [Header("Preview")]
    public GameObject previewPanel;   // Painel overlay (fullscreen)
    public Image previewImage;        // Imagem grande
    public Button closeButton;        // Botão para fechar

    [Header("Mobile")]
    [Tooltip("Pixels de movimento para o tap ser considerado clique (menor = mais sensível)")]
    public float tapMoveThreshold = 20f;
    [Tooltip("Tempo máx (segundos) para considerar um tap")]
    public float tapTimeThreshold = 0.3f;

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePreview);

        previewPanel.SetActive(false);

        // Garante Scroll só vertical
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
            if (img.sprite == null) img.color = Color.white; // visível se não houver sprite base

            // Adiciona o handler que encaminha drag ao ScrollRect e detecta tap
            var touch = cardGO.AddComponent<CardTouchHandler>();
            touch.Setup(this, scrollRect, data.artwork, tapMoveThreshold, tapTimeThreshold);
        }
    }

    public void ShowCard(Sprite cardSprite)
    {
        previewImage.sprite = cardSprite;
        previewPanel.SetActive(true);

        // Reinicia o zoom (se houver CardZoom no 1º filho)
        var zoom = previewPanel.transform.GetChild(0).GetComponent<CardZoom>();
        if (zoom != null) zoom.ResetZoom();
    }

    public void ClosePreview()
    {
        previewPanel.SetActive(false);
    }
}

/// <summary>
/// Encaminha drag para o ScrollRect pai e abre o preview somente em TAP (não-drag).
/// </summary>
public class CardTouchHandler : MonoBehaviour,
    IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    private ScrollRect parentScroll;
    private CardGallery gallery;
    private Sprite artwork;

    private bool dragging;
    private Vector2 downPos;
    private float downTime;
    private float moveThreshold = 20f;   // px
    private float timeThreshold = 0.3f;  // s

    public void Setup(CardGallery gallery, ScrollRect scroll, Sprite artwork, float movePx, float timeSecs)
    {
        this.gallery = gallery;
        this.parentScroll = scroll != null ? scroll : GetComponentInParent<ScrollRect>();
        this.artwork = artwork;
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
        if (dragging) return; // se arrastou, não é clique

        float dist = Vector2.Distance(downPos, eventData.position);
        float dt = Time.unscaledTime - downTime;

        if (dist <= moveThreshold && dt <= timeThreshold)
        {
            // TAP → abre preview
            if (gallery != null && artwork != null)
                gallery.ShowCard(artwork);
        }
    }

    // Encaminha eventos de drag para o ScrollRect (isso faz o scroll funcionar em mobile)
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (parentScroll != null)
            parentScroll.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        if (parentScroll != null)
            parentScroll.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentScroll != null)
            parentScroll.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScroll != null)
            parentScroll.OnEndDrag(eventData);
    }
}
