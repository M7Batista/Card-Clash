using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public Transform uiCanvas;
    public GameObject floatingMessagePrefab;
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
        // 🔹 Limpa PlayerPrefs (mesmo que já fazia)
        PlayerPrefs.DeleteKey("PlayerDeck");
        PlayerPrefs.DeleteKey("PlayerCollection");
        PlayerPrefs.DeleteKey("UnlockedStage");
        PlayerPrefs.DeleteKey("HomeCharacterID");
        PlayerPrefs.DeleteKey("PLAYER_TICKETS");
        PlayerPrefs.DeleteKey("LAST_RECHARGE_DATE");
        PlayerPrefs.Save();

        GameObject go = Instantiate(floatingMessagePrefab, uiCanvas);
        go.transform.localPosition = Vector3.zero;
        go.GetComponent<FloatingMessage>().Show("Dados do jogo apagados.");
    }
}
