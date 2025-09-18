using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardScreen : MonoBehaviour
{
    [Header("Configurações do Deck")]
    public List<CardData> allAvailableCards;    // 🔹 Todas as cartas existentes no jogo
    public GameObject cardPrefab;

    [Header("Referências na UI")]
    public Transform playerHandArea;
    public Transform enemyHandArea;
    public Transform boardArea;
    public GameObject roulletPrefab;

    [Header("Listas de Cartas")]
    public List<CardData> playerOwnedCards = new List<CardData>();   // 🔹 Todas as cartas que o jogador possui
    public List<CardData> playerActiveDeck = new List<CardData>();   // 🔹 As 5 cartas escolhidas pelo jogador para a partida
    public List<CardData> enemyActiveDeck = new List<CardData>();    // 🔹 As 5 cartas que o inimigo usará na partida

    public Owner currentTurn = Owner.None;
    //private int boardSlots = 9;
    public int filledSlots = 0;

    public static BattleCardScreen Instance;


    [Header("Tela de estagio")]
    public GameObject stageScreen;

    [Header("Tela de batalha")]
    public GameObject battleScreen;

    [Header("Botões extras")]
    public Button restartButton;
    public Button exitButton;

    public void OnScreenOpened()
    {

        Debug.Log("Tela de Batalha de Cartas aberta!");
        // 🔹 Carregar os ids do deck ativo do jogador
        List<int> playerDeckIds = PlayerDeckManager.LoadDeck();
        playerActiveDeck.Clear();
        foreach (int id in playerDeckIds)
        {
            CardData card = PlayerDeckManager.GetCardById(id);
            if (card != null)
                playerActiveDeck.Add(card);
        }

    }

    // ===============================
    // 🔹 Seleção do Deck do Jogador
    // ===============================
    private void SelectPlayerActiveDeck()
    {
        playerActiveDeck.Clear();

        if (playerOwnedCards.Count >= 5)
        {
            // Placeholder → seleciona as 5 primeiras
            for (int i = 0; i < 5; i++)
                playerActiveDeck.Add(playerOwnedCards[i]);
        }
        else
        {
            Debug.LogWarning("Jogador não possui 5 cartas, completando com cartas aleatórias.");
            playerActiveDeck.AddRange(playerOwnedCards);

            Shuffle(allAvailableCards);
            for (int i = playerActiveDeck.Count; i < 5; i++)
                playerActiveDeck.Add(allAvailableCards[i]);
        }
    }

    void Start()
    {
        Instance = this;
        restartButton.onClick.AddListener(RestartBattle);
        exitButton.onClick.AddListener(ExitBattle);
    }


    public void StartBattle()
    {
        // 🔹 Verifica se o deck do jogador está válido
        if (playerActiveDeck == null || playerActiveDeck.Count < 5)
        {
            Dialog.Instance.ShowMessage("Choose your cards before starting the game!");
            Debug.LogError("❌ O jogador não possui 5 cartas definidas no deck. O jogo não pode iniciar!");
            return;
        }
        stageScreen.SetActive(false);
        battleScreen.SetActive(true);
        // 🔹 Prepara mão inimiga (5 cartas aleatórias do total de cartas)
        enemyActiveDeck.Clear();
        Shuffle(allAvailableCards);
        for (int i = 0; i < 5 && i < allAvailableCards.Count; i++)
            enemyActiveDeck.Add(allAvailableCards[i]);

        // 🔹 Criar roleta
        Instantiate(roulletPrefab, this.transform);

        // 🔹 Distribuir cartas na mão
        StartCoroutine(CardDealer.Instance.DealCards(
            playerActiveDeck,
            enemyActiveDeck,
            playerHandArea,
            enemyHandArea,
            cardPrefab
        ));
        BoardManager.Instance.UpdateBoardCounts(); // Atualiza contadores iniciais

    }


    public void StartPlayerTurn()
    {
        currentTurn = Owner.Player;
        SetPlayerHandDraggable(true);
        BoardManager.Instance.UpdateTurnArrow(playerHandArea);
    }

    public void StartEnemyTurn()
    {
        currentTurn = Owner.Enemy;
        SetPlayerHandDraggable(false);
        BoardManager.Instance.UpdateTurnArrow(enemyHandArea);
        Invoke(nameof(CallEnemyAI), 2f);

    }
    void CallEnemyAI()
    {
        EnemyAI.Instance.PlayTurn();
    }


    void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    public void OnPlayerCardPlaced(CardUI cardUI)
    {
        int index = cardUI.transform.parent.GetSiblingIndex();
        Debug.Log("Jogador jogou: " + cardUI.cardData.cardName + " no slot " + index);
        filledSlots++;
        playerActiveDeck.Remove(cardUI.cardData);

        // 🔹 Usa o BoardManager para capturas
        bool anyCapture = BoardManager.Instance.CheckCaptures(index);

        currentTurn = Owner.Enemy;
        NextTurn();
    }



    public void NextTurn()
    {
        if (filledSlots >= 9)
        {
            BoardManager.Instance.CheckEndGame(); // ✅ agora quem decide é o BoardManager
            return;
        }

        if (currentTurn == Owner.Player)
        {
            SetPlayerHandDraggable(true);
            BoardManager.Instance.UpdateTurnArrow(playerHandArea);
            Debug.Log("Turno do jogador");
            // jogador vai interagir manualmente
        }
        else if (currentTurn == Owner.Enemy)
        {
            SetPlayerHandDraggable(false);
            BoardManager.Instance.UpdateTurnArrow(enemyHandArea);
            Debug.Log("Turno do inimigo");
            Invoke(nameof(CallEnemyAI), 1f);
        }
    }
    private void SetPlayerHandDraggable(bool canDrag)
{
    foreach (Transform child in playerHandArea)
    {
        var draggable = child.GetComponent<DraggableCard>(); // seu script de drag
        if (draggable != null)
            draggable.enabled = canDrag;
    }
}
    private void RestartBattle()
    {
        Debug.Log("Reiniciando batalha...");

        // Reinicia contadores
        filledSlots = 0;
        currentTurn = Owner.None;
        BoardManager.Instance.HideTurnArrow();

        // Limpa áreas de cartas
        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        foreach (Transform child in enemyHandArea) Destroy(child.gameObject);
        foreach (Transform slot in boardArea)
        {
            foreach (Transform card in slot)
            {
                Destroy(card.gameObject); // só destrói a carta dentro do slot
            }
        }

        // Limpa listas
        playerActiveDeck.Clear();
        enemyActiveDeck.Clear();

        // Recarrega o deck do jogador
        List<int> playerDeckIds = PlayerDeckManager.LoadDeck();
        foreach (int id in playerDeckIds)
        {
            CardData card = PlayerDeckManager.GetCardById(id);
            if (card != null)
                playerActiveDeck.Add(card);
        }

        // Prepara mão do inimigo
        Shuffle(allAvailableCards);
        for (int i = 0; i < 5 && i < allAvailableCards.Count; i++)
            enemyActiveDeck.Add(allAvailableCards[i]);

        // Recria roleta
        Instantiate(roulletPrefab, this.transform);

        // Re-distribui cartas
        StartCoroutine(CardDealer.Instance.DealCards(
            playerActiveDeck,
            enemyActiveDeck,
            playerHandArea,
            enemyHandArea,
            cardPrefab
        ));

        BoardManager.Instance.UpdateBoardCounts();
    }
    public void ExitBattle()
    {
        Debug.Log("Saindo da batalha...");

        battleScreen.SetActive(false);
        stageScreen.SetActive(true);
        BoardManager.Instance.HideTurnArrow();
        // Limpa o estado da batalha (opcional)
        filledSlots = 0;
        currentTurn = Owner.None;
        playerActiveDeck.Clear();
        enemyActiveDeck.Clear();

        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        foreach (Transform child in enemyHandArea) Destroy(child.gameObject);
        foreach (Transform slot in boardArea)
        {
            foreach (Transform card in slot)
            {
                Destroy(card.gameObject); // só destrói a carta dentro do slot
            }
        }
        // Recarrega o deck do jogador
        List<int> playerDeckIds = PlayerDeckManager.LoadDeck();
        foreach (int id in playerDeckIds)
        {
            CardData card = PlayerDeckManager.GetCardById(id);
            if (card != null)
                playerActiveDeck.Add(card);
        }
    }

}
