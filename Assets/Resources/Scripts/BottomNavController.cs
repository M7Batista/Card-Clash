using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BottomNavController : MonoBehaviour
{
    [Header("Screens")]
    public RectTransform homeScreen;
    public RectTransform collectionScreen;
    public RectTransform battleCardScreen;
    public RectTransform puzzleScreen;

    [Header("Buttons")]
    public Button homeButton;
    public Button collectionButton;
    public Button battleCardButton;
    public Button puzzleButton;
    private RectTransform currentScreen;
    private RectTransform nextScreen;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow; // Cor do botão ativo
    private float transitionDuration = 0.4f;

    // 🔹 Guardar todos botões e telas
    private Dictionary<Button, RectTransform> buttonToScreen;

    void Start()
    {
        // Inicializa o mapeamento
        buttonToScreen = new Dictionary<Button, RectTransform>()
        {
            { homeButton, homeScreen },
            { collectionButton, collectionScreen },
            { battleCardButton, battleCardScreen },
            { puzzleButton, puzzleScreen }
        };

        // Estado inicial → Home
        currentScreen = homeScreen;
        homeScreen.gameObject.SetActive(true);
        collectionScreen.gameObject.SetActive(false);
        battleCardScreen.gameObject.SetActive(false);
        puzzleScreen.gameObject.SetActive(false);
        // Botões
        homeButton.onClick.AddListener(() => SwitchScreen(homeScreen, homeButton));
        collectionButton.onClick.AddListener(() => SwitchScreen(collectionScreen, collectionButton));
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
        StartCoroutine(FadeTransition(currentScreen, nextScreen));
        // Atualiza botões
        UpdateAllButtons(clickedButton);
    }
    IEnumerator FadeTransition(RectTransform fromScreen, RectTransform toScreen)
    {
        CanvasGroup fromGroup = fromScreen.GetComponent<CanvasGroup>();
        CanvasGroup toGroup = toScreen.GetComponent<CanvasGroup>();

        if (fromGroup == null) fromGroup = fromScreen.gameObject.AddComponent<CanvasGroup>();
        if (toGroup == null) toGroup = toScreen.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0f;

        // Inicializa estados
        toGroup.alpha = 0f;
        toGroup.interactable = false;
        toGroup.blocksRaycasts = false;
        toScreen.gameObject.SetActive(true);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            fromGroup.alpha = 1f - t;
            toGroup.alpha = t;

            yield return null;
        }

        // Finaliza estados
        fromGroup.alpha = 0f;
        fromGroup.interactable = false;
        fromGroup.blocksRaycasts = false;
        fromScreen.gameObject.SetActive(false);

        toGroup.alpha = 1f;
        toGroup.interactable = true;
        toGroup.blocksRaycasts = true;

        currentScreen = toScreen;
        
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

}
