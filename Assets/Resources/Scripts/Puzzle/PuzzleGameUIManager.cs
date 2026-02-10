using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PuzzleGameUIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject puzzleSelectionScreen;
    public GameObject puzzleAssemblyScreen;

    public static PuzzleGameUIManager Instance;

    public GameObject boardArea; // Referência ao GameObject do tabuleiro
    public GameObject scrollArea; // Referência ao GameObject da área de scroll

    public Button backButton; // Referência ao botão de voltar
    public Button resetButton; // Referência ao botão de resetar
    public Button previewButton; // Referência ao botão de pré-visualizar
    public Image previewImage; // Referência à imagem de pré-visualização

    private int currentPuzzleId;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        backButton.onClick.AddListener(BackToSelection);
        resetButton.onClick.AddListener(ResetPuzzle);
        previewImage.gameObject.SetActive(false);
        SetupPreviewButtonPressEvents();
    }

    public void OpenPuzzleAssembly(int puzzleId)
    {
        currentPuzzleId = puzzleId;

        puzzleSelectionScreen.SetActive(false);
        puzzleAssemblyScreen.SetActive(true);

        Debug.Log("Abrindo Puzzle ID: " + puzzleId);
    }

    public void BackToSelection()
    {
        CleanUpAssemblyScreen();
        puzzleAssemblyScreen.SetActive(false);
        puzzleSelectionScreen.SetActive(true);
    }
    void CleanUpAssemblyScreen()
    {
        // Limpa os filhos do boardArea
        foreach (Transform child in boardArea.transform)
        {
            Destroy(child.gameObject);
        }

        // Limpa os filhos do scrollArea
        foreach (Transform child in scrollArea.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public int GetCurrentPuzzleId()
    {
        return currentPuzzleId;
    }
    public void ShowPreview()
    {
        previewImage.sprite = PuzzleSelectionController.Instance.puzzles[currentPuzzleId].previewImage;
        previewImage.gameObject.SetActive(true);
        // Move o preview para a frente
        previewImage.transform.SetAsLastSibling();
    }

    public void HidePreview()
    {
        previewImage.gameObject.SetActive(false);
    }

    void SetupPreviewButtonPressEvents()
    {
        EventTrigger trigger = previewButton.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = previewButton.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry downEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        downEntry.callback.AddListener(_ => ShowPreview());

        EventTrigger.Entry upEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        upEntry.callback.AddListener(_ => HidePreview());

        trigger.triggers.Add(downEntry);
        trigger.triggers.Add(upEntry);
    }
    void ResetPuzzle()
    {
        //Procura peças nos slots e as move de volta para a área de scroll
        foreach (Transform pieceInSlot in boardArea.transform)
        {
            // Move a peça de volta para a área de scroll
            if (pieceInSlot.childCount > 0)
            {
                pieceInSlot.transform.GetChild(0).GetComponent<PuzzlePieceDrag>().ReturnToStart(); // Reseta a peça para o estado inicial
            }
        }
    }
}
