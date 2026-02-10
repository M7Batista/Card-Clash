using UnityEngine;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour
{
    public int index;
    public int row;
    public int column;
    public bool isFixed = false;
    public PuzzleSlot correctSlot;

    [Header("UI")]
    public Image pieceImage;

    public void Setup(Sprite pieceSprite, int index, int row, int column)
    {
        this.index = index;
        this.row = row;
        this.column = column;
        pieceImage.sprite = pieceSprite;
    }
}
