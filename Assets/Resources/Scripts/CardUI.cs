using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum Owner { None, Player, Enemy }

public class CardUI : MonoBehaviour
{
    [Header("Referências Visuais")]
    public Image backgroundImage;
    public Image artworkImage;
    public Image frameImage;
    public TextMeshProUGUI numTop;
    public TextMeshProUGUI numRight;
    public TextMeshProUGUI numBottom;
    public TextMeshProUGUI numLeft;
    public TextMeshProUGUI txtName;


    [Header("Outros")]
    [HideInInspector] public CardData cardData;
    [HideInInspector] public Owner owner;
    public bool isChecked = false;
    public GameObject checkmark;
    public GameObject front;
    public GameObject back;


    public void SetCard(CardData data, Owner newOwner)
    {
        cardData = data;
        artworkImage.sprite = data.artwork;
        txtName.text = data.cardName;
        numTop.text = ConvertToString(data.top);
        numRight.text = ConvertToString(data.right);
        numBottom.text = ConvertToString(data.bottom);
        numLeft.text = ConvertToString(data.left);
        switch (data.rarity)
        {
            case CardRarity.Common:
                backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_common");
                break;
            case CardRarity.Uncommon:
                backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_uncommon");
                break;
            case CardRarity.Rare:
                backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_rare");
                break;
            case CardRarity.Epic:
                backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_epic");
                break;
            case CardRarity.Legendary:
                backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_legendary");
                break;
            default:
                backgroundImage.sprite = Resources.Load<Sprite>("Art/CardBase/background_common");
                break;
        }

        SetOwner(newOwner);
    }

    public void SetOwner(Owner newOwner)
    {
        owner = newOwner;
        if (newOwner == Owner.Player)
            frameImage.color = new Color32(52, 125, 255, 255); // azul
        else
        if (newOwner == Owner.Enemy)
            frameImage.color = new Color32(255, 71, 71, 255); // vermelho
        else
            frameImage.color = Color.white; // neutro

    }
    string ConvertToString(int value)
    {
        string strValue = "";
        if (value == 10)
        {
            strValue = "A";
        }
        else
        if (value == 11)
        {
            strValue = "B";
        }
        else
            strValue = value.ToString();
        return strValue;
    }


    // Mostra ou esconde o "✔"
    public void ShowCheckmark(bool show)
    {
        if (checkmark != null) checkmark.SetActive(show);
        isChecked = show;
    }


    private bool isFaceUp = true;

    public void ShowFront()
    {
        front.SetActive(true);
        back.SetActive(false);
        isFaceUp = true;
    }

    public void ShowBack()
    {
        front.SetActive(false);
        back.SetActive(true);
        isFaceUp = false;
    }

    public void Flip()
    {
        if (isFaceUp) ShowBack();
        else ShowFront();
    }
}
