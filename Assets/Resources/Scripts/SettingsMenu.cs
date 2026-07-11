using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Button clearDataButton;
    public GameObject floatingMessagePrefab;
    public Transform uiCanvas;

    private void Start()
    {
        // 🔹 Carrega preferências salvas
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 🔹 Atualiza UI
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        // 🔹 Aplica configurações
        AudioManager.Instance.SetMusicVolume(musicVolume);
        AudioManager.Instance.SetSFXVolume(sfxVolume);

        // 🔹 Listeners
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        clearDataButton.onClick.AddListener(ClearData);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
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
        PlayerPrefs.DeleteKey("HomeScreenCharacter");
        PlayerPrefs.Save();

        GameObject go = Instantiate(floatingMessagePrefab, uiCanvas);
        go.transform.localPosition = Vector3.zero;
        go.GetComponent<FloatingMessage>().Show("Dados do jogo apagados.");
        // Reinicia o jogo
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
