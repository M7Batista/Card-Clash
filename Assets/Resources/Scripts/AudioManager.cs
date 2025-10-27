using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip menuMusic;
    public AudioClip battleMusic;
    public List<AudioClip> sfxClips; // Lista de efeitos sonoros

    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    // target volume usado para fade-in quando uma música começa
    private float targetMusicVolume = 1f;
    private Coroutine musicFadeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Preenche o dicionário com base nos nomes dos clipes
            foreach (var clip in sfxClips)
            {
                if (clip != null && !sfxDict.ContainsKey(clip.name))
                    sfxDict.Add(clip.name, clip);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // garante que targetMusicVolume seja usado como destino de fade-in
        targetMusicVolume = musicVol;
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }

    // 🔹 Tocar música de fundo
    public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
    {
        if (clip == null) return;

        // evita coroutines concorrentes
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(FadeMusic(clip, fadeDuration));
    }

    // 🔹 Fade suave entre músicas
    private System.Collections.IEnumerator FadeMusic(AudioClip newClip, float duration)
    {
        if (musicSource.clip == newClip) yield break;

        float startVolume = musicSource.volume;

        // Fade out (usa duration)
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        musicSource.volume = 0f;

        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in até o volume alvo (targetMusicVolume), não até startVolume antigo
        float targetVol = targetMusicVolume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, targetVol, t / duration);
            yield return null;
        }
        musicSource.volume = targetVol;

        musicFadeCoroutine = null;
    }

    // 🔹 Tocar efeito sonoro
    public void PlaySFX(string name)
    {
        if (sfxDict.TryGetValue(name, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{name}' não encontrado!");
        }
    }
    public void StopMusic()
    {
        // cancela qualquer fade atual e inicia fade-out/stop
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }
        StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutAndStop()
    {
        float start = musicSource.volume;
        float duration = 0.5f; // ajuste se quiser fade-out mais lento
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        musicSource.volume = 0f;

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void SetMusicVolume(float value)
    {
        targetMusicVolume = value;
        musicSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
    }
    public void MuteMusic(bool isMuted)
    {
        musicSource.mute = isMuted;
    }

    public void MuteSFX(bool isMuted)
    {
        sfxSource.mute = isMuted;
    }
}