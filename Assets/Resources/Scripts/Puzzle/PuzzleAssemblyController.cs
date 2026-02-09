using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class PuzzleAssemblyController : MonoBehaviour
{
    [Header("Puzzle Image")]
    public Sprite puzzleImage;

    [Header("Grid Settings")]
    public int rows = 6;
    public int columns = 6;

    [Header("Pieces")]
    public PuzzlePiece piecePrefab;
    public Transform piecesParent;

    private List<Sprite> puzzlePieces = new List<Sprite>();
    public RectTransform boardArea;

    void OnEnable()
    {
        SetSpritePuzzle();
        GenerateSprites();
        GeneratePieces();
    }
    IEnumerator Start()
    {
        yield return null; // espera 1 frame
        AssignCorrectSlots();
    }
    void SetSpritePuzzle()
    {
        puzzleImage = PuzzleGameUIManager.Instance.GetCurrentPuzzleId() switch
        {
            0 => Resources.Load<Sprite>("Art/Puzzles/Marine"),
            1 => Resources.Load<Sprite>("Art/Puzzles/Milcow"),
            2 => Resources.Load<Sprite>("Art/Puzzles/Octanea"),
            3 => Resources.Load<Sprite>("Art/Puzzles/Pameli"),
            4 => Resources.Load<Sprite>("Art/Puzzles/Ravenn"),
            5 => Resources.Load<Sprite>("Art/Puzzles/Sara"),
            _ => null
        };
    }

    void GenerateSprites()
    {
        puzzlePieces.Clear();

        Texture2D texture = puzzleImage.texture;
        int pieceWidth = texture.width / columns;
        int pieceHeight = texture.height / rows;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Rect rect = new Rect(
                    c * pieceWidth,
                    (rows - 1 - r) * pieceHeight,
                    pieceWidth,
                    pieceHeight
                );

                Sprite piece = Sprite.Create(
                    texture,
                    rect,
                    new Vector2(0.5f, 0.5f),
                    puzzleImage.pixelsPerUnit
                );

                puzzlePieces.Add(piece);
            }
        }
    }

    void GeneratePieces()
    {
        // Limpa peças anteriores
        foreach (Transform child in piecesParent)
            Destroy(child.gameObject);

        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            int row = i / columns;
            int column = i % columns;

            PuzzlePiece piece =
                Instantiate(piecePrefab, piecesParent);

            piece.Setup(
                puzzlePieces[i],
                i,
                row,
                column
            );

        }
    }
    void AssignCorrectSlots()
    {
        PuzzleSlot[] slots =
            FindFirstObjectByType<PuzzleBoardGenerator>().Slots;

        foreach (Transform child in piecesParent)
        {
            PuzzlePiece piece = child.GetComponent<PuzzlePiece>();

            //piece.correctSlot = slots.First(s => s.index == piece.index);
            var slot = slots.FirstOrDefault(s => s.index == piece.index);

            if (slot == null)
            {
                Debug.LogError($"Slot não encontrado para peça {piece.index}");
                continue;
            }
        }
    }

}
