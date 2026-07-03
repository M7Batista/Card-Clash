using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSetupManager : MonoBehaviour
{
    public static BattleSetupManager Instance { get; private set; }

    [Header("Decks")]
    public List<CardData> playerActiveDeck = new List<CardData>();
    public List<CardData> enemyActiveDeck = new List<CardData>();
    public bool ruleSame = true;
    public bool rulePlus = true;

    [Header("Battle UI")]
    public Transform playerHandArea;
    public Transform enemyHandArea;
    public Transform boardArea;
    public GameObject roulletPrefab;
    public GameObject cardPrefab;
    public Canvas mainCanvas;
    public GameObject coinPrefab;
    public Button exitBattleButton;
    public Button restartBattleButton;
    public GameObject battleScreen;
    public GameObject boardScreen;

    [Header("Battle State")]
    public Owner currentTurn = Owner.None;
    public int filledSlots = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        if (BattleCardScreen.Instance != null)
            InitializeBattleScreenReferences(BattleCardScreen.Instance);
    }

    public void InitializeBattleScreenReferences(BattleCardScreen battleScreen)
    {
        if (battleScreen == null) return;

        playerHandArea = battleScreen.playerHandArea;
        enemyHandArea = battleScreen.enemyHandArea;
        boardArea = battleScreen.boardArea;
        roulletPrefab = battleScreen.roulletPrefab;
        cardPrefab = battleScreen.cardPrefab;
        mainCanvas = battleScreen.mainCanvas;
        coinPrefab = battleScreen.coinPrefab;
        exitBattleButton = battleScreen.exitBattleButton;
        restartBattleButton = battleScreen.restartBattleButton;
        this.battleScreen = battleScreen.battleScreen;
        this.boardScreen = battleScreen.boardScreen;

        if (exitBattleButton != null)
        {
            exitBattleButton.onClick.RemoveAllListeners();
            exitBattleButton.onClick.AddListener(ExitBattle);
            exitBattleButton.interactable = false;
        }

        if (restartBattleButton != null)
        {
            restartBattleButton.onClick.RemoveAllListeners();
            restartBattleButton.onClick.AddListener(RestartBattle);
            restartBattleButton.interactable = false;
        }
    }

    public void SetPlayerActiveDeck()
    {
        List<int> playerDeckIds = PlayerDeckManager.LoadDeck();
        playerActiveDeck.Clear();
        foreach (int id in playerDeckIds)
        {
            CardData card = PlayerDeckManager.GetCardById(id);
            if (card != null)
                playerActiveDeck.Add(card);
        }
    }

    public void SetEnemyActiveDeck(string rankName)
    {
        enemyActiveDeck.Clear();
        enemyActiveDeck = EnemyDeckManager.Instance.GenerateEnemyDeck(rankName);
    }

    public void StartBattle()
    {
        if (!BattleTicketSystem.Instance.ConsumeTicket())
        {
            Dialog.Instance.ShowMessage("You don't have enough tickets to start the game!");
            Debug.LogError("❌ Você não tem tickets suficientes! O jogo não pode iniciar!");
            return;
        }

        if (playerActiveDeck == null || playerActiveDeck.Count < 5)
        {
            Dialog.Instance.ShowMessage("Choose your cards before starting the game!");
            Debug.LogError("❌ O jogador não possui 5 cartas definidas no deck. O jogo não pode iniciar!");
            return;
        }

        if (enemyActiveDeck == null || enemyActiveDeck.Count < 5)
        {
            Dialog.Instance.ShowMessage("Enemy deck is not set! Cannot start the game.");
            Debug.LogError("❌ O deck do inimigo não está definido. O jogo não pode iniciar!");
            return;
        }

        if (boardScreen != null)
            boardScreen.SetActive(true);

        if (!ValidateBattleSetup())
            return;

        StartCoroutine(StartBattleSequence());
    }

    private bool ValidateBattleSetup()
    {
        if (CardDealer.Instance == null)
        {
            Debug.LogError("BattleSetupManager: CardDealer.Instance is null. Please ensure a CardDealer object exists in the scene.");
            return false;
        }

        if (playerHandArea == null)
        {
            Debug.LogError("BattleSetupManager: playerHandArea is not assigned.");
            return false;
        }

        if (enemyHandArea == null)
        {
            Debug.LogError("BattleSetupManager: enemyHandArea is not assigned.");
            return false;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("BattleSetupManager: cardPrefab is not assigned.");
            return false;
        }

        return true;
    }

    private IEnumerator StartBattleSequence()
    {
        if (CardDealer.Instance == null)
        {
            Debug.LogError("BattleSetupManager: cannot start battle sequence because CardDealer.Instance is null.");
            yield break;
        }

        yield return StartCoroutine(CardDealer.Instance.DealCards(
            playerActiveDeck,
            enemyActiveDeck,
            playerHandArea,
            enemyHandArea,
            cardPrefab
        ));

        if (roulletPrefab != null && mainCanvas != null)
            Instantiate(roulletPrefab, mainCanvas.transform);

        AudioManager.Instance.PlayMusic(AudioManager.Instance.battleMusic);
    }

    public void EnableControlButtons()
    {
        if (exitBattleButton != null)
            exitBattleButton.interactable = true;
        if (restartBattleButton != null)
            restartBattleButton.interactable = true;
    }

    public void StartPlayerTurn()
    {
        currentTurn = Owner.Player;
        SetPlayerHandDraggable(true);
        BoardManager.Instance.UpdateTurnArrow(playerHandArea);
    }

    public void StartEnemyTurn()
    {
        currentTurn = Owner.Enemy;
        SetPlayerHandDraggable(false);
        BoardManager.Instance.UpdateTurnArrow(enemyHandArea);
        Invoke(nameof(CallEnemyAI), 2f);
    }

    private void CallEnemyAI()
    {
        EnemyAI.Instance.PlayTurn();
    }

    public void OnPlayerCardPlaced(CardUI cardUI)
    {
        int index = cardUI.transform.parent.GetSiblingIndex();
        Debug.Log("Jogador jogou: " + cardUI.cardData.cardName + " no slot " + index);
        filledSlots++;
        BoardManager.Instance.CheckCaptures(index);
        currentTurn = Owner.Enemy;
        NextTurn();
    }

    public void NextTurn()
    {
        if (filledSlots >= 9)
        {
            BoardManager.Instance.CheckEndGame();
            return;
        }

        if (currentTurn == Owner.Player)
        {
            SetPlayerHandDraggable(true);
            BoardManager.Instance.UpdateTurnArrow(playerHandArea);
            Debug.Log("Turno do jogador!");
        }
        else if (currentTurn == Owner.Enemy)
        {
            SetPlayerHandDraggable(false);
            BoardManager.Instance.UpdateTurnArrow(enemyHandArea);
            Debug.Log("Turno do inimigo!");
            Invoke(nameof(CallEnemyAI), 1f);
        }
    }

    private void SetPlayerHandDraggable(bool canDrag)
    {
        if (playerHandArea == null)
            return;

        foreach (Transform child in playerHandArea)
        {
            var draggable = child.GetComponent<DraggableCard>();
            if (draggable != null)
                draggable.enabled = canDrag;
        }
    }

    public void PosBattleSetup(int result)
    {
        if (boardScreen != null)
            boardScreen.SetActive(false);

        GrantBattleReward(result);
        ExitBattle();
    }

    private void GrantBattleReward(int result)
    {
        int rewardCoins = result switch
        {
            0 => 20,
            2 => 10,
            _ => 0
        };

        ExitBattle();

        // Adiciona vitória ao contador de rank se ganhou
        if (result == 0)
        {
            GameManager.Instance.AddRankWin();
            Debug.Log($"Vitória contabilizada! Total: {GameManager.Instance.GetRankWins()}");
            // Em qualquer classe que tenha acesso ao BattleScreen
            //BattleScreen battleScreen = GetComponent<BattleScreen>();
            //battleScreen.UpdateStarsDisplay();
        }

        if (rewardCoins > 0)
        {
            GameManager.Instance.AddCoins(rewardCoins);
            StartCoroutine(ShowRewardCoins(rewardCoins));
            Debug.Log($"Recompensa de batalha concedida: {rewardCoins} moedas ({GetResultLabel(result)})");
        }
        else
        {
            Debug.Log($"Nenhuma recompensa de moedas para o resultado: {GetResultLabel(result)}");
        }
    }

    private string GetResultLabel(int result)
    {
        return result switch
        {
            0 => "vitória",
            1 => "derrota",
            2 => "empate",
            _ => "resultado desconhecido"
        };
    }

    public void RestartBattle()
    {
        ClearBattleState();
        StartBattle();
    }

    public void ExitBattle()
    {
        ClearBattleState();
        if (boardScreen != null)
            boardScreen.SetActive(false);
        if (battleScreen != null)
        {
            battleScreen.SetActive(true);
            RefreshRankUI();
        }
        BoardManager.Instance.HideTurnArrow();
        AudioManager.Instance.StopMusic();
    }

    private void RefreshRankUI()
    {
        if (battleScreen == null)
            return;

        BattleScreen screen = battleScreen.GetComponent<BattleScreen>();
        if (screen == null)
            return;

        screen.UpdateStarsDisplay();
        screen.UpdateRankDisplay();
        screen.UpdateRankNameDisplay();
    }

    public void ClearBattleState()
    {
        filledSlots = 0;
        currentTurn = Owner.None;
        BoardManager.Instance.HideTurnArrow();

        if (playerHandArea != null)
        {
            foreach (Transform child in playerHandArea)
                Destroy(child.gameObject);
        }

        if (enemyHandArea != null)
        {
            foreach (Transform child in enemyHandArea)
                Destroy(child.gameObject);
        }

        if (boardArea != null)
        {
            foreach (Transform slot in boardArea)
            {
                foreach (Transform card in slot)
                    Destroy(card.gameObject);
            }
        }
    }

    private IEnumerator ShowRewardCoins(int rewardCoins)
    {
        if (mainCanvas == null)
        {
            Debug.LogWarning("mainCanvas não encontrado para animar moedas.");
            yield break;
        }

        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        Vector2 centerPos = Vector2.zero;
        Vector2 topPos = new Vector2(0f, canvasRect.rect.height / 2f + 40f);

        for (int i = 0; i < rewardCoins; i++)
        {
            if (coinPrefab == null)
            {
                Debug.LogWarning("coinPrefab não foi atribuído. A animação de moedas não será exibida.");
                yield break;
            }

            GameObject coinObject = Instantiate(coinPrefab, mainCanvas.transform, false);
            coinObject.transform.SetAsLastSibling();

            RectTransform coinRect = coinObject.GetComponent<RectTransform>();
            if (coinRect != null)
                coinRect.anchoredPosition = centerPos;

            CanvasGroup canvasGroup = coinObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = coinObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            Vector2 spreadTarget = centerPos + new Vector2(Random.Range(-40f, 40f), Random.Range(20f, 60f));
            StartCoroutine(AnimateSingleCoin(coinRect, centerPos, spreadTarget, topPos, 0.9f));

            yield return new WaitForSeconds(0.03f);
        }
    }

    private IEnumerator AnimateSingleCoin(RectTransform coinRect, Vector2 startPos, Vector2 spreadTarget, Vector2 finalTarget, float duration)
    {
        float spreadDuration = duration * 0.3f;
        float elapsed = 0f;

        while (elapsed < spreadDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spreadDuration;
            float easeOut = 1f - (1f - t) * (1f - t);
            coinRect.anchoredPosition = Vector2.Lerp(startPos, spreadTarget, easeOut);
            yield return null;
        }

        float flyDuration = duration * 0.7f;
        elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;
            float easeIn = t * t;
            coinRect.anchoredPosition = Vector2.Lerp(spreadTarget, finalTarget, easeIn);

            CanvasGroup canvasGroup = coinRect.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        Destroy(coinRect.gameObject);
    }

    private Sprite CreateCoinSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 center = new Vector2(size / 2f, size / 2f);
                float dx = x - center.x;
                float dy = y - center.y;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float radius = size * 0.45f;

                pixels[y * size + x] = distance <= radius ? new Color(1f, 0.85f, 0.15f, 1f) : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
}
