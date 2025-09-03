using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BottomNavController : MonoBehaviour
{
    [Header("Screens")]
    public RectTransform homeScreen, galleryScreen, battleCardScreen, puzzleScreen;

    [Header("Buttons")]
    public Button homeButton, galleryButton, battleCardButton, puzzleButton;

    private RectTransform currentScreen;
    private RectTransform nextScreen;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.green; // Cor do botão ativo
    private float transitionDuration = 0.4f;

    // 🔹 Guardar todos botões e telas
    private Dictionary<Button, RectTransform> buttonToScreen;

    void Start()
    {
        // Inicializa o mapeamento
        buttonToScreen = new Dictionary<Button, RectTransform>()
        {
            { homeButton, homeScreen },
            { galleryButton, galleryScreen },
            { battleCardButton, battleCardScreen },
            { puzzleButton, puzzleScreen }
        };

        // Estado inicial → Home
        currentScreen = homeScreen;
        homeScreen.gameObject.SetActive(true);
        galleryScreen.gameObject.SetActive(false);
        battleCardScreen.gameObject.SetActive(false);
        puzzleScreen.gameObject.SetActive(false);

        // Botões
        homeButton.onClick.AddListener(() => SwitchScreen(homeScreen, homeButton));
        galleryButton.onClick.AddListener(() => SwitchScreen(galleryScreen, galleryButton));
        battleCardButton.onClick.AddListener(() => SwitchScreen(battleCardScreen, battleCardButton));
        puzzleButton.onClick.AddListener(() => SwitchScreen(puzzleScreen, puzzleButton));

        // Define estado inicial dos botões
        UpdateAllButtons(homeButton);
    }

    void SwitchScreen(RectTransform targetScreen, Button clickedButton)
    {
        if (targetScreen == currentScreen) return;

        nextScreen = targetScreen;
        nextScreen.gameObject.SetActive(true);

        // Direção do slide
        bool slideLeft = IsSlideLeft(currentScreen, nextScreen);

        StartCoroutine(SlideTransition(currentScreen, nextScreen, slideLeft));

        // Atualiza botões
        UpdateAllButtons(clickedButton);
    }

    IEnumerator SlideTransition(RectTransform fromScreen, RectTransform toScreen, bool slideLeft)
{
    float elapsed = 0f;

    // Usa localPosition em vez de anchoredPosition
    Vector3 startFrom = fromScreen.localPosition;
    Vector3 startTo = new Vector3(slideLeft ? Screen.width : -Screen.width, 0, 0);
    Vector3 endFrom = new Vector3(slideLeft ? -Screen.width : Screen.width, 0, 0);
    Vector3 endTo = Vector3.zero;

    toScreen.localPosition = startTo;

    while (elapsed < transitionDuration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / transitionDuration);

        // 🔹 Ease in-out cúbico (mais suave que linear)
        float easedT = t * t * (3f - 2f * t);

        fromScreen.localPosition = Vector3.Lerp(startFrom, endFrom, easedT);
        toScreen.localPosition = Vector3.Lerp(startTo, endTo, easedT);

        yield return null;
    }

    fromScreen.gameObject.SetActive(false);
    currentScreen = toScreen;

    // 🔹 Chama método especial "OnScreenOpened" se existir
    var screenLogic = currentScreen.GetComponent<MonoBehaviour>();
    if (screenLogic != null)
    {
        var method = screenLogic.GetType().GetMethod("OnScreenOpened");
        if (method != null)
        {
            method.Invoke(screenLogic, null);
        }
    }
}


    void UpdateAllButtons(Button selectedButton)
    {
        foreach (var entry in buttonToScreen.Keys)
        {
            UpdateButtonState(entry, entry == selectedButton);
        }
    }

    void UpdateButtonState(Button button, bool selected)
    {
        var colors = button.colors;

        if (selected)
        {
            colors.normalColor = selectedColor;
            colors.highlightedColor = selectedColor;
            colors.selectedColor = selectedColor;
        }
        else
        {
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.selectedColor = normalColor;
        }

        button.colors = colors;

        // 🔹 força atualização do estado visual
        var selectable = button as Selectable;
        selectable.OnDeselect(null);
    }

    // 🔹 Decide direção do slide
    bool IsSlideLeft(RectTransform from, RectTransform to)
    {
        List<RectTransform> order = new List<RectTransform>()
        {
            homeScreen, galleryScreen, battleCardScreen, puzzleScreen
        };

        int fromIndex = order.IndexOf(from);
        int toIndex = order.IndexOf(to);

        return toIndex > fromIndex; // se for para frente → slide left
    }
}
