using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EndGameUI : MonoBehaviour
{
    public GameObject panel;
    public Image resultImage;
    public float fadeDuration = 1.5f;
    public Sprite spriteVictory, spriteDefeat, spriteDraw;
    public static EndGameUI instance;
    private void Awake()
    {
        instance = this;
    }


    // Chame esse método quando o jogo terminar
    public void ShowEndGame(int result)
    {
        panel.SetActive(true);
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

        //resultImage.GetComponent<Image>().alpha = 1f; // garante que fica 100% visível
        yield return new WaitForSeconds(2f);
        panel.SetActive(false);
        BattleCardScreen.Instance.ExitBattle();
    }
    

}


