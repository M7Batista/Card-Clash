using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardStealUIManager : MonoBehaviour
{
    [Header("Referências de UI")]
    public Transform playerCardContainer;
    public Transform enemyCardContainer;
    public GameObject cardPrefab; // Prefab simples de botão de carta
    public GameObject confirmModal;
    public TMP_Text confirmText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Dados")]
    private List<CardData> playerCards = new List<CardData>();
    private List<CardData> enemyCards = new List<CardData>();
    private CardData selectedCard;
    private GameObject selectedCardGO;

    private bool playerWon;
    private bool isBoss;
    public static CardStealUIManager Instance;
    private void Awake()
    {
        Instance = this;
        //gameObject.SetActive(false);
    }

    // Inicialização da tela de roubo
    public void OpenStealScreen(List<CardData> player, List<CardData> enemy, bool playerWon, bool isBoss)
    {
        Debug.Log("Abrindo tela de roubo de cartas");
        Debug.Log(player.Count + " cartas do jogador");
        Debug.Log(enemy.Count + " cartas do inimigo");
        gameObject.SetActive(true);

        this.playerCards = player;
        this.enemyCards = enemy;
        this.playerWon = playerWon;
        this.isBoss = isBoss;

        ClearContainers();
        PopulateCards(playerCardContainer, player, false);
        PopulateCards(enemyCardContainer, enemy, true);

        confirmModal.SetActive(false);

        if (!playerWon)
        {
            // Derrota → inimigo rouba automaticamente
            Invoke(nameof(EnemyStealsCard), 1.5f);
        }
    }

    private void ClearContainers()
    {
        foreach (Transform child in playerCardContainer) Destroy(child.gameObject);
        foreach (Transform child in enemyCardContainer) Destroy(child.gameObject);
    }

    private void PopulateCards(Transform container, List<CardData> cards, bool isEnemySide)
    {
        foreach (var card in cards)
        {
            Debug.Log($"Mostrando carta: {card.cardName} ({card.rarity})");
            //GameObject go = Instantiate(cardPrefab, container);
            //go.GetComponentInChildren<TMP_Text>().text = card.cardName;

            GameObject cardIstance = GameObject.Instantiate(cardPrefab, container);
            CardUI cardUI = cardIstance.GetComponent<CardUI>();
            cardUI.SetCard(card, Owner.Player);


            if (isEnemySide && playerWon)
            {
                cardIstance.AddComponent<Button>();
                Button btn = cardIstance.GetComponent<Button>();
                btn.onClick.AddListener(() => OnEnemyCardClicked(card, cardIstance));
            }
        }
    }

    private void OnEnemyCardClicked(CardData card, GameObject go)
    {
        selectedCard = card;
        selectedCardGO = go;

        // animação simples: mover um pouco para cima
        selectedCardGO.transform.localPosition += Vector3.up * 20f;

        confirmModal.SetActive(true);
        confirmText.text = $"Deseja roubar a carta \"{card.cardName}\" ({card.rarity})?";

        confirmYesButton.onClick.RemoveAllListeners();
        confirmYesButton.onClick.AddListener(() => ConfirmSteal());

        confirmNoButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.AddListener(() => CancelSteal());
    }


    private void ConfirmSteal()
    {
        Debug.Log($"Jogador roubou: {selectedCard.cardName} ({selectedCard.rarity})");

        // Salvar na coleção oficial
        PlayerDeckManager.AddCardToCollection(selectedCard.id);

        confirmModal.SetActive(false);
        EndStealScreen();
    }

    private void CancelSteal()
    {
        // Volta a carta para posição original
        if (selectedCardGO != null)
            selectedCardGO.transform.localPosition -= Vector3.up * 20f;

        confirmModal.SetActive(false);
        selectedCard = null;
        selectedCardGO = null;
    }


    private void EnemyStealsCard()
    {
        var playerCollection = PlayerDeckManager.GetOwnedCardData();
        if (playerCollection.Count == 0)
        {
            Debug.Log("Jogador não tem cartas para perder.");
            EndStealScreen();
            return;
        }

        CardData stolen = isBoss
            ? WeightedRandomSteal(playerCollection)
            : playerCollection[Random.Range(0, playerCollection.Count)];

        // Remover da coleção (e do deck se estiver equipado)
        PlayerDeckManager.RemoveCardFromCollection(stolen.id);

        Debug.Log($"Inimigo roubou: {stolen.cardName} ({stolen.rarity})");

        // TODO: animar destaque da carta roubada
        EndStealScreen();
    }


    private CardData WeightedRandomSteal(List<CardData> pool)
    {
        List<CardData> weighted = new List<CardData>();

        foreach (var card in pool)
        {
            int weight = 1;
            switch (card.rarity)
            {
                case CardRarity.Common: weight = 1; break;
                case CardRarity.Uncommon: weight = 2; break;
                case CardRarity.Rare: weight = 4; break;
                case CardRarity.Epic: weight = 6; break;
                case CardRarity.Legendary: weight = 8; break;
            }
            for (int i = 0; i < weight; i++)
                weighted.Add(card);
        }

        return weighted[Random.Range(0, weighted.Count)];
    }

    private void EndStealScreen()
    {
        BattleCardScreen.Instance.OnScreenClosed();
       
    }
}
