using UnityEngine;

public class PuzzleGameUIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject puzzleSelectionScreen;
    public GameObject puzzleAssemblyScreen;

    public static PuzzleGameUIManager Instance;

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
        puzzleAssemblyScreen.SetActive(false);
        puzzleSelectionScreen.SetActive(true);
    }

    public int GetCurrentPuzzleId()
    {
        return currentPuzzleId;
    }
}
