using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite artwork;
    [Range(1, 9)] public int top;
    [Range(1, 9)] public int right;
    [Range(1, 9)] public int bottom;
    [Range(1, 9)] public int left;
}