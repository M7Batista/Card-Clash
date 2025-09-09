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
    public Text playerCountText, enemyCountText;
    public GameObject roulletPrefab;

    [Header("Listas de Cartas")]
    public List<CardData> playerOwnedCards = new List<CardData>();   // 🔹 Todas as cartas que o jogador possui
    public List<CardData> playerActiveDeck = new List<CardData>();   // 🔹 As 5 cartas escolhidas pelo jogador para a partida
    public List<CardData> enemyActiveDeck = new List<CardData>();    // 🔹 As 5 cartas que o inimigo usará na partida

    public Owner currentTurn = Owner.None;
    private int boardSlots = 9;
    public int filledSlots = 0;
    private bool hasStarted = false;

    public static BattleCardScreen Instance;


    [Header("Tela de estagio")]
    public GameObject stageScreen;
    public Transform PanelDeckArea;
    public Button startBattleButton;

    [Header("Tela de batalha")]
    public GameObject battleScreen;

    public void OnScreenOpened()
    {
        Debug.Log("BattleCardScreen: OnScreenOpened chamado.");
        if (!hasStarted)
        {
            hasStarted = true;
            Debug.Log("Tela de Batalha de Cartas aberta!");

            // 🔹 Carregar todas as cartas que o jogador possui
            List<int> playerDeckIds = PlayerDeckManager.GetOrCreateDeck(allAvailableCards);
            playerOwnedCards = PlayerDeckManager.ConvertToCards(playerDeckIds, allAvailableCards);

            // 🔹 Selecionar deck ativo
            SelectPlayerActiveDeck();

            // 🔹 Exibir deck atual na UI
            ShowPlayerDeck(playerActiveDeck);
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
        startBattleButton.onClick.AddListener(StartGame);

    }

    void ShowPlayerDeck(List<CardData> deckCards)
    {
        // Limpa painel antes
        foreach (Transform child in PanelDeckArea)
            Destroy(child.gameObject);

        // Instancia cartas no painel
        foreach (CardData card in deckCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, PanelDeckArea);
            CardUI cardUI = cardGO.GetComponent<CardUI>();
            cardUI.SetCard(card, Owner.Player);

            // 🔹 Deck é só exibição, sem drag
            var drag = cardGO.GetComponent<DraggableCard>();
            if (drag != null) Destroy(drag);
        }
    }

    void StartGame()
    {
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
    }


    public void StartPlayerTurn()
    {
        Debug.Log("Player começa!");
        currentTurn = Owner.Player;
    }

    public void StartEnemyTurn()
    {
        Debug.Log("Inimigo começa!");
        currentTurn = Owner.Enemy;
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

        // Atualiza contadores no UI
        UpdateBoardCounts();
        currentTurn = Owner.Enemy;
        NextTurn();
    }

    public void UpdateBoardCounts()
    {
        int playerCount, enemyCount;
        BoardManager.Instance.GetBoardCounts(out playerCount, out enemyCount);

        if (playerCountText != null)
            playerCountText.text = playerCount.ToString();
        if (enemyCountText != null)
            enemyCountText.text = enemyCount.ToString();
    }


    public void NextTurn()
    {
        if (filledSlots >= 9)
        {
            EndGame();
            return;
        }
        if (currentTurn == Owner.Player)
        {
            Debug.Log("Turno do jogador");
            // jogador vai interagir manualmente
        }
        else if (currentTurn == Owner.Enemy)
        {
            Debug.Log("Turno do inimigo");
            Invoke(nameof(CallEnemyAI), 1f);
        }
    }





    void EndGame()
    {
        int playerCount = 0;
        int enemyCount = 0;

        for (int i = 0; i < boardArea.childCount; i++)
        {
            var slot = boardArea.GetChild(i);
            if (slot.childCount > 0)
            {
                var cardUI = slot.GetChild(0).GetComponent<CardUI>();
                if (cardUI != null)
                {
                    if (cardUI.owner == Owner.Player) playerCount++;
                    else if (cardUI.owner == Owner.Enemy) enemyCount++;
                }
            }
        }

        if (playerCount > enemyCount)
        {
            Debug.Log($"Fim de jogo! Jogador venceu ({playerCount} x {enemyCount})");
            StartCoroutine(ShowPanelEndGame(true));
        }
        else if (enemyCount > playerCount)
        {
            Debug.Log($"Fim de jogo! Inimigo venceu ({enemyCount} x {playerCount})");
            StartCoroutine(ShowPanelEndGame(false));
        }
        else
        {
            Debug.Log($"Fim de jogo! Empate ({playerCount} x {enemyCount})");
            StartCoroutine(ShowPanelEndGame(null));
        }
    }

    IEnumerator ShowPanelEndGame(bool? playerWon)
    {
        yield return new WaitForSeconds(2f);

        if (playerWon.HasValue)
            EndGameUI.instance.ShowEndGame(playerWon.Value);
        else
            Debug.Log("Empate! Ninguém vence.");
    }

    public void RestartBattle()
    {
        Debug.Log("Reiniciando a batalha...");

        foreach (Transform slot in boardArea)
        {
            foreach (Transform child in slot)
                Destroy(child.gameObject);
        }

        foreach (Transform card in playerHandArea)
            Destroy(card.gameObject);
        foreach (Transform card in enemyHandArea)
            Destroy(card.gameObject);

        enemyActiveDeck.Clear();
        playerActiveDeck.Clear();
        filledSlots = 0;
        hasStarted = false;

        UpdateBoardCounts();
        playerCountText.text = "0";
        enemyCountText.text = "0";

        EndGameUI.instance.CloseEndGame();
        StartGame();
    }
}
