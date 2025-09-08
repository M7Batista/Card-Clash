using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum Owner { None, Player, Enemy }

public class CardUI : MonoBehaviour
{
    [Header("Card Elements")]
    public Image artworkImage;
    public TextMeshProUGUI numTop;
    public TextMeshProUGUI numRight;
    public TextMeshProUGUI numBottom;
    public TextMeshProUGUI numLeft;
    public TextMeshProUGUI txtName;
    public GameObject panelTextName;
    public Image frameImage;

    [HideInInspector] public CardData cardData;
    [HideInInspector] public Owner owner;

    private bool isEnabled = true;

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

    public void SetEnabledState(bool enabled)
    {
        isEnabled = enabled;

        if (enabled)
        {
            // Normal
            artworkImage.color = Color.white; 
            txtName.text = cardData.cardName;
            numTop.text = cardData.top.ToString();
            numRight.text = cardData.right.ToString();
            numBottom.text = cardData.bottom.ToString();
            numLeft.text = cardData.left.ToString();
        }
        else
        {
            // Desabilitado (escuro e com interrogação)
            artworkImage.color = Color.black;
            txtName.text = "?";
            numTop.text = "?";
            numRight.text = "?";
            numBottom.text = "?";
            numLeft.text = "?";
        }
    }
}
