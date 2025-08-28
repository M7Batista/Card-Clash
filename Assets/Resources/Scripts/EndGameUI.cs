using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EndGameUI : MonoBehaviour
{
    public GameObject panel;
    public Button restartButton;
    public Image resultImage;
    public float fadeDuration = 1.5f;
    public Sprite spriteVictory;
    public Sprite spriteDefeat;

    void Start()
    {
        restartButton.onClick.AddListener(ReloadGame);
    }

    // Chame esse método quando o jogo terminar
    public void ShowEndGame(bool victory)
    {
        panel.SetActive(true);

        if (victory)
        {
            resultImage.sprite = spriteVictory;
        }
        else
        {
            resultImage.sprite = spriteDefeat;
        }
        StartCoroutine(FadeIn());

    }
    private System.Collections.IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color corAtual = resultImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            corAtual.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            resultImage.color = corAtual;
            yield return null;
            
        }

        //resultImage.GetComponent<Image>().alpha = 1f; // garante que fica 100% visível
    }

    public void ReloadGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}


