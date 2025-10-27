using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BannerSystem : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform bannerContainer;
    public RectTransform dotContainer;

    [Header("Config")]
    public float slideInterval = 4f;
    public float slideSpeed = 8f;

    private int currentIndex = 0;
    private List<Image> dotImages = new List<Image>();
    private Coroutine autoSlideRoutine;
    private bool userDragging = false;
    private void Awake()
    {
        

        // se o ScrollRect existir e não for o mesmo GameObject deste componente,
        // adiciona/encaminha eventos BeginDrag/EndDrag para este BannerSystem
        if (scrollRect != null && scrollRect.gameObject != this.gameObject)
        {
            var et = scrollRect.gameObject.GetComponent<EventTrigger>() ?? scrollRect.gameObject.AddComponent<EventTrigger>();

            bool hasBegin = false, hasEnd = false;
            foreach (var entry in et.triggers)
            {
                if (entry.eventID == EventTriggerType.BeginDrag) hasBegin = true;
                if (entry.eventID == EventTriggerType.EndDrag) hasEnd = true;
            }

            if (!hasBegin)
            {
                var begin = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
                begin.callback.AddListener((data) => OnBeginDrag((PointerEventData)data));
                et.triggers.Add(begin);
            }

            if (!hasEnd)
            {
                var end = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
                end.callback.AddListener((data) => OnEndDrag((PointerEventData)data));
                et.triggers.Add(end);
            }
        }
    }
    private void Start()
    {
        SetupDots();
        UpdateDots();

        scrollRect.horizontal = true;
        scrollRect.vertical = false;

        // Garante início no primeiro banner
        StartCoroutine(SetInitialPosition());

        // Inicia o auto-slide
        autoSlideRoutine = StartCoroutine(AutoSlide());
    }

    private IEnumerator SetInitialPosition()
    {
        yield return null; // Espera o layout atualizar
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }


    private void SetupDots()
    {
        // Agora os dots devem ser configurados manualmente no inspector (cada filho de dotContainer)
        dotImages.Clear();

        int bannerCount = bannerContainer.childCount;
        int dotCount = dotContainer.childCount;
        if (dotCount != bannerCount)
        {
            Debug.LogWarning($"[BannerSystem] dotContainer child count ({dotCount}) != banner count ({bannerCount}). Dots should match banners.");
        }

        foreach (Transform t in dotContainer)
        {
            Image img = t.GetComponent<Image>();
            if (img != null)
                dotImages.Add(img);
            else
            {
                Debug.LogWarning("[BannerSystem] Dot child missing Image component. Placeholder added.");
                dotImages.Add(null);
            }
        }

        // Se houver menos dots que banners, completa com nulls para evitar erros de índice
        while (dotImages.Count < bannerCount)
            dotImages.Add(null);
    }


    private void UpdateDots()
    {
        for (int i = 0; i < dotImages.Count; i++)
        {
            dotImages[i].color = (i == currentIndex) ? Color.white : new Color(1, 1, 1, 0.3f);
        }
    }

    private IEnumerator AutoSlide()
    {
        while (true)
        {
            yield return new WaitForSeconds(slideInterval);

            // 🔹 só avança se o usuário não estiver interagindo
            if (!userDragging)
            {
                currentIndex = (currentIndex + 1) % bannerContainer.childCount;
                yield return StartCoroutine(SmoothSlideTo(currentIndex));
                UpdateDots();
            }
        }
    }

    private IEnumerator SmoothSlideTo(int targetIndex)
    {
        float target = (float)targetIndex / (bannerContainer.childCount - 1);
        float elapsed = 0f;
        Vector2 start = scrollRect.normalizedPosition;
        Vector2 end = new Vector2(target, 0);

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * slideSpeed;
            scrollRect.normalizedPosition = Vector2.Lerp(start, end, Mathf.SmoothStep(0, 1, elapsed));
            yield return null;
        }

        scrollRect.normalizedPosition = end;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        userDragging = true;
        // 🔹 Pausa o auto-slide imediatamente
        if (autoSlideRoutine != null)
            StopCoroutine(autoSlideRoutine);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 🔹 Calcula o banner mais próximo
        float nearestIndex = Mathf.Round(scrollRect.horizontalNormalizedPosition * (bannerContainer.childCount - 1));
        nearestIndex = Mathf.Clamp(nearestIndex, 0, bannerContainer.childCount - 1);
        currentIndex = (int)nearestIndex;

        // 🔹 Centraliza no banner mais próximo
        StartCoroutine(SmoothSlideTo(currentIndex));
        UpdateDots();

        // 🔹 Retoma auto-slide após pequeno delay
        StartCoroutine(ResumeAutoSlide());
    }

    private IEnumerator ResumeAutoSlide()
    {
        yield return new WaitForSeconds(3f);
        userDragging = false;
        autoSlideRoutine = StartCoroutine(AutoSlide());
    }
}
