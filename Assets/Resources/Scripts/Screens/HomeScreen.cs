using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class HomeScreen : MonoBehaviour
{
   
    [Header("Personagem (Image UI)")]
    public Image characterImage;              // personagem que deve ficar centralizado no background

    [Header("Transições")]
    public float fadeDuration = 0.8f;
    
    void OnEnable()
    {
        Debug.Log("HomeScreen OnEnable");
        LoadCharacterImage();
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.menuMusic);
    }

    void LoadCharacterImage()
    {
        if (characterImage == null) return;

        if (!PlayerPrefs.HasKey("HomeCharacterID")) return;

        string characterID = PlayerPrefs.GetString("HomeCharacterID");
        if (string.IsNullOrEmpty(characterID)) return;

        Sprite loadedSprite = Resources.Load<Sprite>($"Art/Artworks/{characterID}");
        if (loadedSprite != null)
        {
            characterImage.sprite = loadedSprite;
            characterImage.enabled = true;
            StartCoroutine(FadeInImage());
        }
        else
        {
            characterImage.enabled = false;
            Debug.LogWarning($"Personagem não encontrado: {characterID}");
        }
    }

    IEnumerator FadeInImage()
    {
        if (characterImage == null) yield break;
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

        color.a = 1f;
        characterImage.color = color;
    }
   
    
}