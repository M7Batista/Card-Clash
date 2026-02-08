using UnityEngine;

[System.Serializable]
public class PuzzleData
{
    public int id;
    public Sprite previewImage;
    public PuzzleState state;
    [Range(0, 100)]
    public int progress;
}
