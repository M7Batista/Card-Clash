using System.Collections.Generic;
using UnityEngine;

//public enum CardRarity { Comum, Incomum, Raro, Epico, Lendario, UnicoBoss }


[System.Serializable]
public struct CardProbability
{
    public int stage;
    public float comum;
    public float incomum;
    public float raro;
    public float epico;
    public float lendario;

    public CardProbability(int stage, float c, float i, float r, float e, float l)
    {
        this.stage = stage;
        comum = c;
        incomum = i;
        raro = r;
        epico = e;
        lendario = l;
    }
}

public class EnemyDeckManager : MonoBehaviour
{
    [Header("Banco de Cartas")]
    public List<CardData> allCards = new List<CardData>();

    private List<CardProbability> stageProbabilities = new List<CardProbability>();
    public static EnemyDeckManager Instance;

    private void Awake()
    {
        Instance = this;
        // carregar tabela de probabilidades
        LoadProbabilities();
    }

    /// <summary>
    /// Gera o deck inimigo para um estágio específico
    /// </summary>
    public List<CardData> GenerateEnemyDeck(int stageNumber)
    {
         // 🔹 Carregar todas as cartas do jogo
        CardData[] allCardsData = Resources.LoadAll<CardData>("cards");
        foreach (CardData card in allCardsData)
        {
            allCards.Add(card);
        }

        List<CardData> enemyDeck = new List<CardData>();

        bool isBoss = stageNumber % 10 == 0;
        int deckSize = isBoss ? 6 : 5;

        CardProbability prob = stageProbabilities.Find(p => p.stage == stageNumber);

        for (int i = 0; i < deckSize; i++)
        {
            CardRarity rarity = RollRarity(prob, isBoss);

            // Filtrar cartas dessa raridade
            List<CardData> filtered = allCards.FindAll(c => c.rarity == rarity);

            if (filtered.Count > 0)
            {
                enemyDeck.Add(filtered[Random.Range(0, filtered.Count)]);
            }
            else
            {
                // fallback: qualquer carta
                enemyDeck.Add(allCards[Random.Range(0, allCards.Count)]);
            }
        }

        return enemyDeck;
    }

    /// <summary>
    /// Sorteia a raridade com base nas probabilidades da tabela
    /// </summary>
    private CardRarity RollRarity(CardProbability prob, bool isBoss)
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        cumulative += prob.comum;
        if (roll < cumulative) return CardRarity.Common;

        cumulative += prob.incomum;
        if (roll < cumulative) return CardRarity.Uncommon;

        cumulative += prob.raro;
        if (roll < cumulative) return CardRarity.Rare;

        cumulative += prob.epico;
        if (roll < cumulative) return CardRarity.Epic;

        return CardRarity.Legendary;
    }

    /// <summary>
    /// Preenche a lista de probabilidades (dados embutidos)
    /// </summary>
    private void LoadProbabilities()
    {
        int[,] table = new int[,] {
            // Estagio, C, I, R, E, L
            // Estagio, comum, incomum, raro, epico, lendario
            {0,100,0,0,0,0},
            { 1,70,30,0,0,0},{2,70,30,0,0,0},{3,70,30,0,0,0},{4,70,30,0,0,0},{5,70,30,0,0,0},
            {6,50,35,15,0,0},{7,50,35,15,0,0},{8,50,35,15,0,0},{9,50,35,15,0,0},
            {10,20,30,30,15,5},
            {11,40,35,25,0,0},{12,40,35,25,0,0},{13,40,35,25,0,0},{14,40,35,25,0,0},{15,40,35,25,0,0},
            {16,25,40,30,5,0},{17,25,40,30,5,0},{18,25,40,30,5,0},{19,25,40,30,5,0},
            {20,10,25,35,20,10},
            {21,20,30,35,15,0},{22,20,30,35,15,0},{23,20,30,35,15,0},{24,20,30,35,15,0},{25,20,30,35,15,0},
            {26,10,25,35,25,5},{27,10,25,35,25,5},{28,10,25,35,25,5},{29,10,25,35,25,5},
            {30,5,15,30,30,20},
            {31,8,22,40,25,5},{32,8,22,40,25,5},{33,8,22,40,25,5},{34,8,22,40,25,5},{35,8,22,40,25,5},
            {36,5,20,40,30,5},{37,5,20,40,30,5},{38,5,20,40,30,5},{39,5,20,40,30,5},
            {40,2,10,35,33,20},
            {41,5,18,40,30,7},{42,5,18,40,30,7},{43,5,18,40,30,7},{44,5,18,40,30,7},{45,5,18,40,30,7},
            {46,3,15,40,32,10},{47,3,15,40,32,10},{48,3,15,40,32,10},{49,3,15,40,32,10},
            {50,1,10,30,34,25},
            {51,3,12,35,35,15},{52,3,12,35,35,15},{53,3,12,35,35,15},{54,3,12,35,35,15},{55,3,12,35,35,15},
            {56,2,10,32,36,20},{57,2,10,32,36,20},{58,2,10,32,36,20},{59,2,10,32,36,20},
            {60,1,8,25,36,30},
            {61,2,8,30,40,20},{62,2,8,30,40,20},{63,2,8,30,40,20},{64,2,8,30,40,20},{65,2,8,30,40,20},
            {66,1,6,28,40,25},{67,1,6,28,40,25},{68,1,6,28,40,25},{69,1,6,28,40,25},
            {70,1,5,20,39,35},
            {71,1,5,25,40,29},{72,1,5,25,40,29},{73,1,5,25,40,29},{74,1,5,25,40,29},{75,1,5,25,40,29},
            {76,1,4,20,40,35},{77,1,4,20,40,35},{78,1,4,20,40,35},{79,1,4,20,40,35},
            {80,1,3,15,41,40},
            {81,1,3,18,42,36},{82,1,3,18,42,36},{83,1,3,18,42,36},{84,1,3,18,42,36},{85,1,3,18,42,36},
            {86,1,2,15,42,40},{87,1,2,15,42,40},{88,1,2,15,42,40},{89,1,2,15,42,40},
            {90,1,2,8,39,50},
            {91,1,2,12,40,45},{92,1,2,12,40,45},{93,1,2,12,40,45},{94,1,2,12,40,45},{95,1,2,12,40,45},
            {96,1,1,10,39,49},{97,1,1,10,39,49},{98,1,1,10,39,49},{99,1,1,10,39,49},
            {100,0,1,5,44,50}
        };

        stageProbabilities.Clear();

        for (int i = 0; i < table.GetLength(0); i++)
        {
            stageProbabilities.Add(
                new CardProbability(
                    table[i,0],
                    table[i,1],
                    table[i,2],
                    table[i,3],
                    table[i,4],
                    table[i,5]
                )
            );
        }
    }
}
