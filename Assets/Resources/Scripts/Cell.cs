using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public int index; // posição no tabuleiro
    public Image cardImage; // referência para mostrar a carta

    private void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponent<Image>();
    }

    public bool IsEmpty => cardImage.sprite == null;

    public void PlaceCard(Sprite cardSprite)
    {
        cardImage.sprite = cardSprite;
        cardImage.color = Color.white; // garantir que fique visível
    }
}
