using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Responsável por preparar a configuração da batalha:
/// - gerar deck inimigo por estágio
/// - aplicar regras especiais (RuleSame / RulePlus)
/// - configurar dificuldade (salvar em PlayerPrefs e notificar AI se disponível)
/// </summary>
public class BattleSetupManager : MonoBehaviour
{
    public static BattleSetupManager Instance { get; private set; }
    public List<CardData> playerActiveDeck = new List<CardData>();   // 🔹 As 5 cartas escolhidas pelo jogador para a partida
    public List<CardData> enemyActiveDeck = new List<CardData>();    // 🔹 As 5 cartas que o inimigo usará na partida
    public bool ruleSame = true;
    public bool rulePlus = true;

    void Start()
    {
        Instance = this;
    }
    void LoadPlayerActiveDeck()
    {

        // 🔹 Carregar os ids do deck ativo do jogador
        List<int> playerDeckIds = PlayerDeckManager.LoadDeck();
        playerActiveDeck.Clear();
        foreach (int id in playerDeckIds)
        {
            CardData card = PlayerDeckManager.GetCardById(id);
            if (card != null)
                // Adicionar carta ao deck ativo do jogador
                playerActiveDeck.Add(card);
        }

    }
    void LoadEnemyActiveDeck(int stageNumber)
    {
        // 🔹 Gerar deck inimigo baseado no estágio atual
        enemyActiveDeck.Clear();
        enemyActiveDeck = EnemyDeckManager.Instance.GenerateEnemyDeck(stageNumber);
        EnemyAI.Instance.SetEnemyDeck(enemyActiveDeck);
    }
}