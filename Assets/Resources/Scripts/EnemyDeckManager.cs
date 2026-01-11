using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;


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
        Debug.Log("Probabilidades usadas para estágio " + stageNumber + ": " +
                  $"Comum: {prob.comum}%, Incomum: {prob.incomum}%, Raro: {prob.raro}%, Épico: {prob.epico}%, Lendário: {prob.lendario}%");

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
    /// Carrega as probabilidades de um arquivo CSV ou usa valores padrão
    /// </summary>
    private void LoadProbabilities()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Files/Probabilities");
        if (csvFile != null)
        {
            stageProbabilities.Clear();
            string[] lines = csvFile.text.Split('\n');
            bool isFirstLine = true;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (isFirstLine)
                {
                    isFirstLine = false; // skip header
                    continue;
                }
                string[] values = line.Split(',');
                if (values.Length >= 6)
                {
                    int stage = int.Parse(values[0]);
                    float comum = float.Parse(values[1], CultureInfo.InvariantCulture);
                    float incomum = float.Parse(values[2], CultureInfo.InvariantCulture);
                    float raro = float.Parse(values[3], CultureInfo.InvariantCulture);
                    float epico = float.Parse(values[4], CultureInfo.InvariantCulture);
                    float lendario = float.Parse(values[5], CultureInfo.InvariantCulture);
                    stageProbabilities.Add(new CardProbability(stage, comum, incomum, raro, epico, lendario));
                }
            }
            Debug.Log("Probabilidades carregadas do arquivo CSV.");
        }
        else
        {
            // fallback: carregar valores embutidos
            LoadHardcodedProbabilities();
            Debug.Log("Arquivo CSV não encontrado. Usando probabilidades embutidas.");
        }
    }

    /// <summary>
    /// Preenche a lista de probabilidades (dados embutidos)
    /// </summary>
    private void LoadHardcodedProbabilities()
    {
        stageProbabilities.Clear();

        for (int stage = 1; stage <= 100; stage++)
        {
            bool isBoss = stage % 10 == 0;
            float comum = 0, incomum = 0, raro = 0, epico = 0, lendario = 0;

            if (stage <= 5)
            {
                comum = 100;
            }
            else if (stage <= 9)
            {
                comum = 70;
                incomum = 30;
            }
            else
            {
                int group = (stage - 1) / 10 + 1; // 2 for 10-19, 3 for 20-29, etc.
                int maxRarity = group;
                if (isBoss) maxRarity++;
                if (maxRarity > 5) maxRarity = 5;

                // Define probabilities based on maxRarity
                if (maxRarity == 3) // 10-19, non-boss: common, uncommon, rare
                {
                    if (isBoss)
                    {
                        comum = 20; incomum = 30; raro = 30; epico = 15; lendario = 5;
                    }
                    else
                    {
                        comum = 40; incomum = 35; raro = 25;
                    }
                }
                else if (maxRarity == 4) // 20-29, non-boss: up to epic
                {
                    if (isBoss)
                    {
                        comum = 10; incomum = 25; raro = 35; epico = 20; lendario = 10;
                    }
                    else
                    {
                        comum = 20; incomum = 30; raro = 35; epico = 15;
                    }
                }
                else if (maxRarity == 5) // 30+, up to legendary
                {
                    if (isBoss)
                    {
                        // For bosses, adjust based on stage
                        if (stage == 30)
                        {
                            comum = 5; incomum = 15; raro = 30; epico = 30; lendario = 20;
                        }
                        else if (stage == 40)
                        {
                            comum = 2; incomum = 10; raro = 35; epico = 33; lendario = 20;
                        }
                        else if (stage == 50)
                        {
                            comum = 1; incomum = 10; raro = 30; epico = 34; lendario = 25;
                        }
                        else if (stage == 60)
                        {
                            comum = 1; incomum = 8; raro = 25; epico = 36; lendario = 30;
                        }
                        else if (stage == 70)
                        {
                            comum = 1; incomum = 5; raro = 20; epico = 39; lendario = 35;
                        }
                        else if (stage == 80)
                        {
                            comum = 1; incomum = 3; raro = 15; epico = 41; lendario = 40;
                        }
                        else if (stage == 90)
                        {
                            comum = 1; incomum = 2; raro = 8; epico = 39; lendario = 50;
                        }
                        else if (stage == 100)
                        {
                            comum = 0; incomum = 1; raro = 5; epico = 44; lendario = 50;
                        }
                        else
                        {
                            // For other bosses, use similar to 30
                            comum = 5; incomum = 15; raro = 30; epico = 30; lendario = 20;
                        }
                    }
                    else
                    {
                        // For non-bosses in higher groups, use values similar to previous
                        if (stage <= 39)
                        {
                            comum = 8; incomum = 22; raro = 40; epico = 25; lendario = 5;
                        }
                        else if (stage <= 49)
                        {
                            comum = 5; incomum = 18; raro = 40; epico = 30; lendario = 7;
                        }
                        else if (stage <= 59)
                        {
                            comum = 3; incomum = 12; raro = 35; epico = 35; lendario = 15;
                        }
                        else if (stage <= 69)
                        {
                            comum = 2; incomum = 8; raro = 30; epico = 40; lendario = 20;
                        }
                        else if (stage <= 79)
                        {
                            comum = 1; incomum = 5; raro = 25; epico = 40; lendario = 29;
                        }
                        else if (stage <= 89)
                        {
                            comum = 1; incomum = 3; raro = 18; epico = 42; lendario = 36;
                        }
                        else if (stage <= 99)
                        {
                            comum = 1; incomum = 2; raro = 12; epico = 40; lendario = 45;
                        }
                    }
                }
            }

            stageProbabilities.Add(new CardProbability(stage, comum, incomum, raro, epico, lendario));
        }
    }
}
