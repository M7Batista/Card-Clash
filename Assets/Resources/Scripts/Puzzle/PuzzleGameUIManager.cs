using UnityEngine;

public class PuzzleGameUIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject puzzleSelectionScreen;
    public GameObject puzzleAssemblyScreen;

    public static PuzzleGameUIManager Instance;

    public GameObject boardArea; // Referência ao GameObject do tabuleiro
    public GameObject scrollArea; // Referência ao GameObject da área de scroll

    private int currentPuzzleId;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
}
