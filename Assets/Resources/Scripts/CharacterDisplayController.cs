using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

/// <summary>
/// Controlador reutilizável para exibir vídeo ou imagem de personagens.
/// Pode ser usado em múltiplas telas (HomeScreen, CardView, etc).
/// </summary>
public class CharacterDisplayController : MonoBehaviour
{
    [Header("Configurações de Pasta")]
    [SerializeField] private string characterVideoFolder = "Art/Videos";
    [SerializeField] private string characterArtworkFolder = "Art/Artworks";
    [SerializeField] private bool autoCreateObjects = true;
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("Referências")]
    [SerializeField] private Transform characterContainer; // Container para agrupar vídeo/imagem
    private RawImage characterVideoImage;
    private VideoPlayer characterVideoPlayer;
    private Image characterFallbackImage;
    private RenderTexture characterRenderTexture;

    public void LoadCharacter(string characterID)
    {
        EnsureComponents();

        if (characterVideoImage == null || characterVideoPlayer == null)
        {
            Debug.LogWarning("Componentes de vídeo não foram encontrados.");
            return;
        }

        if (string.IsNullOrEmpty(characterID))
        {
            Debug.LogWarning("ID do personagem inválido.");
            return;
        }

        Debug.Log($"Tentando carregar vídeo para o personagem: {characterID}");
        VideoClip loadedClip = Resources.Load<VideoClip>($"{characterVideoFolder}/{characterID}");
        
        if (loadedClip != null)
        {
            DisplayVideo(loadedClip);
        }
        else
        {
            Debug.LogWarning($"Vídeo não encontrado: {characterVideoFolder}/{characterID}. Tentando carregar imagem como fallback...");
            DisplayImage(characterID);
        }
    }

    private void DisplayVideo(VideoClip clip)
    {
        PrepareVideoOutput(clip);
        
        // Ativar o VideoPlayer antes de reproduzir
        characterVideoPlayer.enabled = true;
        characterVideoPlayer.gameObject.SetActive(true);
        characterVideoPlayer.Play();
        
        characterVideoImage.enabled = true;
        characterVideoImage.gameObject.SetActive(true);
        DisableFallbackImage();
        
        StartCoroutine(FadeInGraphic(characterVideoImage));
    }

    private void DisplayImage(string characterID)
    {
        Sprite loadedSprite = Resources.Load<Sprite>($"{characterArtworkFolder}/{characterID}");
        
        if (loadedSprite != null)
        {
            if (characterFallbackImage == null)
            {
                CreateFallbackImage();
            }

            characterVideoImage.enabled = false;
            characterVideoImage.gameObject.SetActive(false);
            characterFallbackImage.sprite = loadedSprite;
            characterFallbackImage.enabled = true;
            StartCoroutine(DelayedResizeForImage(loadedSprite));
            StartCoroutine(FadeInGraphic(characterFallbackImage));
            Debug.Log($"Imagem carregada com sucesso: {characterArtworkFolder}/{characterID}");
        }
        else
        {
            characterFallbackImage.enabled = false;
            Debug.LogWarning($"Imagem não encontrada: {characterArtworkFolder}/{characterID}");
        }
    }

    public void StopVideo()
    {
        if (characterVideoPlayer != null)
        {
            characterVideoPlayer.Stop();
            characterVideoPlayer.enabled = false;
        }
    }

    private void EnsureComponents()
    {
        // Criar ou encontrar o container dedicado
        if (characterContainer == null && autoCreateObjects)
        {
            GameObject containerObject = new GameObject("CharacterContainer");
            containerObject.transform.SetParent(transform, false);
            
            RectTransform containerRect = containerObject.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            characterContainer = containerObject.transform;
        }

        if (characterVideoImage == null && characterContainer != null)
        {
            characterVideoImage = characterContainer.GetComponentInChildren<RawImage>(true);
        }

        if (characterVideoPlayer == null && characterContainer != null)
        {
            characterVideoPlayer = characterContainer.GetComponentInChildren<VideoPlayer>(true);
        }

        if (characterVideoImage == null && autoCreateObjects && characterContainer != null)
        {
            GameObject videoObject = new GameObject("CharacterVideo");
            videoObject.transform.SetParent(characterContainer, false);

            RectTransform rectTransform = videoObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            characterVideoImage = videoObject.AddComponent<RawImage>();
            characterVideoImage.color = Color.white;
            characterVideoImage.raycastTarget = false;
            characterVideoImage.transform.SetAsFirstSibling();
        }

        if (characterVideoPlayer == null && autoCreateObjects && characterVideoImage != null)
        {
            characterVideoPlayer = characterVideoImage.gameObject.AddComponent<VideoPlayer>();
        }

        if (characterVideoImage != null)
        {
            characterVideoImage.enabled = false;
        }
    }

    private void CreateFallbackImage()
    {
        if (characterFallbackImage != null) return;

        GameObject fallbackObject = new GameObject("CharacterFallbackImage");
        fallbackObject.transform.SetParent(characterContainer != null ? characterContainer : transform, false);

        RectTransform rectTransform = fallbackObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        characterFallbackImage = fallbackObject.AddComponent<Image>();
        characterFallbackImage.color = Color.white;
        characterFallbackImage.raycastTarget = false;
        characterFallbackImage.transform.SetAsFirstSibling();
        characterFallbackImage.enabled = false;
    }

    private void DisableFallbackImage()
    {
        if (characterFallbackImage != null)
        {
            characterFallbackImage.enabled = false;
        }
    }

    private void PrepareVideoOutput(VideoClip clip)
    {
        if (characterVideoImage == null || characterVideoPlayer == null || clip == null)
        {
            return;
        }

        if (characterRenderTexture != null)
        {
            characterRenderTexture.Release();
            Destroy(characterRenderTexture);
        }

        characterRenderTexture = new RenderTexture((int)clip.width, (int)clip.height, 0, RenderTextureFormat.ARGB32);
        characterRenderTexture.Create();

        characterVideoPlayer.source = VideoSource.VideoClip;
        characterVideoPlayer.clip = clip;
        characterVideoPlayer.playOnAwake = false;
        characterVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        characterVideoPlayer.targetTexture = characterRenderTexture;
        characterVideoPlayer.isLooping = true;
        characterVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        characterVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        characterVideoImage.texture = characterRenderTexture;
        characterVideoImage.color = Color.white;
        characterVideoImage.transform.SetAsFirstSibling();
        StartCoroutine(DelayedResizeForVideo(clip));
    }

    private void ResizeVideoToFitScreen(VideoClip clip)
    {
        if (characterVideoImage == null) return;

        RectTransform imageRect = characterVideoImage.rectTransform;
        RectTransform parentRect = imageRect.parent as RectTransform;
        if (parentRect == null) return;

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;
        float aspectRatio = (float)clip.width / Mathf.Max(1f, clip.height);

        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;

        float targetHeight = parentHeight;
        float targetWidth = targetHeight * aspectRatio;

        imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }

    private void ResizeImageToFitScreen(Sprite sprite)
    {
        if (characterFallbackImage == null) return;

        RectTransform imageRect = characterFallbackImage.rectTransform;
        RectTransform parentRect = imageRect.parent as RectTransform;
        if (parentRect == null) return;

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        if (sprite.rect.height == 0) return;
        float aspectRatio = sprite.rect.width / sprite.rect.height;

        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;

        float targetHeight = parentHeight;
        float targetWidth = targetHeight * aspectRatio;

        imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }

    private IEnumerator DelayedResizeForVideo(VideoClip clip)
    {
        yield return new WaitForEndOfFrame();
        ResizeVideoToFitScreen(clip);
    }

    private IEnumerator DelayedResizeForImage(Sprite sprite)
    {
        yield return new WaitForEndOfFrame();
        ResizeImageToFitScreen(sprite);
    }

    private IEnumerator FadeInGraphic(Graphic graphic)
    {
        if (graphic == null) yield break;

        Color color = graphic.color;
        color.a = 0f;
        graphic.color = color;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            graphic.color = color;
            yield return null;
        }

        color.a = 1f;
        graphic.color = color;
    }
}
