using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum Owner { None, Player, Enemy }

public class CardUI : MonoBehaviour
{
    [Header("Referências Visuais")]
    public Image artworkImage;
    public TextMeshProUGUI numTop;
    public TextMeshProUGUI numRight;
    public TextMeshProUGUI numBottom;
    public TextMeshProUGUI numLeft;
    public TextMeshProUGUI txtName;


    [Header("Outros")]
    [HideInInspector] public CardData cardData;
    [HideInInspector] public Owner owner;
    public bool isChecked = false;
    public GameObject checkmark;   // Ícone "✔"
    public GameObject front; // arraste no Inspector
    public GameObject back;  // arraste no Inspector


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
