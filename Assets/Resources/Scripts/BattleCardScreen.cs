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

    public Button exitBattleButton;
    public Button restartBattleButton;

    [Header("Telas")]
    public GameObject battleScreen;
    public GameObject boardScreen;
    public GameObject stealCardsScreen;


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
        // Configurações pós-batalha, se necessário
        boardScreen.SetActive(false);
        stealCardsScreen.SetActive(true);

        if (result == 0)
        {

            CardStealUIManager.Instance.OpenStealScreen(BattleSetupManager.Instance.playerActiveDeck, BattleSetupManager.Instance.enemyActiveDeck, true, false);
        }
        else if (result == 1)
        {
            CardStealUIManager.Instance.OpenStealScreen(BattleSetupManager.Instance.playerActiveDeck, BattleSetupManager.Instance.enemyActiveDeck, false, false);
        }
        else
        {
            battleScreen.SetActive(true);
            ExitBattle();
        }
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
        stealCardsScreen.SetActive(false);
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

   
}
