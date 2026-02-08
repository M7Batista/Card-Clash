using UnityEngine;
using UnityEngine.UI;

public class PuzzleCardUI : MonoBehaviour
{
    [Header("UI")]
    public Image previewImage;
    public GameObject overlay;
    public Button playButton;
    public GameObject lockIcon;
    public GameObject checkIcon;
    public Text progressText;

    [Header("Materials")]
    public Material grayscaleMaterial;
    public Material normalMaterial;

    private PuzzleData data;

    public void Setup(PuzzleData puzzleData)
    {
        data = puzzleData;
        previewImage.sprite = data.previewImage;

        ApplyState();
    }

    void ApplyState()
    {
        playButton.gameObject.SetActive(false);
        lockIcon.SetActive(false);
        checkIcon.SetActive(false);
        overlay.SetActive(true);

        previewImage.material = normalMaterial;

        switch (data.state)
        {
            case PuzzleState.Available:
                playButton.gameObject.SetActive(true);
                playButton.onClick.AddListener(OnCardClicked);
                break;

            case PuzzleState.Locked:
                lockIcon.SetActive(true);
                previewImage.material = grayscaleMaterial;
                break;

            case PuzzleState.Completed:
                overlay.SetActive(false);
                checkIcon.SetActive(true);
                break;
        }

        if (progressText != null)
            progressText.text = data.progress > 0 ? $"{data.progress}%" : "";
    }


    public void OnCardClicked()
    {
        switch (data.state)
        {
            case PuzzleState.Available:
                PuzzleGameUIManager.Instance.OpenPuzzleAssembly(data.id);
                break;

            case PuzzleState.Locked:
                Debug.Log("Conclua o quebra-cabeça anterior");
                break;

            case PuzzleState.Completed:
                Debug.Log("Popup: Reiniciar / Visualizar imagem");
                break;
        }
    }

}
