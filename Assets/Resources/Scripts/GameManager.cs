using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Configurações do Deck")]
    public List<CardData> deck;

    [Header("Referências na UI")]
    public Transform playerHandArea;
    public Transform enemyHandArea;
    public GameObject cardPrefab;
    public Transform boardArea; // container do tabuleiro (grid 3x3)

    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> enemyHand = new List<CardData>();

    private int turn = 0; // 0 = jogador, 1 = inimigo
    private int boardSlots = 9; // número máximo de posições no tabuleiro
    private int filledSlots = 0;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        List<CardData> shuffledDeck = new List<CardData>(deck);
        Shuffle(shuffledDeck);

        for (int i = 0; i < 5; i++)
        {
            playerHand.Add(shuffledDeck[i]);
            enemyHand.Add(shuffledDeck[i + 5]);
        }

        foreach (var cardData in playerHand)
        {
            var cardObj = Instantiate(cardPrefab, playerHandArea);
            var cardUI = cardObj.GetComponent<CardUI>();
            cardUI.SetCard(cardData);

            var drag = cardObj.AddComponent<DraggableCard>();
            drag.OnCardPlaced += OnPlayerCardPlaced; // evento quando o jogador joga
        }

        foreach (var cardData in enemyHand)
        {
            var cardObj = Instantiate(cardPrefab, enemyHandArea);
            var cardUI = cardObj.GetComponent<CardUI>();
            cardUI.SetCard(cardData);

            // inimigo não arrasta
            var drag = cardObj.GetComponent<DraggableCard>();
            if (drag != null) drag.enabled = false;
        }

        Debug.Log("Jogo iniciado. Turno do jogador.");
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

    void OnPlayerCardPlaced(CardUI cardUI)
    {
        if (turn != 0) return; // só joga no turno do player

        Debug.Log("Jogador jogou: " + cardUI.cardData.cardName);

        filledSlots++;
        playerHand.Remove(cardUI.cardData);

        // passa o turno
        NextTurn();
    }

    void EnemyPlay()
{
    if (enemyHand.Count == 0) return;

    CardData bestCard = null;
    Transform bestSlot = null;
    int bestScore = -1;

    // testa todas as combinações (carta x slot vazio)
    foreach (var card in enemyHand)
    {
        foreach (Transform slot in boardArea)
        {
            if (slot.childCount > 0) continue; // slot ocupado

            int score = EvaluateMove(card, slot);
            if (score > bestScore)
            {
                bestScore = score;
                bestCard = card;
                bestSlot = slot;
            }
        }
    }

    if (bestCard != null && bestSlot != null)
    {
        enemyHand.Remove(bestCard);

        // remove da mão do inimigo (primeiro objeto visual encontrado com o mesmo nome)
        for (int i = 0; i < enemyHandArea.childCount; i++)
        {
            var child = enemyHandArea.GetChild(i).GetComponent<CardUI>();
            if (child != null && child.cardData == bestCard)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        // instancia no slot escolhido
        var cardObj = Instantiate(cardPrefab, bestSlot);
        var cardUI = cardObj.GetComponent<CardUI>();
        cardUI.SetCard(bestCard);

        var rect = cardObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        Debug.Log("Inimigo jogou: " + bestCard.cardName + " (score " + bestScore + ")");
    }

    filledSlots++;
    NextTurn();
}
int EvaluateMove(CardData card, Transform slot)
{
    int score = 0;

    // posição do slot (assumindo boardArea = Grid 3x3)
    int index = slot.GetSiblingIndex();
    int row = index / 3;
    int col = index % 3;

    // checar vizinhos
    // cima
    if (row > 0)
    {
        var neighbor = boardArea.GetChild(index - 3);
        if (neighbor.childCount > 0)
        {
            var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
            if (card.top > neighborCard.bottom) score++;
        }
    }
    // baixo
    if (row < 2)
    {
        var neighbor = boardArea.GetChild(index + 3);
        if (neighbor.childCount > 0)
        {
            var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
            if (card.bottom > neighborCard.top) score++;
        }
    }
    // esquerda
    if (col > 0)
    {
        var neighbor = boardArea.GetChild(index - 1);
        if (neighbor.childCount > 0)
        {
            var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
            if (card.left > neighborCard.right) score++;
        }
    }
    // direita
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
        }
        else
        {
            Debug.Log("Turno do inimigo...");
            Invoke(nameof(EnemyPlay), 1f); // espera 1 segundo antes do inimigo jogar
        }
    }

    void EndGame()
    {
        Debug.Log("Fim de jogo!");
        // aqui futuramente: lógica de pontuação/vencedor
    }
}
