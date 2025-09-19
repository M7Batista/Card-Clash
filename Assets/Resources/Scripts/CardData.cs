using UnityEngine;
public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identificação")]
    public int id;         // ID único do card
    public CardRarity rarity; // Raridade do card

    [Header("Atributos")]
    public int top;
    public int right;
    public int bottom;
    public int left;

    [Header("Imagem")]
    public Sprite artwork;     // Imagem do card
    public string cardName;    // Nome exibido

}