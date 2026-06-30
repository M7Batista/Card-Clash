using System.Collections.Generic;
using UnityEngine;

public class EnemyDeckManager : MonoBehaviour
{
    [Header("Banco de Cartas")]
    public List<CardData> allCards = new List<CardData>();
    public static EnemyDeckManager Instance;

    private void Awake()
    {
        Instance = this;
        LoadAllCards();
    }

    private void LoadAllCards()
    {
        if (allCards != null && allCards.Count > 0)
            return;

        CardData[] allCardsData = Resources.LoadAll<CardData>("Cards");
        if (allCardsData != null && allCardsData.Length > 0)
        {
            allCards = new List<CardData>(allCardsData);
        }
        else
        {
            Debug.LogWarning("EnemyDeckManager: no cards found in Resources/Cards.");
            allCards = new List<CardData>();
        }
    }

    /// <summary>
    /// Gera o deck inimigo baseado na classificação de rank.
    /// </summary>
    public List<CardData> GenerateEnemyDeck(string rankName)
    {
        LoadAllCards();

        RankRarityConfig config = RankSystem.GetRarityConfig(rankName);
        if (config.common + config.uncommon + config.rare + config.epic + config.legendary <= 0f)
        {
            config.common = 100f;
            config.uncommon = 0f;
            config.rare = 0f;
            config.epic = 0f;
            config.legendary = 0f;
        }

        List<CardData> enemyDeck = new List<CardData>();
        int deckSize = 5;
        int legendaryCount = 0;
        int epicCount = 0;
        int rareCount = 0;

        for (int i = 0; i < deckSize; i++)
        {
            CardRarity rarity = RollRarity(config);
            rarity = ClampRarityByCount(rarity, ref legendaryCount, ref epicCount, ref rareCount, config);

            List<CardData> filtered = allCards.FindAll(c => c.rarity == rarity);
            if (filtered.Count > 0)
            {
                enemyDeck.Add(filtered[Random.Range(0, filtered.Count)]);
            }
            else
            {
                CardData fallback = GetFallbackCard();
                if (fallback != null)
                    enemyDeck.Add(fallback);
            }
        }

        Debug.Log($"Enemy deck generated for rank {rankName}. Rarity config: {config.common}% common, {config.uncommon}% uncommon, {config.rare}% rare, {config.epic}% epic, {config.legendary}% legendary.");
        return enemyDeck;
    }

    private CardData GetFallbackCard()
    {
        if (allCards == null || allCards.Count == 0)
            return null;

        return allCards[Random.Range(0, allCards.Count)];
    }

    private CardRarity RollRarity(RankRarityConfig config)
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        cumulative += config.common;
        if (roll < cumulative) return CardRarity.Common;

        cumulative += config.uncommon;
        if (roll < cumulative) return CardRarity.Uncommon;

        cumulative += config.rare;
        if (roll < cumulative) return CardRarity.Rare;

        cumulative += config.epic;
        if (roll < cumulative) return CardRarity.Epic;

        return CardRarity.Legendary;
    }

    private CardRarity ClampRarityByCount(CardRarity chosen, ref int legendaryCount, ref int epicCount, ref int rareCount, RankRarityConfig config)
    {
        if (chosen == CardRarity.Legendary && legendaryCount >= config.maxLegendary)
        {
            if (config.maxEpic > 0 && epicCount < config.maxEpic)
                chosen = CardRarity.Epic;
            else if (config.maxRare > 0 && rareCount < config.maxRare)
                chosen = CardRarity.Rare;
            else
                chosen = CardRarity.Uncommon;
        }

        if (chosen == CardRarity.Epic && epicCount >= config.maxEpic)
        {
            if (config.maxRare > 0 && rareCount < config.maxRare)
                chosen = CardRarity.Rare;
            else
                chosen = CardRarity.Uncommon;
        }

        if (chosen == CardRarity.Rare && rareCount >= config.maxRare)
            chosen = CardRarity.Uncommon;

        switch (chosen)
        {
            case CardRarity.Legendary:
                legendaryCount++;
                break;
            case CardRarity.Epic:
                epicCount++;
                break;
            case CardRarity.Rare:
                rareCount++;
                break;
        }

        return chosen;
    }
}
