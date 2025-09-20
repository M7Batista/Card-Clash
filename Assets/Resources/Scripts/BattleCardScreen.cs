using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardScreen : MonoBehaviour
{
    [Header("Listas de Cards")]
    public List<CardData> playerActiveDeck = new List<CardData>();   // 🔹 As 5 cartas escolhidas pelo jogador para a partida
    public List<CardData> enemyActiveDeck = new List<CardData>();    // 🔹 As 5 cartas que o inimigo usará na partida

    [Header("Referências na UI")]
    public Transform playerHandArea;
    public Transform enemyHandArea;
    public Transform boardArea;
    public GameObject roulletPrefab;
    public GameObject cardPrefab;
    public Button restartButton;
    public Button exitButton;

    [Header("Telas")]
    public GameObject stageScreen;
    public GameObject battleScreen;
    public GameObject stealCardsScreen;

    public Owner currentTurn = Owner.None;
    public int filledSlots = 0;
    private GameObject currentRoullet;
    public static BattleCardScreen Instance;



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

    void Start()
    {
        Instance = this;
        restartButton.onClick.AddListener(RestartBattle);
        exitButton.onClick.AddListener(ExitBattle);
    }
    public void SetEnemyDeck(List<CardData> enemyDeck)
    {
        enemyActiveDeck = enemyDeck;
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
        // Verifica se o inimigo tem cartas
        if (enemyActiveDeck == null || enemyActiveDeck.Count < 5)
        {
            Dialog.Instance.ShowMessage("Enemy deck is not set! Cannot start the game.");
            Debug.LogError("❌ O deck do inimigo não está definido. O jogo não pode iniciar!");
            return;
        }
        stageScreen.SetActive(false);
        battleScreen.SetActive(true);
       
        // 🔹 Criar roleta
        currentRoullet = Instantiate(roulletPrefab, this.transform);

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


    public void OnPlayerCardPlaced(CardUI cardUI)
    {
        int index = cardUI.transform.parent.GetSiblingIndex();
        Debug.Log("Jogador jogou: " + cardUI.cardData.cardName + " no slot " + index);
        filledSlots++;
        //playerActiveDeck.Remove(cardUI.cardData);
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
        stealCardsScreen.SetActive(false);
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
    public void PosBattleSetup(int result)
    {
        // Configurações pós-batalha, se necessário
        battleScreen.SetActive(false);
        stealCardsScreen.SetActive(true);
        
        if (result == 0)
        {

            CardStealUIManager.Instance.OpenStealScreen(playerActiveDeck, enemyActiveDeck, true, false);
        }
        else if (result == 1)
        {
            CardStealUIManager.Instance.OpenStealScreen(playerActiveDeck, enemyActiveDeck, false, false);
        }
        else
        {
            stageScreen.SetActive(true);
            ExitBattle();
        }
    }
    public void ExitBattle()
    {
        battleScreen.SetActive(false);
        stealCardsScreen.SetActive(false);
        
        if (currentRoullet != null)
        {
            Destroy(currentRoullet);
        }

    }
    public void OnScreenClosed()
    {
        Debug.Log("Tela de Batalha de Cartas fechada!");

        battleScreen.SetActive(false);
        stealCardsScreen.SetActive(false);
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
