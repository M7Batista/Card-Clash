using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Image artworkImage;
    public string cardName;
    public Text numTop;
    public Text numRight;
    public Text numBottom;
    public Text numLeft;

    public CardData cardData;

    public void SetCard(CardData card)
    {
        cardData = card;

        artworkImage.sprite = cardData.artwork;

        numTop.text    = cardData.top.ToString();
        numRight.text  = cardData.right.ToString();
        numBottom.text = cardData.bottom.ToString();
        numLeft.text   = cardData.left.ToString();
    }

    public CardData GetCardData() => cardData;

}
