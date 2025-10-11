using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle musicMuteToggle;
    public Toggle sfxMuteToggle;

    private void Start()
    {
        // 🔹 Carrega preferências salvas
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        bool sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        // 🔹 Atualiza UI
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;
        musicMuteToggle.isOn = musicMuted;
        sfxMuteToggle.isOn = sfxMuted;

        // 🔹 Aplica configurações
        AudioManager.Instance.SetMusicVolume(musicVolume);
        AudioManager.Instance.SetSFXVolume(sfxVolume);
        AudioManager.Instance.MuteMusic(musicMuted);
        AudioManager.Instance.MuteSFX(sfxMuted);

        // 🔹 Listeners
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        musicMuteToggle.onValueChanged.AddListener(SetMusicMute);
        sfxMuteToggle.onValueChanged.AddListener(SetSFXMute);
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

    public void SetMusicMute(bool isMuted)
    {
        AudioManager.Instance.MuteMusic(isMuted);
        PlayerPrefs.SetInt("MusicMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSFXMute(bool isMuted)
    {
        AudioManager.Instance.MuteSFX(isMuted);
        PlayerPrefs.SetInt("SFXMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }
}
