using UnityEngine;
using UnityEngine.UI;
public class PuzzleSlot : MonoBehaviour
{
    public int row;
    public int column;
    public int index;

    public bool isOccupied = false;
    [Header("Highlight")]
    [SerializeField] private Image highlightImage;

    public void SetHighlight(bool value)
    {
        if (highlightImage != null)
            highlightImage.enabled = value;
    }
}
