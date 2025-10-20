using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public GameObject turnArrow;

    [Header("🔹 Regras Especiais (ativar no Inspector)")]
    public bool ruleSame = true;  // Regra "MESMO"
    public bool rulePlus = true;  // Regra "MAIS"
    public TextMeshProUGUI specialRuleText;   // Texto central exibido no meio do tabuleiro
    public AudioClip specialRuleSFX;          // Som tocado quando a regra ativa
    public float ruleTextDuration = 1.5f;     // Duração do fade-out do texto

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

        Dictionary<int, (CardUI card, int placedValue, int neighborValue)> adjacents = new();

        if (row > 0) AddAdjacent(index - 3, placedCard.cardData.top, "bottom");
        if (row < 2) AddAdjacent(index + 3, placedCard.cardData.bottom, "top");
        if (col > 0) AddAdjacent(index - 1, placedCard.cardData.left, "right");
        if (col < 2) AddAdjacent(index + 1, placedCard.cardData.right, "left");

        void AddAdjacent(int neighborIndex, int placedValue, string neighborSide)
        {
            var neighborSlot = boardArea.GetChild(neighborIndex);
            if (neighborSlot.childCount == 0) return;
            var neighborCard = neighborSlot.GetChild(0).GetComponent<CardUI>();
            if (neighborCard == null) return;

            int neighborValue = neighborSide switch
            {
                "top" => neighborCard.cardData.top,
                "bottom" => neighborCard.cardData.bottom,
                "left" => neighborCard.cardData.left,
                "right" => neighborCard.cardData.right,
                _ => 0
            };

            adjacents[neighborIndex] = (neighborCard, placedValue, neighborValue);
        }

        // 🔹 Regras Especiais
        if (ruleSame)
            anyCapture |= CheckRuleSame(placedCard, adjacents);

        if (rulePlus)
            anyCapture |= CheckRulePlus(placedCard, adjacents);

        // 🔹 Regra padrão (maior valor)
        foreach (var kvp in adjacents)
        {
            int neighborIndex = kvp.Key;
            var (neighborCard, placedValue, neighborValue) = kvp.Value;

            if (neighborCard.owner != placedCard.owner && placedValue > neighborValue)
            {
                neighborCard.SetOwner(placedCard.owner);
                var flip = neighborCard.GetComponent<CardFlip>();
                if (flip != null) flip.FlipCard(placedCard.owner, placedCard);
                anyCapture = true;
            }
        }

        if (anyCapture) UpdateBoardCounts();
        return anyCapture;
    }

    // ===============================
    // 🔸 Regra MESMO
    // ===============================
    private bool CheckRuleSame(CardUI placedCard, Dictionary<int, (CardUI card, int placedValue, int neighborValue)> adj)
    {
        List<int> sameMatches = new();

        foreach (var kvp in adj)
        {
            int idx = kvp.Key;
            var (card, placed, neighbor) = kvp.Value;

            if (card.owner != placedCard.owner && placed == neighbor)
                sameMatches.Add(idx);
        }

        if (sameMatches.Count >= 2)
        {
            ShowSpecialRuleText("SAME");
            foreach (int idx in sameMatches)
            {
                var card = adj[idx].card;
                card.SetOwner(placedCard.owner);

                var flip = card.GetComponent<CardFlip>();
                if (flip != null) flip.FlipCard(placedCard.owner, placedCard);
            }

            Debug.Log($"[MESMO] {placedCard.owner} capturou {sameMatches.Count} cartas!");
            return true;
        }

        return false;
    }

    // ===============================
    // 🔸 Regra MAIS
    // ===============================
    private bool CheckRulePlus(CardUI placedCard, Dictionary<int, (CardUI card, int placedValue, int neighborValue)> adj)
    {
        List<int> plusMatches = new();

        // Calcula todas as somas
        Dictionary<int, int> sums = new();
        foreach (var kvp in adj)
        {
            int idx = kvp.Key;
            var (card, placed, neighbor) = kvp.Value;
            sums[idx] = placed + neighbor;
        }

        // Verifica se há somas iguais
        foreach (var a in sums)
        {
            foreach (var b in sums)
            {
                if (a.Key == b.Key) continue;
                if (a.Value == b.Value)
                {
                    if (!plusMatches.Contains(a.Key)) plusMatches.Add(a.Key);
                    if (!plusMatches.Contains(b.Key)) plusMatches.Add(b.Key);
                }
            }
        }

        if (plusMatches.Count >= 2)
        {
            ShowSpecialRuleText("PLUS");
            foreach (int idx in plusMatches)
            {
                var card = adj[idx].card;
                if (card.owner != placedCard.owner)
                {
                    card.SetOwner(placedCard.owner);
                    var flip = card.GetComponent<CardFlip>();
                    if (flip != null) flip.FlipCard(placedCard.owner, placedCard);
                }
            }

            Debug.Log($"[MAIS] {placedCard.owner} capturou {plusMatches.Count} cartas!");
            return true;
        }

        return false;
    }

    // ===============================
    // 🔹 Contagem, UI e Fim de Jogo (sem alterações)
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

        for (int i = 0; i < playerHandArea.childCount; i++)
        {
            var cardUI = playerHandArea.GetChild(i).GetComponent<CardUI>();
            if (cardUI != null && cardUI.owner == Owner.Player) playerCount++;
        }

        for (int i = 0; i < enemyHandArea.childCount; i++)
        {
            var cardUI = enemyHandArea.GetChild(i).GetComponent<CardUI>();
            if (cardUI != null && cardUI.owner == Owner.Enemy) enemyCount++;
        }
    }

    public void UpdateBoardCounts()
    {
        GetBoardCounts(out int playerCount, out int enemyCount);

        if (playerCountText != null)
            playerCountText.text = playerCount.ToString();

        if (enemyCountText != null)
            enemyCountText.text = enemyCount.ToString();
    }

    public void CheckEndGame()
    {
        GetBoardCounts(out int playerCount, out int enemyCount);
        turnArrow.SetActive(false);
        if (playerCount > enemyCount)
        {
            StageMapGenerator.Instance.UnlockNextStage();
            Debug.Log($"Fim de jogo! Jogador venceu ({playerCount} x {enemyCount})");
            StartCoroutine(ShowPanelEndGame(0));
        }
        else if (enemyCount > playerCount)
        {
            Debug.Log($"Fim de jogo! Inimigo venceu ({enemyCount} x {playerCount})");
            StartCoroutine(ShowPanelEndGame(1));
        }
        else
        {
            Debug.Log($"Fim de jogo! Empate ({playerCount} x {enemyCount})");
            StartCoroutine(ShowPanelEndGame(2));
        }
    }

    private IEnumerator ShowPanelEndGame(int result)
    {
        yield return new WaitForSeconds(1.5f);
        BattleResultScreen.instance.ShowEndGame(result);
    }

    public void UpdateTurnArrow(Transform handArea)
    {
        if (turnArrow == null || handArea == null) return;

        turnArrow.SetActive(true);
        RectTransform rt = turnArrow.GetComponent<RectTransform>();
        RectTransform handRT = handArea.GetComponent<RectTransform>();

        Vector3 handPos = handRT.position;
        Vector3 arrowPos = rt.position;
        arrowPos.y = handPos.y;
        arrowPos.x = Screen.width - 80f;
        rt.position = arrowPos;
    }

    public void HideTurnArrow() => turnArrow.SetActive(false);



    private void ShowSpecialRuleText(string ruleName)
    {
        if (specialRuleText == null) return;

        specialRuleText.text = ruleName;
        specialRuleText.alpha = 1f;
        specialRuleText.gameObject.SetActive(true);

        if (specialRuleSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("specialRuleSFX");

        StopAllCoroutines(); // evita sobreposição
        StartCoroutine(FadeRuleText());
    }
    private IEnumerator FadeRuleText()
    {
        yield return new WaitForSeconds(0.4f);

        float t = 0;
        Color c = specialRuleText.color;

        while (t < ruleTextDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / ruleTextDuration);
            specialRuleText.color = c;
            yield return null;
        }

        specialRuleText.gameObject.SetActive(false);
    }


}
