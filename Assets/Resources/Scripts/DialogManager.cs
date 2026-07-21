using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text messageText;

    private Coroutine autoHideCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dialogPanel.SetActive(false);
    }

    /// <summary>
    /// Exibe um diálogo que permanece na tela até Hide() ser chamado.
    /// </summary>
    public void ShowPersistent(string message, KrakenExpression expression)
    {
        StopAutoHide();

        dialogPanel.SetActive(true);
        messageText.text = message;
        characterImage.sprite = KrakenDatabase.Instance.GetExpression(expression);
    }

    /// <summary>
    /// Exibe um diálogo temporário.
    /// </summary>
    public void ShowTemporary(string message,
                              KrakenExpression expression,
                              float duration = 3f)
    {
        StopAutoHide();
        dialogPanel.SetActive(true);
        messageText.text = message;
        characterImage.sprite = KrakenDatabase.Instance.GetExpression(expression);
        autoHideCoroutine = StartCoroutine(HideAfter(duration));
    }

    /// <summary>
    /// Fecha o diálogo.
    /// </summary>
    public void Hide()
    {
        StopAutoHide();
        dialogPanel.SetActive(false);
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        dialogPanel.SetActive(false);
        autoHideCoroutine = null;
    }

    private void StopAutoHide()
    {
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
    }
}