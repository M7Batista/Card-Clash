using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    private const int STARTING_COINS = 500;

    public int coins;
    public static GameManager Instance;
    private const string RANK_WINS_KEY = "RankWins";

    void Awake()
    {
        Instance = this;
        LoadGame();
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("Coins"))
        {
            coins = STARTING_COINS;
            PlayerPrefs.SetInt("Coins", coins);
            PlayerPrefs.Save();
        }
        else
        {
            coins = PlayerPrefs.GetInt("Coins", 0);
        }
    }
    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("Coins", coins);
        TopPanelController.Instance?.UpdateCoinsDisplay();
    }

    public void SpendCoins(int amount)
    {
        if (amount <= 0) return;

        coins = Mathf.Max(0, coins - amount);
        PlayerPrefs.SetInt("Coins", coins);
        TopPanelController.Instance?.UpdateCoinsDisplay();
    }

    public void AddRankWin()
    {
        int currentWins = PlayerPrefs.GetInt(RANK_WINS_KEY, 0);

        string currentRank = RankSystem.GetPlayerRankName();
        RankInfo? rankInfo = RankSystem.GetRankInfo(currentRank);
        int winsToPromote = rankInfo.HasValue && rankInfo.Value.winsToPromote > 0 ? rankInfo.Value.winsToPromote : 5;

        // Se a próxima vitória atinge o limite, promove e reseta o contador
        if (currentWins + 1 >= winsToPromote)
        {
            Debug.Log($"O jogador atingiu {currentWins + 1} vitórias em '{currentRank}'. Promovendo rank e resetando vitórias.");
            RankSystem.PromotePlayerRank();
            ResetRankWins();
            return;
        }

        currentWins++;
        PlayerPrefs.SetInt(RANK_WINS_KEY, currentWins);
        PlayerPrefs.Save();
        Debug.Log($"Vitória contabilizada. Rank: {currentRank}, Vitórias: {currentWins}/{winsToPromote}");
    }

    public void RemoveRankWin()
    {
        int currentWins = PlayerPrefs.GetInt(RANK_WINS_KEY, 0);
        string currentRank = RankSystem.GetPlayerRankName();

        if (currentWins > 0)
        {
            currentWins--;
            PlayerPrefs.SetInt(RANK_WINS_KEY, currentWins);
            PlayerPrefs.Save();
            Debug.Log($"Derrota: perdeu 1 ponto de vitória. Rank: {currentRank}, Vitórias: {currentWins}");
            return;
        }

        RankSystem.DemotePlayerRank();
        PlayerPrefs.SetInt(RANK_WINS_KEY, 0);
        PlayerPrefs.Save();
        Debug.Log($"Derrota: sem pontos de vitória para perder. Rank reduzido de '{currentRank}'.");
    }

    public int GetRankWins()
    {
        return PlayerPrefs.GetInt(RANK_WINS_KEY, 0);
    }
    public void ResetRankWins()
    {
        PlayerPrefs.SetInt(RANK_WINS_KEY, 0);
        PlayerPrefs.Save();
    }
   
}
