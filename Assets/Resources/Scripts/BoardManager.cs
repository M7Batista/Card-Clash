using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Referências")]
    public Transform boardArea;

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

            return true;
        }

        return false;
    }

    // ===============================
    // 🔹 Contagem de cartas no tabuleiro
    // ===============================
    public void GetBoardCounts(out int playerCount, out int enemyCount)
    {
        playerCount = 0;
        enemyCount = 0;

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
    }
}
