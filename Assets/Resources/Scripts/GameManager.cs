using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void ClearCache()
    {
        // Pega o caminho completo do diretório de dados persistentes
        string path = Application.persistentDataPath;

        // Verifica se o diretório existe antes de tentar apagá-lo
        if (Directory.Exists(path))
        {
            // Apaga o diretório e todo o seu conteúdo (true)
            Directory.Delete(path, true);
            Debug.Log("Cache do jogo apagado com sucesso.");
        }
        else
        {
            Debug.LogWarning("O diretório de cache já está vazio. Nada para apagar.");
        }
    }
    public void ClearData()
    {
        // 🔹 Limpa chaves usadas pelo PlayerDeckManager
        PlayerPrefs.DeleteKey("PlayerDeck");
        PlayerPrefs.DeleteKey("PlayerCollection");
        PlayerPrefs.DeleteKey("UnlockedStage");
        PlayerPrefs.DeleteKey("HomeCharacterID");
        
        // 🔹 Limpa chaves usadas pelo CurrencyManager
        PlayerPrefs.DeleteKey("PLAYER_TICKETS");
        PlayerPrefs.DeleteKey("LAST_RECHARGE_DATE");
        

        // 🔹 Opcional: recarregar cena para resetar UI
        //UnityEngine.SceneManagement.SceneManager.LoadScene(
            //UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        Dialog.Instance.ShowMessage("All data has been cleared!");
        Debug.Log("⚠ Dados apagados com sucesso!");
    }

}
