using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrepareBattleScreen : MonoBehaviour
{

    public TextMeshProUGUI stageText;
    public TextMeshProUGUI enemyPowerText;
    public TextMeshProUGUI playerPowerText;
    public Button ButtonStartBattle;
    public Button ButtonReturn;
    public Button ButtonEditDeck;
    public Transform playerHandArea;
    public Transform enemyHandArea;
    public GameObject cardPrefab;
    public GameObject EditDeckScreen;
    private bool buttonsInitialized = false;

    void OnEnable()
    {
        Debug.Log("PrepareBattleScreen Opened");
        int stage = PlayerPrefs.GetInt("UnlockedStage", 1);
        
        if (!buttonsInitialized)
        {
            InitializeButtons();
            buttonsInitialized = true;
        }
        
        PrepareBattle(stage);
    }

    void InitializeButtons()
    {
        ButtonStartBattle.onClick.RemoveAllListeners();
        ButtonStartBattle.onClick.AddListener(() =>
        {
            BattleCardScreen.Instance.StartBattle();
            this.gameObject.SetActive(false);
        });

        ButtonReturn.onClick.RemoveAllListeners();
        ButtonReturn.onClick.AddListener(() =>
        {
            this.gameObject.SetActive(false);
        });

        ButtonEditDeck.onClick.RemoveAllListeners();
        ButtonEditDeck.onClick.AddListener(() =>
        {
            this.gameObject.SetActive(false);
            EditDeckScreen.SetActive(true);
        });
    }
    public void PrepareBattle(int currentStage)
    {
        BattleSetupManager.Instance.SetPlayerActiveDeck();
        BattleSetupManager.Instance.SetEnemyActiveDeck(currentStage);
        
        List<CardData> playerActiveDeck = BattleSetupManager.Instance.playerActiveDeck;
        List<CardData> enemyActiveDeck = BattleSetupManager.Instance.enemyActiveDeck;
        EnemyAI.Instance.SetEnemyDeck(enemyActiveDeck);
        EnemyAI.Instance.SetDifficultyByStage(currentStage);
        // Calcula o poder do inimigo
        int enemyPower = 0;
        foreach (var card in enemyActiveDeck)
        {
            enemyPower += (card.top + card.bottom + card.left + card.right);
        }
        enemyPowerText.text = enemyPower.ToString();
        // Calcula o poder do jogador
        int playerPower = 0;
        foreach (var card in playerActiveDeck)
        {
            playerPower += (card.top + card.bottom + card.left + card.right);
        }
        playerPowerText.text = playerPower.ToString();
        stageText.text = "Stage " + currentStage;

        InstantiateCardsInHand(playerHandArea, playerActiveDeck);
        InstantiateCardsInHand(enemyHandArea, enemyActiveDeck);

    }
    
    void OnDisable()
    {
        // Limpa as cartas ao desativar para não manter estado
        foreach (Transform child in playerHandArea)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in enemyHandArea)
        {
            Destroy(child.gameObject);
        }
    }

    //instancia as cartas na tela de preparação
    void InstantiateCardsInHand(Transform handArea, List<CardData> deck)
    {
        // Limpa as cartas existentes na mão
        foreach (Transform child in handArea)
        {
            Destroy(child.gameObject);
        }


        foreach (var cardData in deck)
        {
            var cardGO = Instantiate(cardPrefab, handArea);
            var ui = cardGO.GetComponent<CardUI>();
            ui.SetCard(cardData, Owner.None);
        }
    }
}
