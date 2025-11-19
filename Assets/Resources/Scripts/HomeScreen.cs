using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class HomeScreen : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TextMeshProUGUI txtTickets;

    [Header("Background (Image UI)")]
    public RectTransform backgroundRect;      // atribuir o RectTransform da Image do background
    public Image backgroundImage;             // atribuir a Image (opcional, usado para pegar sprite aspect)
    [Tooltip("Margem máxima além das bordas (opcional)")]
    public float extraHorizontalMargin = 0f;

    [Header("Personagem (Image UI)")]
    public Image characterImage;              // personagem que deve ficar centralizado no background

    [Header("Transições")]
    public float fadeDuration = 0.8f;

    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private Vector2 dragStartPointerLocal;
    private Vector2 bgStartAnchored;
    private float minX;
    private float maxX;

    void OnEnable()
    {
        LoadCharacterImage();
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.menuMusic);
        if (BattleTicketSystem.Instance != null && txtTickets != null)
            txtTickets.text = $"{BattleTicketSystem.Instance.GetCurrentTickets()}/30";

        SetupBackgroundScaleAndLimits();
        Debug.Log("HomeScreen ativada.");
    }

    void LoadCharacterImage()
    {
        if (characterImage == null) return;

        if (!PlayerPrefs.HasKey("HomeCharacterID")) return;

        string characterID = PlayerPrefs.GetString("HomeCharacterID");
        if (string.IsNullOrEmpty(characterID)) return;

        Sprite loadedSprite = Resources.Load<Sprite>($"Art/Artworks/{characterID}");
        if (loadedSprite != null)
        {
            characterImage.sprite = loadedSprite;
            characterImage.enabled = true;
            StartCoroutine(FadeInImage());
        }
        else
        {
            characterImage.enabled = false;
            Debug.LogWarning($"Personagem não encontrado: {characterID}");
        }
    }

    IEnumerator FadeInImage()
    {
        if (characterImage == null) yield break;
        Color color = characterImage.color;
        color.a = 0f;
        characterImage.color = color;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            characterImage.color = color;
            yield return null;
        }

        color.a = 1f;
        characterImage.color = color;
    }

    // Ajusta o background para ocupar toda a altura visível do Canvas mantendo a proporção da sprite
    private void SetupBackgroundScaleAndLimits()
    {
        if (backgroundRect == null)
        {
            Debug.LogWarning("backgroundRect não atribuído.");
            return;
        }

        parentCanvas = backgroundRect.GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogWarning("Canvas pai não encontrado para backgroundRect.");
            return;
        }

        canvasRect = parentCanvas.GetComponent<RectTransform>();

        // Altura disponível na view (UI units)
        float viewHeight = canvasRect.rect.height;

        // Calcula proporção do sprite. Se não houver sprite, assume 16:9 (1920/1080)
        float spriteAspect = 1920f / 1080f;
        if (backgroundImage != null && backgroundImage.sprite != null)
        {
            var sp = backgroundImage.sprite;
            if (sp.rect.height > 0)
                spriteAspect = (float)sp.rect.width / sp.rect.height;
        }

        // Define tamanho do background: altura = viewHeight, largura proporcional
        float targetHeight = viewHeight;
        float targetWidth = targetHeight * spriteAspect;

        // Aplica ao RectTransform (centrado)
        //backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        //backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        //backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        //backgroundRect.sizeDelta = new Vector2(targetWidth, targetHeight);
        //backgroundRect.anchoredPosition = Vector2.zero;

        // Calcula limites de arrasto horizontal (mantendo background centrado)
        float viewWidth = canvasRect.rect.width;
        float halfDiff = Mathf.Max(0f, (targetWidth - viewWidth) * 0.5f);
        minX = -halfDiff - extraHorizontalMargin;
        maxX = halfDiff + extraHorizontalMargin;
    }

    // BeginDrag/Drag/EndDrag utilizando conversão para coordenadas locais do Canvas (funciona com Screen Space - Camera)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (backgroundRect == null || canvasRect == null || parentCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, parentCanvas.worldCamera, out dragStartPointerLocal);
        bgStartAnchored = backgroundRect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (backgroundRect == null || canvasRect == null || parentCanvas == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, parentCanvas.worldCamera, out Vector2 currentLocal)) return;

        Vector2 delta = currentLocal - dragStartPointerLocal;
        float newX = bgStartAnchored.x + delta.x;

        newX = Mathf.Clamp(newX, minX, maxX);
        backgroundRect.anchoredPosition = new Vector2(newX, bgStartAnchored.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // opcional: snap/inércia
    }
}