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
    public int currentStage = 1;
    public Color unlockedColor = new Color32(255, 117, 194, 255);
    public Color lockedColor = new Color32(150, 150, 150, 255);

    public static StageMapGenerator Instance;


    void Start()
    {
        Instance = this;
        unlockedStage = PlayerPrefs.GetInt("UnlockedStage", 1);
        currentStage = unlockedStage;
        GenerateStages();

        // 🔹 Após gerar os estágios, rola automaticamente até o estágio atual
        ScrollToStage(currentStage);
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
                    img.color = Color.blue;
            }
        }
    }
    void OnStageClicked(int index)
    {
        Debug.Log("Jogador entrou no estágio " + index);
        currentStage = index;
    }
    public void UnlockNextStage()
    {
        Debug.Log("Desbloqueando próximo estágio...");
        Debug.Log("Estágio atual: " + currentStage + ", Estágio desbloqueado: " + unlockedStage);
        if (unlockedStage < totalStages && currentStage == unlockedStage)
        {
            unlockedStage++;
            currentStage = unlockedStage;
            PlayerPrefs.SetInt("UnlockedStage", unlockedStage);
            PlayerPrefs.Save();

            // Regenera tela com novo desbloqueio
            GenerateStages();
            // manter o foco no estágio desbloqueado
            ScrollToStage(currentStage);
        }
    }

    // Rola o ScrollRect para centralizar (ou aproximar) o botão do estágio especificado
    private void ScrollToStage(int stageIndex)
    {
        if (content == null || scrollRect == null) return;

        Canvas.ForceUpdateCanvases();

        Transform targetT = content.Find("Stage_" + stageIndex);
        if (targetT == null) return;

        RectTransform target = targetT as RectTransform;
        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.transform.Find("Viewport") as RectTransform;
        if (viewport == null) return;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        // posição local do centro do alvo dentro do content (y aumenta para cima)
        // target.anchoredPosition.y costuma ser negativo quando itens são posicionados abaixo
        float targetCenterLocalY = -target.anchoredPosition.y + (target.rect.height * 0.5f);

        float scrollableHeight = contentHeight - viewportHeight;
        if (scrollableHeight <= 0f)
        {
            // não há espaço para rolar
            scrollRect.verticalNormalizedPosition = 1f;
            return;
        }

        // queremos que o centro do target fique no meio da viewport
        float normalized = 1f - Mathf.Clamp01((targetCenterLocalY - (viewportHeight * 0.5f)) / scrollableHeight);

        scrollRect.verticalNormalizedPosition = normalized;
    }

}
