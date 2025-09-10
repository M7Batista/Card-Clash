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
    public GameObject panelTextName;
    public Image frameImage;
    public Image highlightBorder;  // Borda para destaque
    private Image mainImage;

    [Header("Outros")]
    [HideInInspector] public CardData cardData;
    [HideInInspector] public Owner owner;
    private bool isEnabled = true;
    public bool isChecked = false;
    public GameObject checkmark;   // Ícone "✔"


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
    // Mostra ou esconde o "✔"
    public void ShowCheckmark(bool show)
    {
        if (checkmark != null) checkmark.SetActive(show);
        isChecked = show;
    }

    // Ativa borda de destaque
    public void SetHighlight(bool active)
    {
        if (highlightBorder != null) highlightBorder.enabled = active;
    }

    // Deixa o card "apagado" ou normal
    public void SetDimmed(bool dimmed)
    {
        if (mainImage == null) return;

        mainImage.color = dimmed ? new Color(1, 1, 1, 0.4f) : Color.white;
    }
}
