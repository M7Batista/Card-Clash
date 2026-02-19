using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance;
    public GameObject loadingPanel;
    public Animator loadingAnimator;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show()
    {
        Debug.Log("Mostrando tela de loading...");
        loadingPanel.SetActive(true);
        if (loadingAnimator != null)
            loadingAnimator.SetBool("Loading", true);
    }

    public void Hide()
    {
        if (loadingAnimator != null)
            loadingAnimator.SetBool("Loading", false);
        loadingPanel.SetActive(false);
    }
}
