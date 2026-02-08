using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PuzzleSelectionController : MonoBehaviour
{
    public Transform contentParent;
    public PuzzleCardUI puzzleCardPrefab;

    public List<PuzzleData> puzzles = new List<PuzzleData>();
    public Button backButton;

    void Start()
    {
        GeneratePuzzleList();
        backButton.onClick.AddListener(RetornToMainMenu);
    }

    void GeneratePuzzleList()
    {
        foreach (var puzzle in puzzles)
        {
            PuzzleCardUI card =
                Instantiate(puzzleCardPrefab, contentParent);

            card.Setup(puzzle);
        }
    }
    void RetornToMainMenu()
    {
        // volta pra cena do jogo principal
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
