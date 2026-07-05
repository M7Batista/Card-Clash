using UnityEngine;

public class HomeScreen : MonoBehaviour
{
    [Header("Personagem")]
    [SerializeField] private CharacterDisplayController characterDisplayController;
    [SerializeField] private string defaultCharacterID = "Marine";

    void OnEnable()
    {
        Debug.Log("HomeScreen OnEnable");
        LoadHomeCharacter();
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.menuMusic);
    }

    void OnDisable()
    {
        if (characterDisplayController != null)
        {
            characterDisplayController.StopVideo();
        }
    }

    private void LoadHomeCharacter()
    {
        if (characterDisplayController == null)
        {
            Debug.LogWarning("CharacterDisplayController não atribuído em HomeScreen.");
            return;
        }

        string characterID = PlayerPrefs.HasKey("HomeScreenCharacter")
            ? PlayerPrefs.GetString("HomeScreenCharacter")
            : defaultCharacterID;

        if (string.IsNullOrEmpty(characterID))
        {
            Debug.LogWarning("Nenhum personagem foi definido para a HomeScreen.");
            return;
        }

        characterDisplayController.LoadCharacter(characterID);
    }
}
