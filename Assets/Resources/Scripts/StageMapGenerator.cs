using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageMapGenerator : MonoBehaviour
{
    [Header("Configurações do Mapa")]
    public int totalStages = 100;

    [Header("Referências")]
    public RectTransform content;        // Content do Scroll View
    public ScrollRect scrollRect;        // O Scroll View em si
    public GameObject stageButtonPrefab; // Prefab do botão de estágio (com número)
    public int unlockedStage = 1;
    private Color unlockedColor = Color.white;
    private Color lockedColor = new Color32(150, 150, 150, 255);

    public static StageMapGenerator Instance;


    void Start()
    {
        Instance = this;
        unlockedStage = PlayerPrefs.GetInt("UnlockedStage", 1);
        GenerateStages();

        // 🔹 Garante que o Scroll sempre inicie no topo (estágio 1)
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void GenerateStages()
    {
        // limpar botões antigos
        foreach (Transform t in content) Destroy(t.gameObject);
        float posY = 0;
        float posX = 0;
        for (int i = 1; i <= totalStages; i++)
        {
            GameObject newButton = Instantiate(stageButtonPrefab, content);
            newButton.name = "Stage_" + i;

            TMP_Text text = newButton.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = i.ToString();


            posX = (i % 2 == 0) ? 200 : -200;
            posY -= 150;
            //newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);
            //content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Abs(posY) + 150);
            // imagem
            var image = newButton.GetComponent<Image>();
            var button = newButton.GetComponent<Button>();

            if (i <= unlockedStage)
            {
                // desbloqueado
                if (image != null) image.color = unlockedColor;
                if (button != null)
                {
                    button.interactable = true;
                    int stageIndex = i;
                    button.onClick.AddListener(() => OnStageClicked(stageIndex));
                }
            }
            else
            {
                // bloqueado
                if (image != null) image.color = lockedColor;
                if (button != null) button.interactable = false;
            }
            // Chefes em destaque
            if (i % 10 == 0)
            {
                Image img = newButton.GetComponent<Image>();
                if (img != null)
                    img.color = Color.yellow;
            }
        }
    }
    void OnStageClicked(int index)
    {
        Debug.Log("Jogador entrou no estágio " + index);
        BattleCardScreen.Instance.PrepareBattle(index);

    }
    public void UnlockNextStage()
    {
        if (unlockedStage < totalStages)
        {
            unlockedStage++;
            PlayerPrefs.SetInt("UnlockedStage", unlockedStage);
            PlayerPrefs.Save();

            // Regenera tela com novo desbloqueio
            GenerateStages();
        }
    }

}
