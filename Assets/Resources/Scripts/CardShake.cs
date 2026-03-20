using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardShake : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    float duration = 0.1f;
    float strength = 20f;

    private RectTransform rectTransform;
    private Vector3 originalPos;

    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
        {
            StartCoroutine(Shake());
        }
    }

    IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            rectTransform.anchoredPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition = originalPos;
    }
}