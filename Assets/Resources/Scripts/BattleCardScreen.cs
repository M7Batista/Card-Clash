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
    private int filledSlots = 0;
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
        Invoke(nameof(EnemyPlay), 2f);
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
        int index = cardUI.transform.parent.GetSiblingIndex();
        Debug.Log("Jogador jogou: " + cardUI.cardData.cardName + " no slot " + index);
        filledSlots++;
        playerActiveDeck.Remove(cardUI.cardData);

        // Checa capturas
        bool anyCapture = CheckCaptures(index);

        // Atualiza contadores no UI
        UpdateBoardCounts();
        currentTurn = Owner.Enemy;
        NextTurn();
    }


    void EnemyPlay()
    {
        if (enemyActiveDeck.Count == 0) return;

        CardData bestCard = null;
        Transform bestSlot = null;
        int bestScore = -1;
        CardUI bestCardUI = null;

        // Escolhe a melhor carta e slot
        foreach (var card in enemyActiveDeck)
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
            enemyActiveDeck.Remove(bestCard);

            // 🔹 anima movimento da mão até o slot
            StartCoroutine(AnimateEnemyCard(bestCardUI, bestSlot, () =>
            {
                int index = bestSlot.GetSiblingIndex();

                bool anyCapture = CheckCaptures(index);
                UpdateBoardCounts();

                Debug.Log("Inimigo jogou: " + bestCard.cardName + " no slot " + index + " (score " + bestScore + ")");

                filledSlots++;

                // 🔹 Só depois da animação terminar passa a vez
                currentTurn = Owner.Player;
                NextTurn();
            }));
        }

    }
    IEnumerator AnimateEnemyCard(CardUI cardUI, Transform targetSlot, System.Action onComplete)
    {
        Transform startParent = cardUI.transform.parent;
        Vector3 startPos = cardUI.transform.position;
        Vector3 endPos = targetSlot.position;

        float duration = 0.5f;
        float elapsed = 0f;

        // mantém no topo da UI para não ficar atrás
        cardUI.transform.SetParent(boardArea.parent, true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cardUI.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 🔹 fixa no slot final
        cardUI.transform.SetParent(targetSlot, false);
        var rect = cardUI.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        onComplete?.Invoke();
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
            Invoke(nameof(EnemyPlay), 1f);
        }
    }

    // ===============================
    // 🔹 Capturas
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
