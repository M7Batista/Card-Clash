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
    public GameObject dotPrefab;

    [Header("Config")]
    public float slideInterval = 4f;
    public float slideSpeed = 8f;

    private int currentIndex = 0;
    private List<Image> dotImages = new List<Image>();
    private Coroutine autoSlideRoutine;
    private bool userDragging = false;

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
        foreach (Transform t in dotContainer)
            Destroy(t.gameObject);
        dotImages.Clear();

        int bannerCount = bannerContainer.childCount;
        for (int i = 0; i < bannerCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotContainer);
            Image img = dot.GetComponent<Image>();
            dotImages.Add(img);
        }
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
