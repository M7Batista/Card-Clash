using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum Owner { None, Player, Enemy }

public class CardUI : MonoBehaviour
{
    public Image artworkImage;
    public string cardName;
    public Text numTop;
    public Text numRight;
    public Text numBottom;
    public Text numLeft;

    public Image frameImage;

    public CardData cardData;
    public Owner owner;

    public Color playerColor = Color.blue;
    public Color enemyColor = Color.red;




    public void SetCard(CardData data, Owner newOwner)
    {
        cardData = data;
        artworkImage.sprite = data.artwork;
        numTop.text = data.top.ToString();
        numRight.text = data.right.ToString();
        numBottom.text = data.bottom.ToString();
        numLeft.text = data.left.ToString();
        

        SetOwner(newOwner);
    }

    public void SetOwner(Owner newOwner)
    {
        owner = newOwner;
        frameImage = transform.GetChild(1).GetComponent<Image>();
        if (frameImage != null)
        {

            frameImage.color = (newOwner == Owner.Player) ? playerColor :
                              (newOwner == Owner.Enemy) ? enemyColor :
                             Color.white;
        }
    }

}
