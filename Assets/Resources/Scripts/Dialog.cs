using UnityEngine;
using TMPro;
using System.Collections;

public class Dialog : MonoBehaviour
{
    public static Dialog Instance { get; private set; }


    [Header("Referências")]
    public GameObject panel;
    public TextMeshProUGUI txtMessage;

    [Header("Configurações")]
    [Tooltip("Tempo que a mensagem fica visível antes de sumir")]
    public float displayDuration = 2.5f;

    [Tooltip("Velocidade de fade ao desaparecer")]
    public float fadeSpeed = 2f;

    private CanvasGroup canvasGroup;
    private Coroutine activeRoutine;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();

            panel.SetActive(false);
            canvasGroup.alpha = 0f;
        }
    }

    public void ShowMessage(string message)
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        txtMessage.text = message;
        panel.SetActive(true);

        // 🔹 Fade in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        // 🔹 Espera o tempo configurado
        yield return new WaitForSeconds(displayDuration);

        // 🔹 Fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        panel.SetActive(false);
        activeRoutine = null;
    }
}
