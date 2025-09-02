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

    // 🔹 Guardar todos botões e telas para facilitar
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

        // Direção do slide → se a nova tela está "à direita" no fluxo
        bool slideLeft = IsSlideLeft(currentScreen, nextScreen);

        StartCoroutine(SlideTransition(currentScreen, nextScreen, slideLeft));

        // Atualiza botões
        UpdateAllButtons(clickedButton);
    }

    IEnumerator SlideTransition(RectTransform fromScreen, RectTransform toScreen, bool slideLeft)
    {
        float elapsed = 0f;
        Vector2 startFrom = fromScreen.anchoredPosition;
        Vector2 startTo = new Vector2(slideLeft ? Screen.width : -Screen.width, 0);
        Vector2 endFrom = new Vector2(slideLeft ? -Screen.width : Screen.width, 0);
        Vector2 endTo = Vector2.zero;

        toScreen.anchoredPosition = startTo;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            fromScreen.anchoredPosition = Vector2.Lerp(startFrom, endFrom, t);
            toScreen.anchoredPosition = Vector2.Lerp(startTo, endTo, t);

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

        // 🔹 força o botão a redesenhar o estado visual
        var selectable = button as Selectable;
        selectable.OnDeselect(null);
    }


    // 🔹 Decide direção do slide baseado na ordem das telas
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
