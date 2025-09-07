using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerDeckData
{
    public List<int> cardIds = new List<int>(); // IDs (índices na lista global "deck")
}

public static class PlayerDeckManager
{
    private const string DeckKey = "PlayerDeck";
    private const int DefaultDeckSize = 5;

    /// <summary>
    /// Carrega o deck salvo no dispositivo
    /// </summary>
    public static List<int> LoadDeck()
    {
        if (!PlayerPrefs.HasKey(DeckKey))
            return null;

        string json = PlayerPrefs.GetString(DeckKey);
        Debug.Log($"Loaded Deck JSON: {json}");
        PlayerDeckData data = JsonUtility.FromJson<PlayerDeckData>(json);
        return data.cardIds;
    }

    /// <summary>
    /// Salva o deck no dispositivo
    /// </summary>
    public static void SaveDeck(List<int> cardIds)
    {
        PlayerDeckData data = new PlayerDeckData { cardIds = cardIds };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(DeckKey, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Cria um deck inicial (primeiras 5 cartas) se ainda não existir
    /// </summary>
    public static List<int> GetOrCreateDeck(List<CardData> allCards)
    {
        List<int> playerDeckIds = LoadDeck();
        if (playerDeckIds == null || playerDeckIds.Count == 0)
        {
            playerDeckIds = new List<int>();
            for (int i = 0; i < DefaultDeckSize && i < allCards.Count; i++)
                playerDeckIds.Add(i);
                Debug.Log("Creating default player deck.");

            SaveDeck(playerDeckIds);
        }
        return playerDeckIds;
    }

    /// <summary>
    /// Converte IDs em cartas reais
    /// </summary>
    public static List<CardData> ConvertToCards(List<int> ids, List<CardData> allCards)
    {
        List<CardData> deckCards = new List<CardData>();
        foreach (int id in ids)
        {
            if (id >= 0 && id < allCards.Count)
                deckCards.Add(allCards[id]);
        }
        return deckCards;
    }

    // ======================================================
    // 🔹 Funções extras para edição do deck
    // ======================================================

    /// <summary>
    /// Adiciona uma carta ao deck (se não estiver cheia ou duplicada)
    /// </summary>
    public static bool AddCard(int cardId, int maxDeckSize = 5)
    {
        List<int> currentDeck = LoadDeck() ?? new List<int>();

        if (currentDeck.Count >= maxDeckSize)
            return false; // deck cheio

        if (currentDeck.Contains(cardId))
            return false; // já existe

        currentDeck.Add(cardId);
        SaveDeck(currentDeck);
        return true;
    }

    /// <summary>
    /// Remove uma carta do deck (se existir)
    /// </summary>
    public static bool RemoveCard(int cardId)
    {
        List<int> currentDeck = LoadDeck();
        if (currentDeck == null || !currentDeck.Contains(cardId))
            return false;

        currentDeck.Remove(cardId);
        SaveDeck(currentDeck);
        return true;
    }

    /// <summary>
    /// Limpa todo o deck
    /// </summary>
    public static void ClearDeck()
    {
        SaveDeck(new List<int>());
    }
}
