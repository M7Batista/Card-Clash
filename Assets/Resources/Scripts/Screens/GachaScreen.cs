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

    [Header("Atalho para virar todas as cartas")]
    public TextMeshProUGUI txtFlipAll;

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
    int preco5x = 450;
    List<CardData> poolCartas;
    List<float> chancesPorRaridade;
    int cartasViradas;
    int totalCartasExibidas;
    private Button btnFlipAll;

    // Inicializa as probabilidades do gacha ao criar a tela.
    void Awake()
    {
        InitializeGachaConfig();
    }

    // Carrega as cartas disponíveis para sorteio a partir dos assets de recursos.
    void Start()
    {
        poolCartas = new List<CardData>();

        CardData[] allCardsData = Resources.LoadAll<CardData>("cards");
        foreach (CardData card in allCardsData)
        {
            poolCartas.Add(card);
        }
    }

    // Vincula os eventos da interface e reseta o estado visual da tela ao entrar.
    void OnEnable()
    {
        btnGacha1x.onClick.AddListener(() => TentarGacha(1));
        btnGacha5x.onClick.AddListener(() => TentarGacha(5));
        btnOk.onClick.AddListener(OnOkClick);
        btnOk.gameObject.SetActive(false);
        btnOk.interactable = false;
        cartasViradas = 0;
        totalCartasExibidas = 0;

        if (btnProbabilityInfo != null)
            btnProbabilityInfo.onClick.AddListener(ShowProbabilityPanel);

        if (btnCloseProbabilityPanel != null)
            btnCloseProbabilityPanel.onClick.AddListener(HideProbabilityPanel);

        HideProbabilityPanel();
        UpdateProbabilityText();
        txtFlipAll.GetComponent<Button>().onClick.AddListener(VirarTodasCartas);

        if (basePanel != null)
            basePanel.SetActive(true);
        if (viewCardPanel != null)
            viewCardPanel.SetActive(false);

        TutorialManager.Instance?.NotifyGachaScreenOpened();
    }

    

    // Remove os listeners para evitar duplicação ao trocar de tela.
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

    

    // Consome moedas e inicia o processo de sorteio das cartas do gacha.
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

        if (quantidade == 5)
            TutorialManager.Instance?.NotifyGacha5xClicked();

        List<CardData> cartas = SortearCartas(quantidade);
        MostrarCartas(cartas);
    }

    // Cria a lista de cartas sorteadas de acordo com a quantidade solicitada.
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

    // Sorteia uma raridade com base nas chances configuradas e escolhe uma carta dessa raridade.
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

    // Exibe as cartas sorteadas na área de resultado e prepara o estado da tela.
    void MostrarCartas(List<CardData> cartas)
    {
        // Desativa os botões de gacha para evitar cliques durante a animação
        btnGacha1x.interactable = false;
        btnGacha5x.interactable = false;
        cartasViradas = 0;
        totalCartasExibidas = cartas.Count;
        btnOk.gameObject.SetActive(false);
        btnOk.interactable = false;
        foreach (Transform filho in resultadoArea) Destroy(filho.gameObject);
        if (basePanel != null)
            basePanel.SetActive(false);
        if (viewCardPanel != null)
            viewCardPanel.SetActive(true);

        StartCoroutine(MostrarCartasSequencial(cartas));
    }

    // Instancia as cartas uma por vez com efeito de entrada e as deixa prontas para virar.
    System.Collections.IEnumerator MostrarCartasSequencial(List<CardData> cartas)
    {
        foreach (CardData carta in cartas)
        {
            GameObject obj = Instantiate(cartaPrefab, resultadoArea);
            CardUI cardUI = obj.GetComponent<CardUI>();
            if (cardUI == null)
                cardUI = obj.AddComponent<CardUI>();

            cardUI.SetCard(carta, Owner.Player);
            cardUI.ShowBack();

            CardFlip cardFlip = obj.GetComponent<CardFlip>();
            if (cardFlip == null)
                cardFlip = obj.AddComponent<CardFlip>();

            if (cardFlip.front == null)
                cardFlip.front = cardUI.front;
            if (cardFlip.back == null)
                cardFlip.back = cardUI.back;

            // Adicona as cartas à coleção do jogador aqui, se necessário
            PlayerDeckManager.AddCardToCollection(carta.id); // Removido em testes
            Button btn = obj.GetComponent<Button>();
            if (btn == null) btn = obj.AddComponent<Button>();
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnCardClicked(cardUI, btn));

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
        if (txtFlipAll != null)
        {
            txtFlipAll.gameObject.SetActive(true);
            if (btnFlipAll != null)
                btnFlipAll.interactable = true;
        }
    }

    // Executa o flip da carta clicada e habilita o botão de confirmação quando todas estiverem viradas.
    void OnCardClicked(CardUI cardUI, Button button)
    {
        if (cardUI == null || button == null || !button.interactable)
            return;

        CardFlip cardFlip = button.GetComponentInParent<CardFlip>();
        if (cardFlip != null)
        {
            cardFlip.FlipCardForGacha(cardUI);
        }
        else
        {
            cardUI.Flip();
        }

        button.interactable = false;
        cartasViradas++;
        AudioManager.Instance?.PlaySFX("card-slide-8");
        if (cartasViradas >= totalCartasExibidas && totalCartasExibidas > 0)
        {
            btnOk.gameObject.SetActive(true);
            btnOk.interactable = true;
        }
    }

    void VirarTodasCartas()
    {
        if (totalCartasExibidas <= 0)
            return;

        if (btnFlipAll != null)
            btnFlipAll.interactable = false;

        if (txtFlipAll != null)
            txtFlipAll.gameObject.SetActive(false);

        foreach (Transform filho in resultadoArea)
        {
            Button button = filho.GetComponent<Button>();
            if (button == null || !button.interactable)
                continue;

            CardFlip cardFlip = filho.GetComponent<CardFlip>();
            CardUI cardUI = filho.GetComponent<CardUI>();
            if (cardFlip != null)
            {
                cardFlip.FlipCardForGacha(cardUI);
            }
            else if (cardUI != null)
            {
                cardUI.Flip();
            }

            button.interactable = false;
            cartasViradas++;
        }

        TutorialManager.Instance?.NotifyRevealAllClicked();

        if (cartasViradas >= totalCartasExibidas && totalCartasExibidas > 0)
        {
            btnOk.gameObject.SetActive(true);
            btnOk.interactable = true;
        }
    }

    // Fecha a etapa de visualização e retorna à tela base do gacha.
    void OnOkClick()
    {
        ClearResult();
        if (basePanel != null)
            basePanel.SetActive(true);
        if (viewCardPanel != null)
            viewCardPanel.SetActive(false);
    }

    // Mostra o painel com as probabilidades do gacha.
    void ShowProbabilityPanel()
    {
        if (probabilityPanel == null)
            return;

        probabilityPanel.SetActive(true);
    }

    // Esconde o painel com as probabilidades do gacha.
    void HideProbabilityPanel()
    {
        if (probabilityPanel == null)
            return;

        probabilityPanel.SetActive(false);
    }

    // Atualiza os textos do painel de probabilidades com valores normalizados.
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

    // Formata um valor decimal como porcentagem com uma casa decimal.
    string FormatProbability(float value)
    {
        return (value * 100f).ToString("F1") + "%";
    }

    // Define as chances de raridade usadas pelo sistema de gacha.
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

    
    // Limpa as cartas exibidas e retorna a tela ao estado inicial.
    void ClearResult()
    {
        foreach (Transform filho in resultadoArea)
        {
            Destroy(filho.gameObject);
        }
        btnOk.gameObject.SetActive(false);
        btnOk.interactable = false;
        cartasViradas = 0;
        totalCartasExibidas = 0;
        btnGacha1x.interactable = true;
        btnGacha5x.interactable = true;

        if (txtFlipAll != null)
            txtFlipAll.gameObject.SetActive(false);
    }

}
