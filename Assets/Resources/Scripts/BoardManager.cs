using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Necessário para o Text
using TMPro; // Necessário para TextMeshProUGUI

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Referências")]
    public Transform boardArea;
    public Transform playerHandArea;
    public Transform enemyHandArea;

    [Header("UI")]
    public TextMeshProUGUI playerCountText;
    public TextMeshProUGUI enemyCountText;

    private void Awake()
    {
        Instance = this;
    }

    // ===============================
    // 🔹 Capturas
    // ===============================
    public bool CheckCaptures(int index)
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

        UpdateBoardCounts(); // ✅ sempre atualiza após possíveis capturas
        return anyCapture;
    }

    private bool CaptureCheck(CardUI placedCard, int neighborIndex, int placedValue, string neighborSide)
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

            // ✅ Atualiza pontos só quando houve captura
            UpdateBoardCounts();

            return true;
        }

        return false;
    }


    // ===============================
    // 🔹 Contagem de pontos (board + mãos)
    // ===============================
    public void GetBoardCounts(out int playerCount, out int enemyCount)
    {
        playerCount = 0;
        enemyCount = 0;

        // Contagem no TABULEIRO
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

        // Contagem na MÃO do JOGADOR
        for (int i = 0; i < playerHandArea.childCount; i++)
        {
            var cardUI = playerHandArea.GetChild(i).GetComponent<CardUI>();
            if (cardUI != null && cardUI.owner == Owner.Player) playerCount++;
        }

        // Contagem na MÃO do INIMIGO
        for (int i = 0; i < enemyHandArea.childCount; i++)
        {
            var cardUI = enemyHandArea.GetChild(i).GetComponent<CardUI>();
            if (cardUI != null && cardUI.owner == Owner.Enemy) enemyCount++;
        }
    }

    // ===============================
    // 🔹 Atualiza UI
    // ===============================
    public void UpdateBoardCounts()
    {
        GetBoardCounts(out int playerCount, out int enemyCount);

        if (playerCountText != null)
            playerCountText.text = playerCount.ToString();

        if (enemyCountText != null)
            enemyCountText.text = enemyCount.ToString();
    }

    // ===============================
    // 🔹 Fim de jogo
    // ===============================
    public void CheckEndGame()
    {
        GetBoardCounts(out int playerCount, out int enemyCount);

        if (playerCount > enemyCount)
        {
            Debug.Log($"Fim de jogo! Jogador venceu ({playerCount} x {enemyCount})");
            BattleCardScreen.Instance.StartCoroutine(ShowPanelEndGame(0));
        }
        else if (enemyCount > playerCount)
        {
            Debug.Log($"Fim de jogo! Inimigo venceu ({enemyCount} x {playerCount})");
            BattleCardScreen.Instance.StartCoroutine(ShowPanelEndGame(1));
        }
        else
        {
            Debug.Log($"Fim de jogo! Empate ({playerCount} x {enemyCount})");
            BattleCardScreen.Instance.StartCoroutine(ShowPanelEndGame(2));
        }
    }

    private IEnumerator ShowPanelEndGame(int result)
    {
        yield return new WaitForSeconds(2f);


        EndGameUI.instance.ShowEndGame(result);

    }
}
