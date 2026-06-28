using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleCardScreen : MonoBehaviour
{

    [Header("Referências na UI")]
    public Transform playerHandArea;
    public Transform enemyHandArea;
    public Transform boardArea;
    public GameObject roulletPrefab;
    public GameObject cardPrefab;
    public Canvas mainCanvas;
    public GameObject coinPrefab;

    public Button exitBattleButton;
    public Button restartBattleButton;

    [Header("Telas")]
    public GameObject battleScreen;
    public GameObject boardScreen;

    private GameObject currentRoullet;

    [Header("Estado do Jogo")]
    public Owner currentTurn = Owner.None;
    public int filledSlots = 0;
    public static BattleCardScreen Instance;
    
    public void OnScreenOpened()
    {
        Debug.Log("Tela de Batalha de Cartas aberta!");
    }
    

    void Start()
    {
        Instance = this;
        exitBattleButton.onClick.AddListener(ExitBattle);
        restartBattleButton.onClick.AddListener(RestartBattle);
        
        // Botões desativados inicialmente
        exitBattleButton.interactable = false;
        restartBattleButton.interactable = false;
    }

    public void StartBattle()
    {
        
        // 🔹 Verifica se o jogador tem tickets suficientes
        if (!BattleTicketSystem.Instance.ConsumeTicket())
        {
            Dialog.Instance.ShowMessage("You don't have enough tickets to start the game!");
            Debug.LogError("❌ Você não tem tickets suficientes! O jogo não pode iniciar!");
            return;
        } 
        // 🔹 Verifica se o deck do jogador está válido
        if (BattleSetupManager.Instance.playerActiveDeck == null || BattleSetupManager.Instance.playerActiveDeck.Count < 5)
        {
            Dialog.Instance.ShowMessage("Choose your cards before starting the game!");
            Debug.LogError("❌ O jogador não possui 5 cartas definidas no deck. O jogo não pode iniciar!");
            return;
        }
        // 🔹 Verifica se o inimigo tem cartas
        if (BattleSetupManager.Instance.enemyActiveDeck == null || BattleSetupManager.Instance.enemyActiveDeck.Count < 5)
        {
            Dialog.Instance.ShowMessage("Enemy deck is not set! Cannot start the game.");
            Debug.LogError("❌ O deck do inimigo não está definido. O jogo não pode iniciar!");
            return;
        }
        boardScreen.SetActive(true);

        // 🔹 Inicia a distribuição de cartas e depois a roleta
        StartCoroutine(StartBattleSequence());
    }

    private IEnumerator StartBattleSequence()
    {
        // 🔹 Distribuir cartas na mão
        yield return StartCoroutine(CardDealer.Instance.DealCards(
            BattleSetupManager.Instance.playerActiveDeck,
            BattleSetupManager.Instance.enemyActiveDeck,
            playerHandArea,
            enemyHandArea,
            cardPrefab
        ));

        // 🔹 Agora instancia a roleta após as cartas serem distribuídas
        currentRoullet = Instantiate(roulletPrefab, mainCanvas.transform);
        AudioManager.Instance.PlayMusic(AudioManager.Instance.battleMusic);
    }


    public void EnableControlButtons()
    {
        exitBattleButton.interactable = true;
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
    void CallEnemyAI()
    {
        EnemyAI.Instance.PlayTurn();
    }


    public void OnPlayerCardPlaced(CardUI cardUI)
    {
        int index = cardUI.transform.parent.GetSiblingIndex();
        Debug.Log("Jogador jogou: " + cardUI.cardData.cardName + " no slot " + index);
        filledSlots++;
        bool anyCapture = BoardManager.Instance.CheckCaptures(index);
        currentTurn = Owner.Enemy;
        NextTurn();
    }

    public void NextTurn()
    {
        if (filledSlots >= 9)
        {
            BoardManager.Instance.CheckEndGame(); // ✅ agora quem decide é o BoardManager
            return;
        }

        if (currentTurn == Owner.Player)
        {
            SetPlayerHandDraggable(true);
            BoardManager.Instance.UpdateTurnArrow(playerHandArea);
            Debug.Log("Turno do jogador!");
            // jogador vai interagir manualmente
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
        foreach (Transform child in playerHandArea)
        {
            var draggable = child.GetComponent<DraggableCard>(); // seu script de drag
            if (draggable != null)
                draggable.enabled = canDrag;
        }
    }

    public void PosBattleSetup(int result)
    {
        // Configurações pós-batalha
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
        boardScreen.SetActive(false);
        battleScreen.SetActive(true);
        BoardManager.Instance.HideTurnArrow();
        // interrompe qualquer música de batalha
        AudioManager.Instance.StopMusic();
    }

    public void ClearBattleState()
    {
        filledSlots = 0;
        currentTurn = Owner.None;
        BoardManager.Instance.HideTurnArrow();

        foreach (Transform child in playerHandArea) Destroy(child.gameObject);
        foreach (Transform child in enemyHandArea) Destroy(child.gameObject);
        foreach (Transform slot in boardArea)
        {
            foreach (Transform card in slot) Destroy(card.gameObject);
        }
        if (currentRoullet != null)  Destroy(currentRoullet);
       
    }
    private IEnumerator ShowRewardCoins(int rewardCoins)
    {
        if (mainCanvas == null)
        {
            Debug.LogWarning("mainCanvas não encontrado para animar moedas.");
            yield break;
        }

        Canvas canvas = mainCanvas;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 centerPos = Vector2.zero;
        Vector2 topPos = new Vector2(0f, canvasRect.rect.height / 2f + 40f);
        Sprite coinSprite = CreateCoinSprite();

        for (int i = 0; i < rewardCoins; i++)
        {
            if (coinPrefab == null)
            {
                Debug.LogWarning("coinPrefab não foi atribuído. A animação de moedas não será exibida.");
                yield break;
            }

            GameObject coinObject = Instantiate(coinPrefab, canvas.transform, false);
            coinObject.transform.SetAsLastSibling();

            RectTransform coinRect = coinObject.GetComponent<RectTransform>();
            if (coinRect != null)
            {
                coinRect.anchoredPosition = centerPos;
            }

            CanvasGroup canvasGroup = coinObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = coinObject.AddComponent<CanvasGroup>();
            }
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
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

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

                if (distance <= radius)
                {
                    pixels[y * size + x] = new Color(1f, 0.85f, 0.15f, 1f);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

}
