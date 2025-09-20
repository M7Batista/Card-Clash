using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;
    public List<CardData> enemyDeck = new List<CardData>();
    

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Executa o turno do inimigo.
    /// </summary>
    public void PlayTurn()
    {
        var battle = BattleCardScreen.Instance;
        if(enemyDeck.Count == 0)
        {
            enemyDeck = new List<CardData>(battle.enemyActiveDeck);
        }

        CardData bestCard = null;
        Transform bestSlot = null;
        int bestScore = -1;
        CardUI bestCardUI = null;

        // Escolhe a melhor carta e slot
        foreach (var card in enemyDeck)
        {
            foreach (Transform slot in battle.boardArea)
            {
                if (slot.childCount > 0) continue;

                int score = EvaluateMove(battle, card, slot);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCard = card;
                    bestSlot = slot;

                    foreach (Transform c in battle.enemyHandArea)
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
            enemyDeck.Remove(bestCard);

            StartCoroutine(AnimateEnemyCard(bestCardUI, bestSlot, () =>
            {
                int index = bestSlot.GetSiblingIndex();

                // ✅ Corrigido: usa o BoardManager
                bool anyCapture = BoardManager.Instance.CheckCaptures(index);

                Debug.Log($"Inimigo jogou: {bestCard.cardName} no slot {index} (score {bestScore})");

                battle.filledSlots++;
                battle.currentTurn = Owner.Player;
                battle.NextTurn();
            }));
        }
    }

    IEnumerator AnimateEnemyCard(CardUI cardUI, Transform targetSlot, System.Action onComplete)
    {
        var battle = BattleCardScreen.Instance;

        Vector3 startPos = cardUI.transform.position;
        Vector3 endPos = targetSlot.position;

        float duration = 0.5f;
        float elapsed = 0f;

        // mantém no topo da UI
        cardUI.transform.SetParent(battle.boardArea.parent, true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cardUI.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // fixa no slot final
        cardUI.transform.SetParent(targetSlot, false);
        var rect = cardUI.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        onComplete?.Invoke();
    }

    int EvaluateMove(BattleCardScreen battle, CardData card, Transform slot)
    {
        int score = 0;
        int index = slot.GetSiblingIndex();
        int row = index / 3;
        int col = index % 3;

        if (row > 0)
        {
            var neighbor = battle.boardArea.GetChild(index - 3);
            if (neighbor.childCount > 0)
            {
                var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
                if (card.top > neighborCard.bottom) score++;
            }
        }
        if (row < 2)
        {
            var neighbor = battle.boardArea.GetChild(index + 3);
            if (neighbor.childCount > 0)
            {
                var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
                if (card.bottom > neighborCard.top) score++;
            }
        }
        if (col > 0)
        {
            var neighbor = battle.boardArea.GetChild(index - 1);
            if (neighbor.childCount > 0)
            {
                var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
                if (card.left > neighborCard.right) score++;
            }
        }
        if (col < 2)
        {
            var neighbor = battle.boardArea.GetChild(index + 1);
            if (neighbor.childCount > 0)
            {
                var neighborCard = neighbor.GetChild(0).GetComponent<CardUI>().cardData;
                if (card.right > neighborCard.left) score++;
            }
        }

        return score;
    }
}
