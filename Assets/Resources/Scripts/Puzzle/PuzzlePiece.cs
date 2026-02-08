using UnityEngine;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour
{
    public int index;
    public int row;
    public int column;
    public Image image;
    public bool isFixed = false;
    public PuzzleSlot correctSlot;


    public void Setup(Sprite sprite, int index, int row, int column)
    {
        this.index = index;
        this.row = row;
        this.column = column;

        image.sprite = sprite;
    }
}
