using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
    public static CardView Instance;
    //public Image previewImage;
    public GameObject panelTop, panelBottom;
    public TextMeshProUGUI numTop, numRight, numBottom, numLeft;
    public TextMeshProUGUI characterName, characterRarity, txtID;
    public RadarPolygon radarPolygon;
    public Button buttonView;
    public Button buttonSetHomeScreen; //Atribui o personagem a tela inicial do jogo
    public Toggle toggleAnimation; // alterna entre sprite e animação
    string currentCardName;
    public Image backgroundImage;
    public CharacterDisplayController characterDisplayController;

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
        
        // garantir que qualquer vídeo anterior seja parado
        if (characterDisplayController != null)
        {
            characterDisplayController.StopVideo();
        }
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

       
        // remove listeners to avoid duplications
        if (buttonView != null)
        {
            buttonView.onClick.RemoveAllListeners();
            buttonView.onClick.AddListener(FullScreen);
        }
        if (buttonSetHomeScreen != null)
        {
            buttonSetHomeScreen.onClick.RemoveAllListeners();
            buttonSetHomeScreen.onClick.AddListener(() => SetHomeScreen(cardData));
        }

        // Setup animation toggle: visible only if card has animation
        bool hasAnim = cardData.hasAnimation;
        SetHasAnimation(hasAnim);

        // Always show image initially. If there's no controller, load image via preview sprite already set.
        if (characterDisplayController != null)
        {
            // Ensure image fallback is loaded into the controller as well (keeps sizing consistent)
            characterDisplayController.StopVideo();
            characterDisplayController.LoadImage(cardData.cardName);
        }
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
    // Permite que outro componente (por exemplo CardUI) informe se existe animação
    public void SetHasAnimation(bool hasAnim)
    {
        if (toggleAnimation != null)
        {
            // Mostrar o toggle apenas quando houver animação
            toggleAnimation.gameObject.SetActive(hasAnim);
            toggleAnimation.onValueChanged.RemoveAllListeners();
            toggleAnimation.isOn = false;
            toggleAnimation.onValueChanged.AddListener(OnToggleAnimation);
        }

        
        if (characterDisplayController != null)
        {
            characterDisplayController.StopVideo();
        }
    }

    void OnToggleAnimation(bool on)
    {
        if (on)
        {
            // show animated video via controller
            if (characterDisplayController != null)
            {
                characterDisplayController.LoadCharacter(currentCardName);
            }
        }
        else
        {
            // stop video and show image
            if (characterDisplayController != null)
            {
                characterDisplayController.StopVideo();
                characterDisplayController.LoadImage(currentCardName);
            }

        }
    }
    void SetHomeScreen(CardData cardData)
    {
        PlayerPrefs.SetString("HomeScreenCharacter", cardData.cardName);
        Debug.Log($"Home screen character set to: {cardData.cardName}");
    }
}
