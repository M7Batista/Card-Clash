using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public int coins;
    public static GameManager Instance;

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

}
