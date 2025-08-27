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
    private int boardSlots = 9; 
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
            cardUI.SetCard(cardData, Owner.Player);

            var drag = cardObj.AddComponent<DraggableCard>();
            drag.OnCardPlaced += OnPlayerCardPlaced; 
        }

        foreach (var cardData in enemyHand)
        {
            var cardObj = Instantiate(cardPrefab, enemyHandArea);
            var cardUI = cardObj.GetComponent<CardUI>();
            cardUI.SetCard(cardData, Owner.Enemy);

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
        if (turn != 0) return;

        int index = cardUI.transform.parent.GetSiblingIndex();
        Debug.Log("Jogador jogou: " + cardUI.cardData.cardName + " no slot " + index);

        filledSlots++;
        playerHand.Remove(cardUI.cardData);

        CheckCaptures(index);

        NextTurn();
    }

    void EnemyPlay()
    {
        if (enemyHand.Count == 0) return;

        CardData bestCard = null;
        Transform bestSlot = null;
        int bestScore = -1;

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
                }
            }
        }

        if (bestCard != null && bestSlot != null)
        {
            enemyHand.Remove(bestCard);

            for (int i = 0; i < enemyHandArea.childCount; i++)
            {
                var child = enemyHandArea.GetChild(i).GetComponent<CardUI>();
                if (child != null && child.cardData == bestCard)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }

            var cardObj = Instantiate(cardPrefab, bestSlot);
            var cardUI = cardObj.GetComponent<CardUI>();
            cardUI.SetCard(bestCard, Owner.Enemy);

            int index = bestSlot.GetSiblingIndex();
            CheckCaptures(index);

            var rect = cardObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

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
        }
        else
        {
            Debug.Log("Turno do inimigo...");
            Invoke(nameof(EnemyPlay), 1f);
        }
    }

    void EndGame()
    {
        Debug.Log("Fim de jogo!");
    }

    // ===============================
    // 🔹 Nova versão de CheckCaptures com index
    // ===============================
    void CheckCaptures(int index)
    {
        var placedSlot = boardArea.GetChild(index);
        var placedCard = placedSlot.GetChild(0).GetComponent<CardUI>();

        int row = index / 3;
        int col = index % 3;

        if (row > 0) CaptureCheck(placedCard, index - 3, placedCard.cardData.top, "bottom");
        if (row < 2) CaptureCheck(placedCard, index + 3, placedCard.cardData.bottom, "top");
        if (col > 0) CaptureCheck(placedCard, index - 1, placedCard.cardData.left, "right");
        if (col < 2) CaptureCheck(placedCard, index + 1, placedCard.cardData.right, "left");
    }

    void CaptureCheck(CardUI placedCard, int neighborIndex, int placedValue, string neighborSide)
    {
        var neighborSlot = boardArea.GetChild(neighborIndex);
        if (neighborSlot.childCount == 0) return;

        var neighborCard = neighborSlot.GetChild(0).GetComponent<CardUI>();
        if (neighborCard == null || neighborCard.owner == placedCard.owner) return;

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
        }
    }
}
