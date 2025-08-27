using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Image artworkImage;

    public Text numTop;
    public Text numRight;
    public Text numBottom;
    public Text numLeft;

    private CardData data;

    public void SetCard(CardData card)
    {
        data = card;

        artworkImage.sprite = data.artwork;

        numTop.text    = data.top.ToString();
        numRight.text  = data.right.ToString();
        numBottom.text = data.bottom.ToString();
        numLeft.text   = data.left.ToString();
    }

    public CardData GetData() => data;
}
