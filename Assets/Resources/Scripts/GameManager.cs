using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
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
        coins = PlayerPrefs.GetInt("Coins", 0);
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

    public int GetRankWins()
    {
        return PlayerPrefs.GetInt(RANK_WINS_KEY, 0);
    }
    public void ResetRankWins()
    {
        PlayerPrefs.SetInt(RANK_WINS_KEY, 0);
    }
   
}
