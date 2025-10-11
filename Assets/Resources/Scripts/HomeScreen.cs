using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HomeScreen : MonoBehaviour
{
    [Header("Imagem onde o personagem será exibido")]
    public Image characterImage;

    [Header("Configurações de transição")]
    [Tooltip("Tempo que o personagem leva para aparecer na tela (em segundos)")]
    public float fadeDuration = 0.8f;

    void OnEnable()
    {
        LoadCharacterImage();
        AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);

    }

    void LoadCharacterImage()
    {
        // Verifica se há um personagem salvo
        if (!PlayerPrefs.HasKey("HomeCharacterID"))
        {
            Debug.Log("Nenhum personagem inicial definido ainda.");
            return;
        }

        string characterID = PlayerPrefs.GetString("HomeCharacterID");
        if (string.IsNullOrEmpty(characterID))
        {
            Debug.LogWarning("O ID do personagem está vazio.");
            return;
        }

        // 🔹 Carrega o sprite do diretório Resources/Art/Artworks/
        Sprite loadedSprite = Resources.Load<Sprite>($"Art/Artworks/{characterID}");

        if (loadedSprite != null)
        {
            characterImage.sprite = loadedSprite;
            characterImage.enabled = true;
            StartCoroutine(FadeInImage());
            Debug.Log($"Personagem '{characterID}' carregado com sucesso!");
        }
        else
        {
            Debug.LogError($"Imagem não encontrada em Resources/Art/Artworks/{characterID}.png");
            characterImage.enabled = false;
        }
    }

    IEnumerator FadeInImage()
    {
        Color color = characterImage.color;
        color.a = 0f;
        characterImage.color = color;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            characterImage.color = color;
            yield return null;
        }

        // Garante alfa total no final
        color.a = 1f;
        characterImage.color = color;
    }
}