using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public string targetSceneName = "GameScene";
    public float minVisibleTime = 2f;

    void Start()
    {
        StartCoroutine(LoadMainSceneAsync());
    }

    IEnumerator LoadMainSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);
        operation.allowSceneActivation = false;

        float startTime = Time.time;
        float displayProgress = 0f;

        while (!operation.isDone)
        {
            float elapsed = Time.time - startTime;
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (elapsed < minVisibleTime)
            {
                targetProgress = Mathf.Max(targetProgress, elapsed / minVisibleTime);
            }

            if (operation.progress >= 0.9f)
            {
                targetProgress = 1f;
            }

            displayProgress = Mathf.Lerp(displayProgress, targetProgress, 0.12f);

            if (progressBar != null)
                progressBar.value = displayProgress;

            if (progressText != null)
                progressText.text = $"Loading... {Mathf.Round(displayProgress * 100f)}%";

            if (operation.progress >= 0.9f && elapsed >= minVisibleTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        displayProgress = 1f;

        if (progressBar != null)
            progressBar.value = displayProgress;

        if (progressText != null)
            progressText.text = "Loading... 100%";
    }
}