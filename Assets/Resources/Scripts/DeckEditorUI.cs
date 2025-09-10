using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditorUI : MonoBehaviour
{
    public static DeckEditorUI Instance;

    [Header("Referências")]
    public Transform collectionContainer; // ScrollView content (todas cartas do jogador)
    public Transform deckSlotsContainer;  // Painel com 5 slots
    public Button saveButton;

    [Header("Prefabs")]
    public GameObject cardPrefab; // Miniatura usada na coleção

    [Header("Debug / Dados Atuais")]
    private List<CardData> playerCollection = new List<CardData>(); // coleção do jogador (exibida no inspector)
    private List<CardData> activeDeck = new List<CardData>();       // cartas atualmente nos slots

    private DeckSlot[] deckSlots;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        deckSlots = deckSlotsContainer.GetComponentsInChildren<DeckSlot>();

        // 1. Carregar coleção
        playerCollection.Clear();
        List<int> ownedIds = PlayerDeckManager.GetOwnedCards();
        if (ownedIds == null || ownedIds.Count == 0)
        {
            Debug.LogWarning("A coleção do jogador está vazia ou nula. Criando 5 cartas padrão.");
            ownedIds.Add(1); // Adiciona um ID padrão para evitar erros
            ownedIds.Add(2); // Adiciona um ID padrão para evitar erros
            ownedIds.Add(3); // Adiciona um ID padrão para evitar erros
            ownedIds.Add(4); // Adiciona um ID padrão para evitar erros
            ownedIds.Add(5); // Adiciona um ID padrão para evitar erros
        }
        foreach (int id in ownedIds)
        {
            CardData card = PlayerDeckManager.GetCardById(id);
            if (card != null)
                playerCollection.Add(card);
        }

        // 2. Carregar deck e remover da coleção
        List<int> deckIds = PlayerDeckManager.LoadDeck();
        foreach (int id in deckIds)
        {
            if (id != -1)
            {
                CardData card = PlayerDeckManager.GetCardById(id);
                if (card != null)
                    playerCollection.Remove(card); // garante que não duplique na coleção
            }
        }

        PopulateCollection();
        saveButton.onClick.AddListener(SaveDeck);
        LoadDeck();
    }

    private void PopulateCollection()
    {
        Debug.Log($"Populando coleção com {playerCollection.Count} cartas.");
        foreach (Transform child in collectionContainer)
            Destroy(child.gameObject);

        foreach (var card in playerCollection)
        {
            GameObject cardGO = Instantiate(cardPrefab, collectionContainer);

            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.SetCard(card, Owner.None);

            Image img = cardGO.GetComponent<Image>();
            if (img == null) img = cardGO.AddComponent<Image>();
            img.raycastTarget = true;
            if (img.sprite == null) img.color = Color.white;

        }
    }
    


    
    private void RefreshActiveDeckList()
    {
        activeDeck.Clear();
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null)
                activeDeck.Add(slot.CurrentCard.cardData);
        }
    }

    private void SaveDeck()
    {
        // Salvar o deck ativo
        List<int> deckIds = new List<int>();
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null)
                deckIds.Add(slot.CurrentCard.cardData.id);
            else
                deckIds.Add(-1); // slot vazio
        }
        PlayerDeckManager.SaveDeck(deckIds);

        // Atualizar lista de coleção (tudo que NÃO está no deck)
        RefreshActiveDeckList();

        List<int> ownedIds = new List<int>();

        // adiciona de volta a coleção visível
        foreach (var card in playerCollection)
        {
            ownedIds.Add(card.id);
        }

        // adiciona também o deck (porque no fim o jogador "possui" tanto o que está no deck quanto na coleção)
        foreach (var card in activeDeck)
        {
            ownedIds.Add(card.id);
        }

        PlayerDeckManager.SaveCollection(ownedIds);

        Debug.Log("Deck e coleção salvos com sucesso!");
    }


    private void LoadDeck()
    {
        List<int> deckIds = PlayerDeckManager.LoadDeck();

        for (int i = 0; i < deckSlots.Length; i++)
        {
            if (i < deckIds.Count && deckIds[i] != -1)
            {
                var cardData = PlayerDeckManager.GetCardById(deckIds[i]);
                if (cardData != null)
                {
                    deckSlots[i].SetCard(cardData);
                }
            }
            else
            {
                //deckSlots[i].ClearSlot();
            }
        }

        RefreshActiveDeckList();
    }
}
