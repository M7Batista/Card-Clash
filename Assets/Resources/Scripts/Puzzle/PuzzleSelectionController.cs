using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PuzzleSelectionController : MonoBehaviour
{
    public static PuzzleSelectionController Instance;
    public Transform contentParent;
    public PuzzleCardUI puzzleCardPrefab;

    public List<PuzzleData> puzzles = new List<PuzzleData>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        GeneratePuzzleList();
    }

    void GeneratePuzzleList()
    {
        foreach (var puzzle in puzzles)
        {
            PuzzleCardUI card =
                Instantiate(puzzleCardPrefab, contentParent);
            puzzle.state = PuzzleState.Available; // TESTE - REMOVER DEPOIS
            card.Setup(puzzle);
        }
    }
}
