using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    public static CardView Instance;
    public Image previewImage;
    public GameObject panelTop, panelBottom;
    public TextMeshProUGUI numTop, numRight, numBottom, numLeft;
    public TextMeshProUGUI characterName, characterRarity, txtID;
    public RadarPolygon radarPolygon;
    public Button buttonView;
    string currentCardName;
    public Image backgroundImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ShowCard(CardData cardData)
    {
        previewImage.sprite = cardData.artwork;
        panelTop.SetActive(true);
        panelBottom.SetActive(true);

        if (numTop) numTop.text = ConvertToString(cardData.top);
        if (numRight) numRight.text = ConvertToString(cardData.right);
        if (numBottom) numBottom.text = ConvertToString(cardData.bottom);
        if (numLeft) numLeft.text = ConvertToString(cardData.left);
        if (characterName) characterName.text = cardData.cardName;
        if (characterRarity)
        {
            characterRarity.text = cardData.rarity.ToString();

            switch (cardData.rarity)
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
        }

        if (radarPolygon != null)
        {
            radarPolygon.top = cardData.top;
            radarPolygon.right = cardData.right;
            radarPolygon.bottom = cardData.bottom;
            radarPolygon.left = cardData.left;
            radarPolygon.SetVerticesDirty();
        }

        currentCardName = cardData.cardName;

        txtID.text = $"{cardData.id}";

        var zoom = gameObject.transform.GetChild(1).GetComponent<CardZoom>();
        if (zoom != null) zoom.ResetZoom();
        buttonView.onClick.AddListener(FullScreen);
    }


    string ConvertToString(int value)
    {
        if (value == 10) return "A";
        if (value == 11) return "B";
        return value.ToString();
    }


    void FullScreen()
    {
        panelTop.SetActive(!panelTop.activeSelf);
        panelBottom.SetActive(!panelBottom.activeSelf);
    }
}
