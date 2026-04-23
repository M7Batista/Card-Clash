using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaScreen : MonoBehaviour
{
    [Header("Referências de UI")]
    public TextMeshProUGUI txtMoedas;
    public Button btnGacha1x;
    public Button btnGacha10x;
    public Transform resultadoArea;
    public GameObject cartaPrefab;
    public Button btnOk;
    public GameObject cardViewPanel;

    [Header("Configuração do Gacha")]
    int preco1x = 10;
    int preco10x = 100;
    List<CardData> poolCartas;
    List<float> chancesPorRaridade;

    private int moedas;
   
    void Start()
    {
        // Carrega a lista poolCartas 
        poolCartas = new List<CardData>();

        CardData[] allCardsData = Resources.LoadAll<CardData>("cards");
        foreach (CardData card in allCardsData)
        {
            poolCartas.Add(card);
        }
        // Configura as chances por raridade (exemplo)
        chancesPorRaridade = new List<float>();
        chancesPorRaridade.Add(0.70f); // Comum
        chancesPorRaridade.Add(0.20f); // Incomum
        chancesPorRaridade.Add(0.7f); // Rara
        chancesPorRaridade.Add(0.25f); // Epica
        chancesPorRaridade.Add(0.05f); // Lendária
    }

    void OnEnable()
    {
        AtualizarMoedas();
        btnGacha1x.onClick.AddListener(() => TentarGacha(1));
        btnGacha10x.onClick.AddListener(() => TentarGacha(10));
        btnOk.onClick.AddListener(OnOkClick);
        btnOk.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        btnGacha1x.onClick.RemoveAllListeners();
        btnGacha10x.onClick.RemoveAllListeners();
        btnOk.onClick.RemoveAllListeners();
        ClearResult();
    }

    void AtualizarMoedas()
    {
        moedas = PlayerPrefs.GetInt("Moedas", 0);
        txtMoedas.text = moedas.ToString();
    }

    void TentarGacha(int quantidade)
    {
        int preco = quantidade == 1 ? preco1x : preco10x;
        if (moedas < preco)
        {
            Debug.Log("Moedas insuficientes!");
            //return;
        }
        // Não reduz as moedas em teste para facilitar o desenvolvimento, mas aqui é onde você faria isso:
        // moedas -= preco;
        // PlayerPrefs.SetInt("Moedas", moedas);
        AtualizarMoedas();
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
        btnGacha10x.interactable = false;
        foreach (Transform filho in resultadoArea) Destroy(filho.gameObject);
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
        
    }
    void OnCardClicked(CardUI cardUI)
    {

        cardViewPanel.SetActive(true);
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
        btnGacha10x.interactable = true;
    }

}
