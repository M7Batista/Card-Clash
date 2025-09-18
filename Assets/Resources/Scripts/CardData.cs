using UnityEngine;
public enum Type { Common, Rare, Epic, Legendary }
[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identificação")]

    public int id;         // ID único do card
    public Type typeCard; // Raridade do card
    public string cardName;    // Nome exibido

    [Header("Arte")]
    public Sprite artwork;     // Imagem do card

    [Header("Atributos")]
    public int top;
    public int right;
    public int bottom;
    public int left;


}