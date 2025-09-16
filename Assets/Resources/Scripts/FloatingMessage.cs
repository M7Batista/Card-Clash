using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FloatingMessage : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup;
    public float floatDistance = 50f;
    public float duration = 2f;

    public void Show(string message)
    {
        messageText.text = message;
        StartCoroutine(AnimateMessage());
    }

    private IEnumerator AnimateMessage()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * floatDistance;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Sobe aos poucos
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            // Fade out suave
            canvasGroup.alpha = 1 - t;

            yield return null;
        }

        Destroy(gameObject);
    }
}
