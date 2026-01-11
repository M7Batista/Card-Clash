using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DeckEditorUI : MonoBehaviour
{
    public static DeckEditorUI Instance;

    [Header("Referências")]
    public Transform uiCanvas;
    public Transform collectionContainer;
    public Transform deckSlotsContainer;
    public Button clearButton, autoAssignButton;
    public TextMeshProUGUI combatPowerText;
    public ScrollRect scrollRect;

    [Header("Prefabs")]
    public GameObject cardPrefab;
    public GameObject floatingMessagePrefab;

    [Header("Dados do Jogador")]
    public List<CardData> playerCollection = new List<CardData>();
    private List<CardData> activeDeck = new List<CardData>();

    private DeckSlot[] deckSlots;
    private CardUI selectedCollectionCardUI; // card clicado na coleção para substituir

    private void Awake() => Instance = this;

    void OnEnable()
    {
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

        deckSlots = deckSlotsContainer.GetComponentsInChildren<DeckSlot>();

        // Carrega coleção
        playerCollection.Clear();
        List<int> ownedIds = PlayerDeckManager.GetOwnedCards();
        foreach (int id in ownedIds)
        {
            var card = PlayerDeckManager.GetCardById(id);
            if (card != null) playerCollection.Add(card);
        }

        PopulateCollection();
        StartCoroutine(LoadDeckWithDelay());

        clearButton.onClick.AddListener(ClearSlot);
        autoAssignButton.onClick.AddListener(AutoAssignStrongestCards);
    }

    private IEnumerator LoadDeckWithDelay()
    {
        yield return null;
        LoadActiveDeck();
    }

    private void PopulateCollection()
    {
        foreach (Transform child in collectionContainer)
            Destroy(child.gameObject);

        foreach (var card in playerCollection)
        {
            GameObject cardGO = Instantiate(cardPrefab, collectionContainer);
            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.SetCard(card, Owner.None);

            // clique no card da coleção
            Button btn = cardGO.GetComponent<Button>();
            if (btn == null) btn = cardGO.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCollectionCardClicked(cardUI));
        }
    }

    private void OnCollectionCardClicked(CardUI cardUI)
    {
        // 🔹 Se o card já está no deck → remove uma ocorrência
        if (cardUI.isChecked)
        {
            DeckSlot slotWithCard = FindSlotWithCard(cardUI.cardData.id);
            if (slotWithCard != null)
            {
                slotWithCard.ClearSlot();
                RefreshMarksForCardID(cardUI.cardData.id);
                RefreshActiveDeckList();
                AutoSaveDeck();
                return;
            }
        }

        // 🔹 Se ainda existe slot vazio → adiciona
        DeckSlot emptySlot = FindEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.SetCard(cardUI.cardData);
            RefreshMarksForCardID(cardUI.cardData.id);
            RefreshActiveDeckList();
            AutoSaveDeck();
            return;
        }

        UpdateCombatPower();
    }

    private DeckSlot FindSlotWithCard(int cardId)
    {
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null && slot.CurrentCard.cardData.id == cardId)
                return slot;
        }
        return null;
    }

    private void OnDeckSlotClicked(DeckSlot slot)
    {
        if (slot.CurrentCard == null) return;

        if (selectedCollectionCardUI != null)
        {
            // substituição
            CardData newCard = selectedCollectionCardUI.cardData;
            slot.SetCard(newCard);
            RefreshMarksForCardID(newCard.id);
        }
        else
        {
            // remove card do slot
            int id = slot.CurrentCard.cardData.id;
            slot.ClearSlot();
            RefreshMarksForCardID(id);
        }

        RefreshActiveDeckList();
        UpdateCombatPower();
        AutoSaveDeck();
    }

    private DeckSlot FindEmptySlot()
    {
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard == null) return slot;
        }
        return null;
    }

    private void MarkCollectionCard(CardUI cardUI, bool selected)
    {
        cardUI.ShowCheckmark(selected);
    }

    /// <summary>
    /// Atualiza corretamente as marcas (checkmarks) para um determinado card ID,
    /// levando em conta cópias repetidas.
    /// </summary>
    private void RefreshMarksForCardID(int cardId)
    {
        // Conta quantas cópias desse ID estão no deck
        int countInDeck = CountCardCopiesInDeck(cardId);

        // Pega todas as cópias na coleção
        var copiesInCollection = FindCollectionUIs(cardId);

        // Marca apenas a quantidade de cópias que estão no deck
        for (int i = 0; i < copiesInCollection.Count; i++)
        {
            copiesInCollection[i].ShowCheckmark(i < countInDeck);
        }
    }

    private void UnmarkCollectionCard(CardData cardData)
    {
        RefreshMarksForCardID(cardData.id);
    }

    public void ClearSlot()
    {
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null)
            {
                slot.ClearSlot();
            }
        }

        // Atualiza todos os checkmarks após limpar
        foreach (Transform child in collectionContainer)
        {
            var ui = child.GetComponent<CardUI>();
            ui.ShowCheckmark(false);
        }

        RefreshActiveDeckList();
        UpdateCombatPower();
        AutoSaveDeck();
    }

    private void RefreshActiveDeckList()
    {
        activeDeck.Clear();
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null)
                activeDeck.Add(slot.CurrentCard.cardData);
        }
        UpdateCombatPower();
    }

    private void AutoSaveDeck()
    {
        List<int> deckIds = new List<int>();
        foreach (var slot in deckSlots)
            deckIds.Add(slot.CurrentCard != null ? slot.CurrentCard.cardData.id : -1);

        PlayerDeckManager.SaveDeck(deckIds);
    }

    private void LoadActiveDeck()
    {
        List<int> deckIds = PlayerDeckManager.LoadDeck();

        for (int i = 0; i < deckSlots.Length; i++)
        {
            var slot = deckSlots[i];
            CardData card = null;

            if (i < deckIds.Count && deckIds[i] != -1)
                card = PlayerDeckManager.GetCardById(deckIds[i]);

            if (card != null && playerCollection.Contains(card))
                slot.SetCard(card);
            else
                slot.ClearSlot();

            Button btn = slot.GetComponent<Button>();
            if (btn == null) btn = slot.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnDeckSlotClicked(slot));
        }

        // 🔹 Atualiza todos os checkmarks com base no deck carregado
        RefreshAllMarks();

        RefreshActiveDeckList();
        UpdateCombatPower();
    }

    private void RefreshAllMarks()
    {
        HashSet<int> ids = new HashSet<int>();
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null)
                ids.Add(slot.CurrentCard.cardData.id);
        }

        foreach (int id in ids)
            RefreshMarksForCardID(id);
    }

    // Retorna todas as cópias de um ID
    private List<CardUI> FindCollectionUIs(int cardId)
    {
        List<CardUI> list = new List<CardUI>();
        foreach (Transform child in collectionContainer)
        {
            var ui = child.GetComponent<CardUI>();
            if (ui.cardData.id == cardId)
                list.Add(ui);
        }
        return list;
    }

    private int CountCardCopiesInDeck(int cardId)
    {
        int count = 0;
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null && slot.CurrentCard.cardData.id == cardId)
                count++;
        }
        return count;
    }

    private void UpdateCombatPower()
    {
        int totalPower = 0;

        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null)
            {
                CardData c = slot.CurrentCard.cardData;
                totalPower += c.top + c.right + c.bottom + c.left;
            }
        }

        if (combatPowerText != null)
            combatPowerText.text = totalPower.ToString();
    }

    private void AutoAssignStrongestCards()
    {
        // Limpa o deck atual
        ClearSlot();

        // Ordena a coleção por poder total (descendente)
        List<CardData> sortedCards = new List<CardData>(playerCollection);
        sortedCards.Sort((a, b) => 
        {
            int powerA = a.top + a.right + a.bottom + a.left;
            int powerB = b.top + b.right + b.bottom + b.left;
            return powerB.CompareTo(powerA); // Descendente
        });

        // Adiciona as 5 cartas mais fortes ao deck
        int count = Mathf.Min(5, sortedCards.Count, deckSlots.Length);
        for (int i = 0; i < count; i++)
        {
            DeckSlot slot = deckSlots[i];
            slot.SetCard(sortedCards[i]);
            RefreshMarksForCardID(sortedCards[i].id);
        }

        RefreshActiveDeckList();
        UpdateCombatPower();
        AutoSaveDeck();
    }
}
