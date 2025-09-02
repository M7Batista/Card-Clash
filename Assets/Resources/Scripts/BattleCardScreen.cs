using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardScreen : MonoBehaviour
{
    [Header("Configurações do Deck")]
    public List<CardData> deck;
    public GameObject cardPrefab;

    [Header("Referências na UI")]
    public Transform playerHandArea, enemyHandArea;
    public Transform boardArea; // container do tabuleiro (grid 3x3)
    public Text playerCountText, enemyCountText;
    public GameObject roulletPrefab;

    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> enemyHand = new List<CardData>();

    private int turn = 0; // 0 = jogador, 1 = inimigo
    private int boardSlots = 9;
    private int filledSlots = 0;
    private bool hasStarted = false;
    public static BattleCardScreen Instance;


    // Chamado quando a tela é aberta pelo controlador
    public void OnScreenOpened()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            StartGame();
            Debug.Log("Tela de Batalha de Cartas aberta!");
        }
    }

    void Start()
    {
        Instance = this;
        
        
    }

    void StartGame()
    {
        // inicializa os contadores (provavelmente 0)
        //UpdateBoardCounts();
        Instantiate(roulletPrefab, this.transform);
        StartCoroutine(CardDealer.Instance.DealCards(deck, playerHand, enemyHand, playerHandArea, enemyHandArea, cardPrefab));
    }

    public void GoToGalleryScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Gallery");
    }

    public void StartPlayerTurn()
    {
        Debug.Log("Player começa!");
        turn = 0; // Player
    }

    public void StartEnemyTurn()
    {
        Debug.Log("Inimigo começa!");
        turn = 1; // Enemy
        Invoke(nameof(EnemyPlay), 1f);
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

    // ===============================
    // 🔹 Lógica de Turnos
    // ===============================

    public void OnPlayerCardPlaced(CardUI cardUI)
    {
        if (turn != 0) return;

        int index = cardUI.transform.parent.GetSiblingIndex();
        Debug.Log("Jogador jogou: " + cardUI.cardData.cardName + " no slot " + index);

        filledSlots++;
        playerHand.Remove(cardUI.cardData);

        // Checa capturas (retorna true se houve captura)
        bool anyCapture = CheckCaptures(index);

        // Atualiza contadores no UI
        UpdateBoardCounts();

        NextTurn();
    }

    void EnemyPlay()
    {
        if (enemyHand.Count == 0) return;

        CardData bestCard = null;
        Transform bestSlot = null;
        int bestScore = -1;
        CardUI bestCardUI = null;

        // Escolhe a melhor carta e slot
        foreach (var card in enemyHand)
        {
            foreach (Transform slot in boardArea)
            {
                if (slot.childCount > 0) continue;

                int score = EvaluateMove(card, slot);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCard = card;
                    bestSlot = slot;

                    // pega a referência do objeto real na mão inimiga
                    foreach (Transform c in enemyHandArea)
                    {
                        var ui = c.GetComponent<CardUI>();
                        if (ui != null && ui.cardData == bestCard)
                        {
                            bestCardUI = ui;
                            break;
                        }
                    }
                }
            }
        }

        if (bestCard != null && bestSlot != null && bestCardUI != null)
        {
            enemyHand.Remove(bestCard);

            // move a carta real para o slot escolhido
            bestCardUI.transform.SetParent(bestSlot, false);

            var rect = bestCardUI.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            int index = bestSlot.GetSiblingIndex();

            // Checa capturas
            bool anyCapture = CheckCaptures(index);

            // Atualiza contadores no UI
            UpdateBoardCounts();

            Debug.Log("Inimigo jogou: " + bestCard.cardName + " no slot " + index + " (score " + bestScore + ")");
        }

        filledSlots++;
        NextTurn();
    }

    int EvaluateMove(CardData card, Transform slot)
    {
        int score = 0;
        int index = slot.GetSiblingIndex();
        int row = index / 3;
        int col = index % 3;

        if (row > 0)
        {
            var neighbor = boardArea.GetChild(index - 3);
            if (neighbor.childCount > 0)
            {
                var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
                if (card.top > neighborCard.bottom) score++;
            }
        }
        if (row < 2)
        {
            var neighbor = boardArea.GetChild(index + 3);
            if (neighbor.childCount > 0)
            {
                var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
                if (card.bottom > neighborCard.top) score++;
            }
        }
        if (col > 0)
        {
            var neighbor = boardArea.GetChild(index - 1);
            if (neighbor.childCount > 0)
            {
                var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
                if (card.left > neighborCard.right) score++;
            }
        }
        if (col < 2)
        {
            var neighbor = boardArea.GetChild(index + 1);
            if (neighbor.childCount > 0)
            {
                var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
                if (card.right > neighborCard.left) score++;
            }
        }

        return score;
    }

    void NextTurn()
    {
        if (filledSlots >= boardSlots)
        {
            EndGame();
            return;
        }

        turn = (turn + 1) % 2;

        if (turn == 0)
        {
            Debug.Log("Turno do jogador.");
            DraggableCard.CanDrag = true;
        }
        else
        {
            Debug.Log("Turno do inimigo...");
            Invoke(nameof(EnemyPlay), 1f);
            DraggableCard.CanDrag = false;
        }
    }

    // ===============================
    // 🔹 Nova versão de CheckCaptures com retorno
    // ===============================
    bool CheckCaptures(int index)
    {
        bool anyCapture = false;

        var placedSlot = boardArea.GetChild(index);
        var placedCard = placedSlot.GetChild(0).GetComponent<CardUI>();

        int row = index / 3;
        int col = index % 3;

        if (row > 0) anyCapture |= CaptureCheck(placedCard, index - 3, placedCard.cardData.top, "bottom");
        if (row < 2) anyCapture |= CaptureCheck(placedCard, index + 3, placedCard.cardData.bottom, "top");
        if (col > 0) anyCapture |= CaptureCheck(placedCard, index - 1, placedCard.cardData.left, "right");
        if (col < 2) anyCapture |= CaptureCheck(placedCard, index + 1, placedCard.cardData.right, "left");

        return anyCapture;
    }

    bool CaptureCheck(CardUI placedCard, int neighborIndex, int placedValue, string neighborSide)
    {
        var neighborSlot = boardArea.GetChild(neighborIndex);
        if (neighborSlot.childCount == 0) return false;

        var neighborCard = neighborSlot.GetChild(0).GetComponent<CardUI>();
        if (neighborCard == null || neighborCard.owner == placedCard.owner) return false;

        int neighborValue = 0;
        switch (neighborSide)
        {
            case "top": neighborValue = neighborCard.cardData.top; break;
            case "bottom": neighborValue = neighborCard.cardData.bottom; break;
            case "left": neighborValue = neighborCard.cardData.left; break;
            case "right": neighborValue = neighborCard.cardData.right; break;
        }

        if (placedValue > neighborValue)
        {
            neighborCard.SetOwner(placedCard.owner);
            Debug.Log($"{placedCard.owner} capturou {neighborCard.cardData.cardName}!");

            // chama a animação de flip com o novo dono
            var flip = neighborCard.GetComponent<CardFlip>();
            if (flip != null) flip.FlipCard(placedCard.owner);

            return true;
        }

        return false;
    }

    void UpdateBoardCounts()
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

        if (playerCountText != null)
            playerCountText.text = playerCount.ToString();
        if (enemyCountText != null)
            enemyCountText.text = enemyCount.ToString();
    }

    void EndGame()
    {
        int playerCount = 0;
        int enemyCount = 0;

        // percorre todos os slots do tabuleiro
        for (int i = 0; i < boardArea.childCount; i++)
        {
            var slot = boardArea.GetChild(i);
            if (slot.childCount > 0)
            {
                var cardUI = slot.GetChild(0).GetComponent<CardUI>();
                if (cardUI != null)
                {
                    if (cardUI.owner == Owner.Player)
                        playerCount++;
                    else if (cardUI.owner == Owner.Enemy)
                        enemyCount++;
                }
            }
        }

        // decidir o vencedor
        if (playerCount > enemyCount)
        {
            Debug.Log($"Fim de jogo! Jogador venceu ({playerCount} x {enemyCount})");

            EndGameUI.instance.ShowEndGame(true);

        }
        else if (enemyCount > playerCount)
        {
            Debug.Log($"Fim de jogo! Inimigo venceu ({enemyCount} x {playerCount})");
            EndGameUI.instance.ShowEndGame(false);
        }
        else
        {
            Debug.Log($"Fim de jogo! Empate ({playerCount} x {enemyCount})");
        }

    }
    public void RestartBattle()
    {
        Debug.Log("Reiniciando a batalha...");

        // 🔹 Limpa tabuleiro
        foreach (Transform slot in boardArea)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }

        // 🔹 Limpa mãos
        foreach (Transform card in playerHandArea)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyHandArea)
        {
            Destroy(card.gameObject);
        }

        // 🔹 Reseta variáveis
        playerHand.Clear();
        enemyHand.Clear();
        filledSlots = 0;
        turn = 0;
        hasStarted = false; // para garantir que o StartGame rode de novo

        // 🔹 Reinicia contadores de UI
        UpdateBoardCounts();
        
            playerCountText.text = "0";
       
            enemyCountText.text = "0";

        // 🔹 Fecha UI de fim de jogo
        EndGameUI.instance.CloseEndGame();
        // 🔹 Recomeça
        StartGame();
    }

}
