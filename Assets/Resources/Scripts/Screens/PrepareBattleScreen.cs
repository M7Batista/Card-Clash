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
        string currentRank = RankSystem.GetPlayerRankName();

        if (!buttonsInitialized)
        {
            InitializeButtons();
            buttonsInitialized = true;
        }

        PrepareBattle(currentRank);
    }

    void InitializeButtons()
    {
        ButtonStartBattle.onClick.RemoveAllListeners();
        ButtonStartBattle.onClick.AddListener(() =>
        {
            BattleSetupManager.Instance.StartBattle();
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
            TutorialManager.Instance?.NotifyDeckEditorOpened();
        });
    }

    public void PrepareBattle(string currentRank)
    {
        BattleSetupManager.Instance.SetPlayerActiveDeck();
        BattleSetupManager.Instance.SetEnemyActiveDeck(currentRank);

        List<CardData> playerActiveDeck = BattleSetupManager.Instance.playerActiveDeck;
        List<CardData> enemyActiveDeck = BattleSetupManager.Instance.enemyActiveDeck;
        EnemyAI.Instance.SetEnemyDeck(enemyActiveDeck);
        EnemyAI.Instance.SetDifficultyByRank(currentRank);

        int enemyPower = 0;
        foreach (var card in enemyActiveDeck)
            enemyPower += (card.top + card.bottom + card.left + card.right);
        enemyPowerText.text = enemyPower.ToString();

        int playerPower = 0;
        foreach (var card in playerActiveDeck)
            playerPower += (card.top + card.bottom + card.left + card.right);
        playerPowerText.text = playerPower.ToString();

        stageText.text = currentRank;

        InstantiateCardsInHand(playerHandArea, playerActiveDeck);
        InstantiateCardsInHand(enemyHandArea, enemyActiveDeck);
    }

    void OnDisable()
    {
        foreach (Transform child in playerHandArea)
            Destroy(child.gameObject);
        foreach (Transform child in enemyHandArea)
            Destroy(child.gameObject);
    }

    void InstantiateCardsInHand(Transform handArea, List<CardData> deck)
    {
        foreach (Transform child in handArea)
            Destroy(child.gameObject);

        foreach (var cardData in deck)
        {
            var cardGO = Instantiate(cardPrefab, handArea);
            var ui = cardGO.GetComponent<CardUI>();
            ui.SetCard(cardData, Owner.None);
        }
    }
}
