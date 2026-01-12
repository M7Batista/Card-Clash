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
    public Button pStartBattleButton;
    public Button pCancelBattleButton;

    public Transform playerHandArea;
    public Transform enemyHandArea;
    public GameObject cardPrefab;

    void OnEnable()
    {
        Debug.Log("PrepareBattleScreen Opened");
        int stage = PlayerPrefs.GetInt("UnlockedStage", 1);
        PrepareBattle(stage);

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

        pStartBattleButton.onClick.RemoveAllListeners();
        pStartBattleButton.onClick.AddListener(() =>
        {

            BattleCardScreen.Instance.StartBattle();
            //Desativa essa tela
            this.gameObject.SetActive(false);

        });
        pCancelBattleButton.onClick.RemoveAllListeners();
        pCancelBattleButton.onClick.AddListener(() =>
        {

        });

        InstantiateCardsInHand(playerHandArea, playerActiveDeck);
        InstantiateCardsInHand(enemyHandArea, enemyActiveDeck);

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
