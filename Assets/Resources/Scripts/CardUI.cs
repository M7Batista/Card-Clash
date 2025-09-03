using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum Owner { None, Player, Enemy }

public class CardUI : MonoBehaviour
{
    public Image artworkImage;
    public TextMeshProUGUI numTop;
    public TextMeshProUGUI numRight;
    public TextMeshProUGUI numBottom;
    public TextMeshProUGUI numLeft;
    public TextMeshProUGUI txtName;
    public GameObject panelTextName;
    public Image frameImage;
    public CardData cardData;
    public Owner owner;


    public void SetCard(CardData data, Owner newOwner)
    {
        cardData = data;
        artworkImage.sprite = data.artwork;
        txtName.text = data.cardName;
        numTop.text = data.top.ToString();
        numRight.text = data.right.ToString();
        numBottom.text = data.bottom.ToString();
        numLeft.text = data.left.ToString();

        SetOwner(newOwner);
    }

    public void SetOwner(Owner newOwner)
    {
        owner = newOwner;
    }
    public void ShowName(bool show)
    {
        panelTextName.SetActive(show);
    }

}