using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckEditorUI : MonoBehaviour
{
    public static DeckEditorUI Instance;

    [Header("Referências")]
    public Transform uiCanvas;
    public Transform collectionContainer;
    public Transform deckSlotsContainer;
    public Button saveButton, clearButton;
    public TextMeshProUGUI combatPowerText;

    [Header("Prefabs")]
    public GameObject cardPrefab;
    public GameObject floatingMessagePrefab;
    public List<CardData> playerCollection = new List<CardData>();
    private List<CardData> activeDeck = new List<CardData>();

    private DeckSlot[] deckSlots;

    // 🔑 Controle de seleção
    private CardUI selectedCollectionCardUI; // card clicado na coleção para substituir

    private void Awake() => Instance = this;

    /*private void Start()
    {
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
        LoadDeck();

        saveButton.onClick.AddListener(SaveDeck);
        clearButton.onClick.AddListener(ClearSlot);
    }*/
    void OnEnable()
    {
        Debug.Log("DeckEditorUI: OnScreenOpened called");
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
        LoadDeck();

        saveButton.onClick.AddListener(SaveDeck);
        clearButton.onClick.AddListener(ClearSlot);
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
        // 🔹 Se o card já está no deck (check ativo) → remove uma ocorrência
        if (cardUI.isChecked) // ou cardUI.ShowCheckmark == true, dependendo da sua implementação
        {
            // procura um slot que contenha esse card
            DeckSlot slotWithCard = FindSlotWithCard(cardUI.cardData.id);
            if (slotWithCard != null)
            {
                slotWithCard.ClearSlot();        // remove do deck
                MarkCollectionCard(cardUI, false); // remove o checkmark
                RefreshActiveDeckList();
                return;
            }
        }

        // 1️⃣ Se ainda existe slot vazio → adiciona direto
        DeckSlot emptySlot = FindEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.SetCard(cardUI.cardData);
            MarkCollectionCard(cardUI, true);
            RefreshActiveDeckList();
            return;
        }

        UpdateCombatPower();
    }
    private DeckSlot FindSlotWithCard(int cardId)
    {
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null && slot.CurrentCard.cardData.id == cardId)
            {
                return slot;
            }
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

            // libera card antigo
            UnmarkCollectionCard(slot.CurrentCard.cardData);

            // coloca novo
            slot.SetCard(newCard);
            MarkCollectionCard(selectedCollectionCardUI, true);

            //ClearSubstitutionMode();
        }
        else
        {
            // remove card do slot
            UnmarkCollectionCard(slot.CurrentCard.cardData);
            slot.ClearSlot();
        }

        RefreshActiveDeckList();
        UpdateCombatPower();
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

    private void UnmarkCollectionCard(CardData cardData)
    {
        foreach (Transform child in collectionContainer)
        {
            var ui = child.GetComponent<CardUI>();
            if (ui.cardData.id == cardData.id)
            {
                ui.ShowCheckmark(false);
                break;
            }
        }
    }
    public void ClearSlot()
    {
        foreach (var slot in deckSlots)
        {
            if (slot.CurrentCard != null)
            {
                UnmarkCollectionCard(slot.CurrentCard.cardData);
                slot.ClearSlot();
            }
        }
        RefreshActiveDeckList();
        UpdateCombatPower();
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

    private void SaveDeck()
    {
        List<int> deckIds = new List<int>();
        foreach (var slot in deckSlots)
            deckIds.Add(slot.CurrentCard != null ? slot.CurrentCard.cardData.id : -1);

        PlayerDeckManager.SaveDeck(deckIds);

        Debug.Log("Deck salvo!");
        // Exibe mensagem flutuante
        GameObject go = Instantiate(floatingMessagePrefab, uiCanvas);
        go.transform.localPosition = Vector3.zero; // aparece no centro
        go.GetComponent<FloatingMessage>().Show("Deck saved successfully!");
    }

    private void LoadDeck()
    {
        List<int> deckIds = PlayerDeckManager.LoadDeck();
        for (int i = 0; i < deckSlots.Length; i++)
        {
            var slot = deckSlots[i];
            if (i < deckIds.Count && deckIds[i] != -1)
            {
                var card = PlayerDeckManager.GetCardById(deckIds[i]);
                slot.SetCard(card);
                MarkCollectionCard(FindCollectionUI(card.id), true);
            }

            // clique no slot
            Button btn = slot.GetComponent<Button>();
            if (btn == null) btn = slot.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => OnDeckSlotClicked(slot));
        }

        RefreshActiveDeckList();
        UpdateCombatPower();
    }



    private CardUI FindCollectionUI(int cardId)
    {
        foreach (Transform child in collectionContainer)
        {
            var ui = child.GetComponent<CardUI>();
            if (ui.cardData.id == cardId) return ui;
        }
        return null;
    }
    /// <summary>
    /// Calcula o poder de combate do deck atual e atualiza o TextMeshPro.
    /// </summary>
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
}
