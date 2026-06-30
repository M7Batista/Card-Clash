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
        if (currentWins >= 4)
        {
            Debug.LogWarning("O jogador já atingiu o número máximo de vitórias para o rank atual.");
            Debug.LogWarning("Jogador avança para o próximo rank e o contador de vitórias é resetado.");
            ResetRankWins();
            return;
        }
        currentWins++;
        PlayerPrefs.SetInt(RANK_WINS_KEY, currentWins);
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
