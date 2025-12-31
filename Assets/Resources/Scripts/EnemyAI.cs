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
    public EnemyDifficulty difficulty = EnemyDifficulty.Hard; // 🔹 Configurável no Inspetor


    
    private void OnDifficultyChanged(int index)
    {
        difficulty = (EnemyDifficulty)index;
        Debug.Log($"🎚️ Dificuldade da IA alterada para: {difficulty}");
    }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Define a dificuldade da IA baseada no número do estágio (1-100)
    /// </summary>
    public void SetDifficultyByStage(int stageNumber)
    {
        EnemyDifficulty baseDifficulty;

        if (stageNumber >= 1 && stageNumber <= 25)
        {
            baseDifficulty = EnemyDifficulty.Easy;
        }
        else if (stageNumber >= 26 && stageNumber <= 50)
        {
            baseDifficulty = EnemyDifficulty.Medium;
        }
        else if (stageNumber >= 51 && stageNumber <= 75)
        {
            baseDifficulty = EnemyDifficulty.Hard;
        }
        else if (stageNumber >= 76 && stageNumber <= 100)
        {
            baseDifficulty = EnemyDifficulty.Advanced;
        }
        else
        {
            // Fallback para Hard se fora do range
            baseDifficulty = EnemyDifficulty.Hard;
        }

        // Para estágios chefes (múltiplos de 10), aumentar a dificuldade
        if (stageNumber % 10 == 0)
        {
            switch (baseDifficulty)
            {
                case EnemyDifficulty.Easy:
                    difficulty = EnemyDifficulty.Medium;
                    break;
                case EnemyDifficulty.Medium:
                    difficulty = EnemyDifficulty.Hard;
                    break;
                case EnemyDifficulty.Hard:
                    difficulty = EnemyDifficulty.Advanced;
                    break;
                case EnemyDifficulty.Advanced:
                    difficulty = EnemyDifficulty.Advanced; // Já é o máximo
                    break;
            }
        }
        else
        {
            difficulty = baseDifficulty;
        }

        Debug.Log($"🎚️ Dificuldade da IA definida para estágio {stageNumber}: {difficulty}");
    }

    public void SetEnemyDeck(List<CardData> deck)
    {
        enemyDeckInBattle = new List<CardData>(deck);
    }

    public void PlayTurn()
    {
        var battle = BattleCardScreen.Instance;

        if (enemyDeckInBattle.Count == 0)
        {
            Debug.LogWarning("⚠️ EnemyAI não tem cartas para jogar!");

        }

        CardData chosenCard = null;
        Transform chosenSlot = null;
        CardUI chosenUI = null;

        switch (difficulty)
        {
            case EnemyDifficulty.Easy:
                (chosenCard, chosenSlot, chosenUI) = PlayEasy(battle);
                break;
            case EnemyDifficulty.Medium:
                (chosenCard, chosenSlot, chosenUI) = PlayMedium(battle);
                break;
            case EnemyDifficulty.Hard:
                (chosenCard, chosenSlot, chosenUI) = PlayHard(battle);
                break;
            case EnemyDifficulty.Advanced:
                (chosenCard, chosenSlot, chosenUI) = PlayAdvanced(battle);
                break;
        }

        if (chosenCard != null && chosenSlot != null && chosenUI != null)
        {
            enemyDeckInBattle.Remove(chosenCard);
            StartCoroutine(AnimateEnemyCard(chosenUI, chosenSlot, () =>
            {
                int index = chosenSlot.GetSiblingIndex();
                BoardManager.Instance.CheckCaptures(index);
                battle.filledSlots++;
                battle.currentTurn = Owner.Player;
                battle.NextTurn();
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
    private (CardData, Transform, CardUI) PlayEasy(BattleCardScreen battle)
    {
        List<CardData> validCards = new List<CardData>(enemyDeckInBattle);
        List<Transform> emptySlots = new List<Transform>();

        foreach (Transform slot in battle.boardArea)
            if (slot.childCount == 0) emptySlots.Add(slot);

        if (validCards.Count == 0 || emptySlots.Count == 0)
            return (null, null, null);

        CardData card = validCards[Random.Range(0, validCards.Count)];
        Transform slotChosen = emptySlots[Random.Range(0, emptySlots.Count)];
        CardUI ui = FindCardUI(battle, card);

        return (card, slotChosen, ui);
    }

    // 🔹 Média: captura imediata > cantos > aleatório
    private (CardData, Transform, CardUI) PlayMedium(BattleCardScreen battle)
    {
        CardData bestCard = null;
        Transform bestSlot = null;
        int bestCaptures = -1;

        foreach (var card in enemyDeckInBattle)
        {
            foreach (Transform slot in battle.boardArea)
            {
                if (slot.childCount > 0) continue;

                int captures = EvaluateMove(battle, card, slot);
                if (captures > bestCaptures)
                {
                    bestCaptures = captures;
                    bestCard = card;
                    bestSlot = slot;
                }
            }
        }

        if (bestCard == null)
            return PlayEasy(battle); // fallback

        return (bestCard, bestSlot, FindCardUI(battle, bestCard));
    }

    // 🔹 Difícil: captura + defesa (evita se expor)
    private (CardData, Transform, CardUI) PlayHard(BattleCardScreen battle)
    {
        CardData bestCard = null;
        Transform bestSlot = null;
        int bestScore = -999;

        foreach (var card in enemyDeckInBattle)
        {
            foreach (Transform slot in battle.boardArea)
            {
                if (slot.childCount > 0) continue;

                int captures = EvaluateMove(battle, card, slot);
                int risk = EvaluateRisk(battle, card, slot);
                int score = captures * 2 - risk; // captura vale mais, mas risco pesa

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCard = card;
                    bestSlot = slot;
                }
            }
        }

        if (bestCard == null)
            return PlayEasy(battle);

        return (bestCard, bestSlot, FindCardUI(battle, bestCard));
    }

    // 🔹 Avançada: simulação de 1 turno (simplificada)
    private (CardData, Transform, CardUI) PlayAdvanced(BattleCardScreen battle)
    {
        // Aqui você pode expandir com "mini-minimax" de 1-2 turnos
        // Por enquanto só chamei o Hard como placeholder
        return PlayHard(battle);
    }

    // ✅ Funções auxiliares
    private CardUI FindCardUI(BattleCardScreen battle, CardData card)
    {
        foreach (Transform c in battle.enemyHandArea)
        {
            var ui = c.GetComponent<CardUI>();
            if (ui != null && ui.cardData == card) return ui;
        }
        return null;
    }

    private int EvaluateMove(BattleCardScreen battle, CardData card, Transform slot)
    {
        int captures = 0;
        int index = slot.GetSiblingIndex();
        int row = index / 3;
        int col = index % 3;

        // Cima
        if (row > 0)
        {
            var neighbor = battle.boardArea.GetChild(index - 3);
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

        // Baixo
        if (row < 2)
        {
            var neighbor = battle.boardArea.GetChild(index + 3);
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

        // Esquerda
        if (col > 0)
        {
            var neighbor = battle.boardArea.GetChild(index - 1);
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

        // Direita
        if (col < 2)
        {
            var neighbor = battle.boardArea.GetChild(index + 1);
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


    private int EvaluateRisk(BattleCardScreen battle, CardData card, Transform slot)
    {
        int risk = 0;
        int index = slot.GetSiblingIndex();
        int row = index / 3;
        int col = index % 3;

        // Cima
        if (row > 0)
        {
            var neighbor = battle.boardArea.GetChild(index - 3);
            if (neighbor.childCount == 0)
            {
                // Jogador pode colocar carta aqui no próximo turno
                // Se existir alguma carta com "bottom > card.top", risco++
                if (PlayerHasCounter(card.top, Side.Bottom))
                    risk++;
            }
        }

        // Baixo
        if (row < 2)
        {
            var neighbor = battle.boardArea.GetChild(index + 3);
            if (neighbor.childCount == 0)
            {
                if (PlayerHasCounter(card.bottom, Side.Top))
                    risk++;
            }
        }

        // Esquerda
        if (col > 0)
        {
            var neighbor = battle.boardArea.GetChild(index - 1);
            if (neighbor.childCount == 0)
            {
                if (PlayerHasCounter(card.left, Side.Right))
                    risk++;
            }
        }

        // Direita
        if (col < 2)
        {
            var neighbor = battle.boardArea.GetChild(index + 1);
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
        var playerHand = BattleCardScreen.Instance.playerHandArea;
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
        AudioManager.Instance?.PlaySFX("card-slide-1");
        onComplete?.Invoke();
    }


}
