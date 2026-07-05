using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaScreen : MonoBehaviour
{
    [Header("Referências de UI")]
    
    public GameObject cartaPrefab;

    [Header("Painel base")]
    public GameObject basePanel;
    public Button btnGacha1x;
    public Button btnGacha5x;

    [Header("Painel de Visualização de Carta")]
    public GameObject viewCardPanel;
    public Transform resultadoArea;
    public Button btnOk;

    [Header("Painel de Probabilidades")]
    public Button btnProbabilityInfo;
    public GameObject probabilityPanel;
    public Button btnCloseProbabilityPanel;
    public TextMeshProUGUI txtProbabilityCommon;
    public TextMeshProUGUI txtProbabilityUncommon;
    public TextMeshProUGUI txtProbabilityRare;
    public TextMeshProUGUI txtProbabilityEpic;
    public TextMeshProUGUI txtProbabilityLegendary;

    [Header("Configuração do Gacha")]
    int preco1x = 100;
    int preco5x = 500;
    List<CardData> poolCartas;
    List<float> chancesPorRaridade;

    void Awake()
    {
        InitializeGachaConfig();
    }

    void Start()
    {
        // Carrega a lista poolCartas 
        poolCartas = new List<CardData>();

        CardData[] allCardsData = Resources.LoadAll<CardData>("cards");
        foreach (CardData card in allCardsData)
        {
            poolCartas.Add(card);
        }
    }

    void OnEnable()
    {
        btnGacha1x.onClick.AddListener(() => TentarGacha(1));
        btnGacha5x.onClick.AddListener(() => TentarGacha(5));
        btnOk.onClick.AddListener(OnOkClick);
        btnOk.gameObject.SetActive(false);

        if (btnProbabilityInfo != null)
            btnProbabilityInfo.onClick.AddListener(ShowProbabilityPanel);

        if (btnCloseProbabilityPanel != null)
            btnCloseProbabilityPanel.onClick.AddListener(HideProbabilityPanel);

        HideProbabilityPanel();
        UpdateProbabilityText();

        if (basePanel != null)
            basePanel.SetActive(true);
        if (viewCardPanel != null)
            viewCardPanel.SetActive(false);
    }

    void OnDisable()
    {
        btnGacha1x.onClick.RemoveAllListeners();
        btnGacha5x.onClick.RemoveAllListeners();
        btnOk.onClick.RemoveAllListeners();

        if (btnProbabilityInfo != null)
            btnProbabilityInfo.onClick.RemoveAllListeners();

        if (btnCloseProbabilityPanel != null)
            btnCloseProbabilityPanel.onClick.RemoveAllListeners();

        ClearResult();
    }

    

    void TentarGacha(int quantidade)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager não encontrado para consumir moedas.");
            return;
        }

        int preco = quantidade == 1 ? preco1x : preco5x;
        if (GameManager.Instance.coins < preco)
        {
            Debug.Log("Moedas insuficientes!");
            return;
        }

        GameManager.Instance.SpendCoins(preco);
        List<CardData> cartas = SortearCartas(quantidade);
        MostrarCartas(cartas);
    }

    List<CardData> SortearCartas(int quantidade)
    {
        List<CardData> resultado = new List<CardData>();
        for (int i = 0; i < quantidade; i++)
        {
            CardData carta = SortearCartaPorRaridade();
            resultado.Add(carta);
        }
        return resultado;
    }

    CardData SortearCartaPorRaridade()
    {
        float total = 0f;
        foreach (float chance in chancesPorRaridade) total += chance;
        float rand = Random.Range(0, total);
        float acumulado = 0f;
        int raridadeIndex = 0;
        for (int i = 0; i < chancesPorRaridade.Count; i++)
        {
            acumulado += chancesPorRaridade[i];
            if (rand <= acumulado)
            {
                raridadeIndex = i;
                break;
            }
        }
        // Filtra cartas da raridade sorteada
        List<CardData> cartasDaRaridade = poolCartas.FindAll(c => (int)c.rarity == raridadeIndex);
        if (cartasDaRaridade.Count == 0) cartasDaRaridade = poolCartas; // fallback
        return cartasDaRaridade[Random.Range(0, cartasDaRaridade.Count)];
    }

    void MostrarCartas(List<CardData> cartas)
    {
        // Desativa os botões de gacha para evitar cliques durante a animação
        btnGacha1x.interactable = false;
        btnGacha5x.interactable = false;
        foreach (Transform filho in resultadoArea) Destroy(filho.gameObject);
        if (basePanel != null)
            basePanel.SetActive(false);
        if (viewCardPanel != null)
            viewCardPanel.SetActive(true);
        StartCoroutine(MostrarCartasSequencial(cartas));
    }

    System.Collections.IEnumerator MostrarCartasSequencial(List<CardData> cartas)
    {
        foreach (CardData carta in cartas)
        {
            GameObject obj = Instantiate(cartaPrefab, resultadoArea);
            // Aqui você pode preencher a UI da carta com os dados de CardData
            obj.GetComponent<CardUI>().SetCard(carta, Owner.Player);
            // Adicona as cartas à coleção do jogador aqui, se necessário
            PlayerDeckManager.AddCardToCollection(carta.id); // Removido em testes
            Button btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCardClicked(obj.GetComponent<CardUI>()));

            // Efeito de fade-in
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            float fadeTime = 0.3f;
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(t / fadeTime);
                yield return null;
            }
            cg.alpha = 1f;

            yield return new WaitForSeconds(0.1f);
        }
        btnOk.gameObject.SetActive(true);
    
    }

    void OnOkClick()
    {
        ClearResult();
        if (basePanel != null)
            basePanel.SetActive(true);
        if (viewCardPanel != null)
            viewCardPanel.SetActive(false);
    }

    void ShowProbabilityPanel()
    {
        if (probabilityPanel == null)
            return;

        probabilityPanel.SetActive(true);
    }

    void HideProbabilityPanel()
    {
        if (probabilityPanel == null)
            return;

        probabilityPanel.SetActive(false);
    }

    void UpdateProbabilityText()
    {
        if (chancesPorRaridade == null || chancesPorRaridade.Count < 5)
            return;

        float total = 0f;
        foreach (float chance in chancesPorRaridade)
            total += Mathf.Max(chance, 0f);

        if (total <= 0f)
            return;

        if (txtProbabilityCommon != null)
            txtProbabilityCommon.text = $"Common: {FormatProbability(chancesPorRaridade[0] / total)}";
        if (txtProbabilityUncommon != null)
            txtProbabilityUncommon.text = $"Uncommon: {FormatProbability(chancesPorRaridade[1] / total)}";
        if (txtProbabilityRare != null)
            txtProbabilityRare.text = $"Rare: {FormatProbability(chancesPorRaridade[2] / total)}";
        if (txtProbabilityEpic != null)
            txtProbabilityEpic.text = $"Epic: {FormatProbability(chancesPorRaridade[3] / total)}";
        if (txtProbabilityLegendary != null)
            txtProbabilityLegendary.text = $"Legendary: {FormatProbability(chancesPorRaridade[4] / total)}";
    }

    string FormatProbability(float value)
    {
        return (value * 100f).ToString("F1") + "%";
    }

    void InitializeGachaConfig()
    {
        if (chancesPorRaridade != null && chancesPorRaridade.Count >= 5)
            return;

        chancesPorRaridade = new List<float>();
        chancesPorRaridade.Add(0.70f); // Comum
        chancesPorRaridade.Add(0.20f); // Incomum
        chancesPorRaridade.Add(0.07f); // Rara
        chancesPorRaridade.Add(0.02f); // Épica
        chancesPorRaridade.Add(0.01f); // Lendária
    }

    void OnCardClicked(CardUI cardUI)
    {
        if (viewCardPanel != null)
            viewCardPanel.SetActive(true);
        if (basePanel != null)
            basePanel.SetActive(false);

        CardView.Instance.ShowCard(cardUI.cardData);
    }
    void ClearResult()
    {
        foreach (Transform filho in resultadoArea)
        {
            Destroy(filho.gameObject);
        }
        btnOk.gameObject.SetActive(false);
        btnGacha1x.interactable = true;
        btnGacha5x.interactable = true;
    }

}
