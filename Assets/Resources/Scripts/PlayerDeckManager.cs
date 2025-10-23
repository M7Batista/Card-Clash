using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia a coleção do jogador (com duplicatas) e o deck ativo (5 slots),
/// carregando CardData diretamente de Resources/Cards.
/// </summary>
public static class PlayerDeckManager
{
    private const string COLLECTION_KEY = "PlayerCollection"; // coleção do jogador (somente os ids dos cards. podem ter duplicatas)
    private const string DECK_KEY = "PlayerDeck";             // deck ativo (5 ids ou -1)

    // Cache de CardData carregados de Resources/Cards
    private static List<CardData> _allCardsCache;
    private static Dictionary<int, CardData> _idLookup;

    // ======================================================
    // 🔹 Cache / Carregamento de Cards
    // ======================================================
    private static void EnsureCacheLoaded()
    {
        if (_allCardsCache != null && _idLookup != null) return;

        // Carrega todos os CardData de Assets/Resources/Cards
        var loaded = Resources.LoadAll<CardData>("Cards");
        _allCardsCache = new List<CardData>(loaded);
        _idLookup = new Dictionary<int, CardData>();

        foreach (var c in loaded)
        {
            if (c == null) continue;

            if (!_idLookup.ContainsKey(c.id))
            {
                _idLookup[c.id] = c;
            }
            else
            {
                Debug.LogWarning($"[PlayerDeckManager] ID duplicado {c.id} em Resources/Cards. " +
                                 $"Usando o primeiro encontrado ('{_idLookup[c.id].name}'), ignorando '{c.name}'.");
            }
        }
    }

    /// <summary>Retorna uma cópia da lista de todos os CardData carregados (somente leitura para quem chama).</summary>
    public static List<CardData> GetAllCards()
    {
        EnsureCacheLoaded();
        return new List<CardData>(_allCardsCache);
    }

    /// <summary>Busca um CardData pelo ID. Retorna null se não encontrado.</summary>
    public static CardData GetCardById(int id)
    {
        EnsureCacheLoaded();
        if (_idLookup.TryGetValue(id, out var data)) return data;

        Debug.LogWarning($"[PlayerDeckManager] Card id {id} não encontrado em Resources/Cards.");
        return null;
    }

    // ======================================================
    // 🔹 Coleção do Jogador (com duplicatas)
    // ======================================================
    public static void SaveCollection(List<int> ownedIds)
    {
        string json = JsonHelper.ToJson(ownedIds.ToArray(), true);
        PlayerPrefs.SetString(COLLECTION_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[DeckManager] Coleção salva com {ownedIds.Count} cartas (pode conter duplicatas).");
    }

    public static List<int> GetOwnedCards()
    {
        EnsureCacheLoaded();

        // 🔹 Caso não exista coleção salva, o jogador recebe cartas aleatórias
        if (!PlayerPrefs.HasKey(COLLECTION_KEY))
        {
            Dialog.Instance.ShowMessage("You will start with 5 common cards!");

            List<int> starterCollection = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                int randIndex = Random.Range(0, 20);
                starterCollection.Add(_allCardsCache[randIndex].id);
            }

            // 🔹 Salva a coleção inicial
            SaveCollection(starterCollection);

            Debug.Log($"[DeckManager] Nenhuma coleção encontrada. Gerada coleção inicial com {starterCollection.Count} cartas aleatórias.");
            return starterCollection;
        }

        // 🔹 Caso exista coleção salva
        string json = PlayerPrefs.GetString(COLLECTION_KEY);

        if (string.IsNullOrEmpty(json))
            return new List<int>();

        int[] ids = JsonHelper.FromJson<int>(json);
        if (ids == null)
            return new List<int>();

        return new List<int>(ids);
    }


    // ======================================================
    // 🔹 Deck Ativo (5 slots; usar -1 para vazio)
    // ======================================================
    public static void SaveDeck(List<int> deckIds)
    {
        string json = JsonHelper.ToJson(deckIds.ToArray(), true);
        PlayerPrefs.SetString(DECK_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[DeckManager] Deck salvo com {deckIds.Count} slots.");
    }

    public static List<int> LoadDeck()
    {
        if (!PlayerPrefs.HasKey(DECK_KEY))
            return new List<int>(); // sem deck salvo ainda

        string json = PlayerPrefs.GetString(DECK_KEY);

        if (string.IsNullOrEmpty(json))
            return new List<int>();

        int[] ids = JsonHelper.FromJson<int>(json);
        if (ids == null)
            return new List<int>();

        return new List<int>(ids);
    }
    /// <summary>Adiciona uma carta à coleção do jogador.</summary>
    public static void AddCardToCollection(int cardId)
    {
        var collection = GetOwnedCards();
        collection.Add(cardId);
        SaveCollection(collection);
        Debug.Log($"[DeckManager] Carta adicionada à coleção: {cardId}");
    }

    /// <summary>Remove uma carta da coleção do jogador (apenas uma cópia).</summary>
    public static bool RemoveCardFromCollection(int cardId)
    {
        var collection = GetOwnedCards();
        if (collection.Contains(cardId))
        {
            collection.Remove(cardId);
            SaveCollection(collection);

            // Também verificar se estava no deck
            var deck = LoadDeck();
            if (deck.Contains(cardId))
            {
                deck.Remove(cardId);
                SaveDeck(deck);
                Debug.Log($"[DeckManager] Carta removida do deck ativo: {cardId}");
            }

            Debug.Log($"[DeckManager] Carta removida da coleção: {cardId}");
            return true;
        }

        Debug.LogWarning($"[DeckManager] Tentou remover carta {cardId}, mas não estava na coleção.");
        return false;
    }

    /// <summary>Retorna uma lista de CardData que representa a coleção completa.</summary>
    public static List<CardData> GetOwnedCardData()
    {
        var ids = GetOwnedCards();
        var result = new List<CardData>();
        foreach (var id in ids)
        {
            var data = GetCardById(id);
            if (data != null) result.Add(data);
        }
        return result;
    }

    /// <summary>Retorna uma lista de CardData do deck ativo.</summary>
    public static List<CardData> GetDeckCardData()
    {
        var ids = LoadDeck();
        var result = new List<CardData>();
        foreach (var id in ids)
        {
            var data = GetCardById(id);
            if (data != null) result.Add(data);
        }
        return result;
    }
}

/// <summary>
/// Auxiliar para serializar arrays em JSON (PlayerPrefs).
/// </summary>
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array, bool prettyPrint = false)
    {
        Wrapper<T> wrapper = new Wrapper<T> { Items = array };
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

}
