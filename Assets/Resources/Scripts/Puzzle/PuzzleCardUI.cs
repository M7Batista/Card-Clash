using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleCardUI : MonoBehaviour
{
    [Header("UI")]
    public Image previewImage;
    public Button playButton;
    public GameObject lockIcon;
    public GameObject checkIcon;
    public TextMeshProUGUI idText;

    [Header("Materials")]
    public Material grayscaleMaterial;
    public Material normalMaterial;
    public Material blurMaterial;

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
        previewImage.material = normalMaterial;

        switch (data.state)
        {
            case PuzzleState.Available:
                previewImage.material = grayscaleMaterial;
                playButton.gameObject.SetActive(true);
                playButton.onClick.AddListener(OnCardClicked);
                break;

            case PuzzleState.Locked:
                previewImage.material = blurMaterial;
                //previewImage.color =  new Color(0.5f, 0.5f, 0.5f, 1.0f); 
                lockIcon.SetActive(true);
                break;

            case PuzzleState.Completed:
                previewImage.material = normalMaterial;
                checkIcon.SetActive(true);
                playButton.gameObject.SetActive(true);
                break;
        }

        if (idText != null)
            idText.text = data.id.ToString("D2"); // Exibe o ID com dois dígitos (ex: 01, 02, etc.)
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
