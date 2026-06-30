using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // se usar Dropdown padrão
using TMPro;          // se usar TMP_Dropdown


public enum EnemyDifficulty { Easy, Medium, Hard, Advanced }

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;
    public List<CardData> enemyDeckInBattle = new List<CardData>();
    EnemyDifficulty difficulty = EnemyDifficulty.Medium;

    private void Awake()
    {
        Instance = this;
    }

    public void SetDifficultyByStage(int stageNumber)
    {
        if (stageNumber >= 1 && stageNumber <= 25)
        {
            difficulty = EnemyDifficulty.Easy;
        }
        else if (stageNumber >= 26 && stageNumber <= 50)
        {
            difficulty = EnemyDifficulty.Medium;
        }
        else if (stageNumber >= 51 && stageNumber <= 75)
        {
            difficulty = EnemyDifficulty.Hard;
        }
        else if (stageNumber >= 76 && stageNumber <= 100)
        {
            difficulty = EnemyDifficulty.Advanced;
        }
        else
        {
            difficulty = EnemyDifficulty.Medium;
        }

        Debug.Log($"🎚️ Dificuldade da IA definida para estágio {stageNumber}: {difficulty}");
    }

    public void SetDifficultyByRank(string rankName)
    {
        RankInfo? rankInfo = RankSystem.GetRankInfo(rankName);
        string league = rankInfo.HasValue ? rankInfo.Value.league : "Bronze";

        switch (league)
        {
            case "Bronze":
            case "Silver":
                difficulty = EnemyDifficulty.Easy;
                break;
            case "Gold":
            case "Platinum":
            case "Diamond":
                difficulty = EnemyDifficulty.Medium;
                break;
            case "Master":
                difficulty = EnemyDifficulty.Hard;
                break;
            case "Grandmaster":
            case "Legendary":
                difficulty = EnemyDifficulty.Advanced;
                break;
            default:
                difficulty = EnemyDifficulty.Medium;
                break;
        }

        Debug.Log($"🎚️ Dificuldade da IA definida para rank {rankName}: {difficulty}");
    }
    public void SetEnemyDeck(List<CardData> deck)
    {
        enemyDeckInBattle = new List<CardData>(deck);
    }

    public void PlayTurn()
    {
        if (enemyDeckInBattle.Count == 0)
        {
            Debug.LogWarning("⚠️ EnemyAI não tem cartas para jogar!");
            return;
        }

        CardData chosenCard = null;
        Transform chosenSlot = null;
        CardUI chosenUI = null;

        switch (difficulty)
        {
            case EnemyDifficulty.Easy:
                (chosenCard, chosenSlot, chosenUI) = PlayEasy();
                break;
            case EnemyDifficulty.Medium:
                (chosenCard, chosenSlot, chosenUI) = PlayMedium();
                break;
            case EnemyDifficulty.Hard:
                (chosenCard, chosenSlot, chosenUI) = PlayHard();
                break;
            case EnemyDifficulty.Advanced:
                (chosenCard, chosenSlot, chosenUI) = PlayAdvanced();
                break;
        }

        if (chosenCard != null && chosenSlot != null && chosenUI != null)
        {
            enemyDeckInBattle.Remove(chosenCard);
            StartCoroutine(AnimateEnemyCard(chosenUI, chosenSlot, () =>
            {
                int index = chosenSlot.GetSiblingIndex();
                BoardManager.Instance.CheckCaptures(index);
                BattleSetupManager.Instance.filledSlots++;
                BattleSetupManager.Instance.currentTurn = Owner.Player;
                BattleSetupManager.Instance.NextTurn();
            }));
        }
        else
        {
            Debug.LogWarning("⚠️ EnemyAI não encontrou jogada válida!");
            Debug.Log(chosenCard);
            Debug.Log(chosenSlot);
            Debug.Log(chosenUI);
        }
    }

    // 🔹 Fácil: aleatória (mas evita jogadas suicidas)
    private (CardData, Transform, CardUI) PlayEasy()
    {
        List<CardData> validCards = new List<CardData>(enemyDeckInBattle);
        List<Transform> emptySlots = new List<Transform>();

        foreach (Transform slot in BattleSetupManager.Instance.boardArea)
            if (slot.childCount == 0) emptySlots.Add(slot);

        if (validCards.Count == 0 || emptySlots.Count == 0)
            return (null, null, null);

        CardData card = validCards[Random.Range(0, validCards.Count)];
        Transform slotChosen = emptySlots[Random.Range(0, emptySlots.Count)];
        CardUI ui = FindCardUI(card);

        return (card, slotChosen, ui);
    }

    // 🔹 Média: captura imediata > cantos > aleatório
    private (CardData, Transform, CardUI) PlayMedium()
    {
        CardData bestCard = null;
        Transform bestSlot = null;
        int bestCaptures = -1;

        foreach (var card in enemyDeckInBattle)
        {
            foreach (Transform slot in BattleSetupManager.Instance.boardArea)
            {
                if (slot.childCount > 0) continue;

                int captures = EvaluateMove(card, slot);
                if (captures > bestCaptures)
                {
                    bestCaptures = captures;
                    bestCard = card;
                    bestSlot = slot;
                }
            }
        }

        if (bestCard == null)
            return PlayEasy();

        return (bestCard, bestSlot, FindCardUI(bestCard));
    }

    // 🔹 Difícil: captura + defesa (evita se expor)
    private (CardData, Transform, CardUI) PlayHard()
    {
        CardData bestCard = null;
        Transform bestSlot = null;
        int bestScore = -999;

        foreach (var card in enemyDeckInBattle)
        {
            foreach (Transform slot in BattleSetupManager.Instance.boardArea)
            {
                if (slot.childCount > 0) continue;

                int captures = EvaluateMove(card, slot);
                int risk = EvaluateRisk(card, slot);
                int score = captures * 2 - risk;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCard = card;
                    bestSlot = slot;
                }
            }
        }

        if (bestCard == null)
            return PlayEasy();

        return (bestCard, bestSlot, FindCardUI(bestCard));
    }

    // 🔹 Avançada: simulação de 1 turno (simplificada)
    private (CardData, Transform, CardUI) PlayAdvanced()
    {
        return PlayHard();
    }

    // ✅ Funções auxiliares
    private CardUI FindCardUI(CardData card)
    {
        foreach (Transform c in BattleSetupManager.Instance.enemyHandArea)
        {
            var ui = c.GetComponent<CardUI>();
            if (ui != null && ui.cardData == card) return ui;
        }
        return null;
    }

    private int EvaluateMove(CardData card, Transform slot)
    {
        int captures = 0;
        int index = slot.GetSiblingIndex();
        int row = index / 3;
        int col = index % 3;

        if (row > 0)
        {
            var neighbor = BattleSetupManager.Instance.boardArea.GetChild(index - 3);
            if (neighbor.childCount > 0)
            {
                var neighborUI = neighbor.GetChild(0).GetComponent<CardUI>();
                if (neighborUI != null && neighborUI.owner == Owner.Player)
                {
                    if (card.top > neighborUI.cardData.bottom)
                        captures++;
                }
            }
        }

        if (row < 2)
        {
            var neighbor = BattleSetupManager.Instance.boardArea.GetChild(index + 3);
            if (neighbor.childCount > 0)
            {
                var neighborUI = neighbor.GetChild(0).GetComponent<CardUI>();
                if (neighborUI != null && neighborUI.owner == Owner.Player)
                {
                    if (card.bottom > neighborUI.cardData.top)
                        captures++;
                }
            }
        }

        if (col > 0)
        {
            var neighbor = BattleSetupManager.Instance.boardArea.GetChild(index - 1);
            if (neighbor.childCount > 0)
            {
                var neighborUI = neighbor.GetChild(0).GetComponent<CardUI>();
                if (neighborUI != null && neighborUI.owner == Owner.Player)
                {
                    if (card.left > neighborUI.cardData.right)
                        captures++;
                }
            }
        }

        if (col < 2)
        {
            var neighbor = BattleSetupManager.Instance.boardArea.GetChild(index + 1);
            if (neighbor.childCount > 0)
            {
                var neighborUI = neighbor.GetChild(0).GetComponent<CardUI>();
                if (neighborUI != null && neighborUI.owner == Owner.Player)
                {
                    if (card.right > neighborUI.cardData.left)
                        captures++;
                }
            }
        }

        return captures;
    }


    private int EvaluateRisk(CardData card, Transform slot)
    {
        int risk = 0;
        int index = slot.GetSiblingIndex();
        int row = index / 3;
        int col = index % 3;

        if (row > 0)
        {
            var neighbor = BattleSetupManager.Instance.boardArea.GetChild(index - 3);
            if (neighbor.childCount == 0)
            {
                if (PlayerHasCounter(card.top, Side.Bottom))
                    risk++;
            }
        }

        if (row < 2)
        {
            var neighbor = BattleSetupManager.Instance.boardArea.GetChild(index + 3);
            if (neighbor.childCount == 0)
            {
                if (PlayerHasCounter(card.bottom, Side.Top))
                    risk++;
            }
        }

        if (col > 0)
        {
            var neighbor = BattleSetupManager.Instance.boardArea.GetChild(index - 1);
            if (neighbor.childCount == 0)
            {
                if (PlayerHasCounter(card.left, Side.Right))
                    risk++;
            }
        }

        if (col < 2)
        {
            var neighbor = BattleSetupManager.Instance.boardArea.GetChild(index + 1);
            if (neighbor.childCount == 0)
            {
                if (PlayerHasCounter(card.right, Side.Left))
                    risk++;
            }
        }

        return risk;
    }

    private bool PlayerHasCounter(int enemyValue, Side side)
    {
        var playerHand = BattleSetupManager.Instance.playerHandArea;
        foreach (Transform cardObj in playerHand)
        {
            var ui = cardObj.GetComponent<CardUI>();
            if (ui == null) continue;
            var card = ui.cardData;

            switch (side)
            {
                case Side.Top: if (card.top > enemyValue) return true; break;
                case Side.Bottom: if (card.bottom > enemyValue) return true; break;
                case Side.Left: if (card.left > enemyValue) return true; break;
                case Side.Right: if (card.right > enemyValue) return true; break;
            }
        }
        return false;
    }

    public enum Side { Top, Bottom, Left, Right }


    // ✅ Animação de jogar a carta

    IEnumerator AnimateEnemyCard(CardUI cardUI, Transform targetSlot, System.Action onComplete)
    {
        Vector3 startPos = cardUI.transform.position;
        Vector3 endPos = targetSlot.position;

        float duration = 0.5f;
        float elapsed = 0f;

        // mantém no topo da UI
        if (BattleSetupManager.Instance != null && BattleSetupManager.Instance.boardArea != null)
            cardUI.transform.SetParent(BattleSetupManager.Instance.boardArea.parent, true);
        else
            cardUI.transform.SetParent(targetSlot.parent, true);

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
        AudioManager.Instance?.PlaySFX("card-slide-1");
        onComplete?.Invoke();
    }


}
