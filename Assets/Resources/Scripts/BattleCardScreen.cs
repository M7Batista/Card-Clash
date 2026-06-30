using UnityEngine;
using UnityEngine.UI;

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

    public static BattleCardScreen Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (BattleSetupManager.Instance != null)
            BattleSetupManager.Instance.InitializeBattleScreenReferences(this);
    }

    public void StartBattle()
    {
        BattleSetupManager.Instance?.StartBattle();
    }

    public void EnableControlButtons()
    {
        BattleSetupManager.Instance?.EnableControlButtons();
    }

    public void StartPlayerTurn()
    {
        BattleSetupManager.Instance?.StartPlayerTurn();
    }

    public void StartEnemyTurn()
    {
        BattleSetupManager.Instance?.StartEnemyTurn();
    }

    public void OnPlayerCardPlaced(CardUI cardUI)
    {
        BattleSetupManager.Instance?.OnPlayerCardPlaced(cardUI);
    }

    public void NextTurn()
    {
        BattleSetupManager.Instance?.NextTurn();
    }

    public void PosBattleSetup(int result)
    {
        BattleSetupManager.Instance?.PosBattleSetup(result);
    }

    public void RestartBattle()
    {
        BattleSetupManager.Instance?.RestartBattle();
    }

    public void ExitBattle()
    {
        BattleSetupManager.Instance?.ExitBattle();
    }

    public void ClearBattleState()
    {
        BattleSetupManager.Instance?.ClearBattleState();
    }
}
