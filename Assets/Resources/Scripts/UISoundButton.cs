using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UISoundButton : MonoBehaviour
{
    public string soundName = "Click"; // Nome do som configurado no AudioManager

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySFX(soundName);
        });
    }
}
