using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BattleResultScreen : MonoBehaviour
{
    [Header("Referências na UI")]
    public GameObject panel;
    public Image resultImage;
    public Sprite spriteVictory;
    public Sprite spriteDefeat;
    public Sprite spriteDraw;

    float fadeDuration = 1.5f;
    int result; // 0 = vitória, 1 = derrota, 2 = empate
    public static BattleResultScreen instance;
    private void Awake()
    {
        instance = this;
    }


    // Chame esse método quando o jogo terminar
    public void ShowEndGame(int result)
    {
        AudioManager.Instance.StopMusic();
        panel.SetActive(true);
        this.result = result;
        switch (result)
        {
            case 0: //vitória
                resultImage.sprite = spriteVictory;
                break;
            case 1: //derrota
                resultImage.sprite = spriteDefeat;
                break;
            case 2: //empate
                resultImage.sprite = spriteDraw;
                break;
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

        //yield return new WaitForSeconds(2f);
        //panel.SetActive(false);
        //BattleCardScreen.Instance.PosBattleSetup(result);
    }
    public void ClosePanel()
    {
        panel.SetActive(false);
        BattleCardScreen.Instance.PosBattleSetup(result);
    }

}


