using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PuzzleBoardGenerator : MonoBehaviour
{
    [Header("Board Settings")]
    public int rows = 6;
    public int columns = 6;

    [Header("References")]
    public RectTransform boardRect;
    public PuzzleSlot slotPrefab;

    void Start()
    {
       GenerateBoard();
    }


    void GenerateBoard()
    {
        float boardWidth = boardRect.rect.width;
        float boardHeight = boardRect.rect.height;

        float cellSize = Mathf.Min(
            boardWidth / columns,
            boardHeight / rows
        );

        float startX = -boardWidth / 2 + cellSize / 2;
        float startY = boardHeight / 2 - cellSize / 2;

        int index = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                PuzzleSlot slot =
                    Instantiate(slotPrefab, boardRect);

                RectTransform slotRect =
                    slot.GetComponent<RectTransform>();

                slotRect.sizeDelta =
                    new Vector2(cellSize, cellSize);

                float posX = startX + c * cellSize;
                float posY = startY - r * cellSize;

                slotRect.anchoredPosition =
                    new Vector2(posX, posY);

                slot.row = r;
                slot.column = c;
                slot.index = index;

                index++;
            }
        }
    }
    public PuzzleSlot[] Slots => GetComponentsInChildren<PuzzleSlot>();

}
