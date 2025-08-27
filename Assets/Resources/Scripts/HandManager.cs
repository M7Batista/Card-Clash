using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab; // arraste o prefab da carta
    public Sprite[] cardSprites;  // sprites diferentes (placeholder)

    void Start()
    {
        // Instancia 5 cartas
        for (int i = 0; i < 5; i++)
        {
            var cardObj = Instantiate(cardPrefab, transform);
            var img = cardObj.GetComponent<Image>();

            // Se tiver sprites diferentes, escolhe um
            if (cardSprites != null && cardSprites.Length > 0)
            {
                img.sprite = cardSprites[i % cardSprites.Length];
            }
        }
    }
}
