using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class CardStealUIManager : MonoBehaviour
{
    [Header("Referências de UI")]
    public Transform stealCardContainer;
    public GameObject cardPrefab; // Prefab simples de botão de carta
    public GameObject confirmModal;
    public TMP_Text confirmText;
    public TMP_Text titleText;
    public Button confirmYesButton;
    public Button confirmNoButton;
    public Button ButtonTakeCoins;

    [Header("Efeito de Moedas")]
    public Image coinImagePrefab; // Prefab de uma moeda (Image)
    public Sprite coinSprite; // Sprite da moeda
    public AudioClip coinCollectSound; // Som de coleta de moeda
    public float coinAnimationDuration = 1.5f;
    public float coinSpreadRadius = 100f; // Raio de dispersão das moedas

    [Header("Dados")]
    private List<CardData> playerCards = new List<CardData>();
    private List<CardData> enemyCards = new List<CardData>();
    List<CardData> cards; // cartas atualmente mostradas (inimigo ou jogador)
    private CardData selectedCard;
    private GameObject selectedCardGO;

    private bool playerWon;
    private bool isBoss;
    public static CardStealUIManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        ButtonTakeCoins.onClick.AddListener(() => TakeCoinsReward());
    }

    void OnEnable()
    {
        // Reseta variáveis
        selectedCard = null;
        selectedCardGO = null;
        ButtonTakeCoins.interactable = true;
    }

    // Inicialização da tela de roubo
    public void OpenStealScreen(List<CardData> player, List<CardData> enemy, bool playerWon, bool isBoss)
    {
        gameObject.SetActive(true);

        this.playerCards = player;
        this.enemyCards = enemy;
        this.playerWon = playerWon;
        this.isBoss = isBoss;
        confirmModal.SetActive(false);
        ClearContainers();
        PopulateCards(playerWon);

        if (playerWon)
        {
            titleText.text = "Select 1 card you want!";
            ButtonTakeCoins.gameObject.SetActive(true);
        }
        else
        {
            ButtonTakeCoins.gameObject.SetActive(false);
            titleText.text = "You lost a card to the enemy!";
            // Derrota → inimigo rouba automaticamente
            Invoke(nameof(EnemyStealsCard), 2.5f);
        }
    }

    private void ClearContainers()
    {
        foreach (Transform child in stealCardContainer) Destroy(child.gameObject);
    }

    private void PopulateCards(bool playerWon)
    {
        cards = playerWon ? enemyCards : playerCards; //se o jogador venceu, mostra as cartas inimigas

        foreach (var card in cards)
        {

            GameObject cardIstance = GameObject.Instantiate(cardPrefab, stealCardContainer);
            CardUI cardUI = cardIstance.GetComponent<CardUI>();


            if (playerWon) // se o jogador venceu, pode clicar nas cartas inimigas
            {
                cardUI.SetCard(card, Owner.Enemy);
                cardUI.ShowBack(); // mostra o verso da carta
                cardIstance.AddComponent<Button>();
                Button btn = cardIstance.GetComponent<Button>();
                btn.onClick.AddListener(() => OnEnemyCardClicked(card, cardIstance));
            }
            else
            {

                cardUI.SetCard(card, Owner.Player);
            }
        }
    }

    private void OnEnemyCardClicked(CardData card, GameObject go)
    {
        // Se o modal de confirmação já está ativo, ignora cliques adicionais
        if (confirmModal != null && confirmModal.activeSelf) return;

        selectedCard = card;
        selectedCardGO = go;

        // animação simples: mover um pouco para cima
        selectedCardGO.transform.localPosition += Vector3.up * 40f;

        confirmModal.SetActive(true);
        confirmText.text = "Select a card from your opponent to steal!";

        confirmYesButton.onClick.RemoveAllListeners();
        confirmYesButton.onClick.AddListener(() => ConfirmSteal());

        confirmNoButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.AddListener(() => CancelSteal());
    }


    private void ConfirmSteal()
    {
        ButtonTakeCoins.interactable = false;
        Debug.Log($"Jogador roubou: {selectedCard.cardName} ({selectedCard.rarity})");
        // Faz um flip na carta para mostrar a frente
        CardUI cardUI = selectedCardGO.GetComponent<CardUI>();
        cardUI.ShowFront();
        // Salvar na coleção oficial
        PlayerDeckManager.AddCardToCollection(selectedCard.id);

        confirmModal.SetActive(false);
        StartCoroutine(AnimateStolenCard(selectedCardGO, true, () =>
         {
             //Desativa a tela de roubo
             gameObject.SetActive(false);
         }));
    }

    private void CancelSteal()
    {
        // Volta a carta para posição original
        if (selectedCardGO != null)
            selectedCardGO.transform.localPosition -= Vector3.up * 40f;

        confirmModal.SetActive(false);
        selectedCard = null;
        selectedCardGO = null;
    }

    private void TakeCoinsReward()
    {
        Debug.Log("Jogador recebeu 10 moedas");
        GameManager.Instance.AddCoins(10);
        // Iniciar animação de moedas
        StartCoroutine(AnimateCoinsFlyUp(10));
    }

    private IEnumerator AnimateCoinsFlyUp(int coinCount)
    {
        Canvas canvas = ButtonTakeCoins.GetComponentInParent<Canvas>();
        RectTransform buttonRect = ButtonTakeCoins.GetComponent<RectTransform>();
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

        if (canvas == null)
        {
            Debug.LogError("Canvas não encontrado!");
            yield break;
        }

        // Desabilitar interação do botão
        ButtonTakeCoins.interactable = false;

        for (int i = 0; i < coinCount; i++)
        {
            // Criar uma imagem de moeda simples se não tiver prefab
            Image coinImage = Instantiate(coinImagePrefab, canvas.transform);
            AudioManager.Instance.PlaySFX("coin_collect");

            RectTransform coinRect = coinImage.GetComponent<RectTransform>();
            coinRect.anchoredPosition = buttonRect.anchoredPosition; // Usar anchoredPosition no lugar de position
            //coinRect.sizeDelta = new Vector2(40, 40);

            // Adicionar CanvasGroup se não tiver
            CanvasGroup canvasGroup = coinImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = coinImage.gameObject.AddComponent<CanvasGroup>();
            }

            // Elevar acima de outros elementos
            coinImage.transform.SetAsLastSibling();

            Debug.Log($"Moeda {i} criada em: {coinRect.anchoredPosition}");

            // Calcular direção aleatória de dispersão
            float angle = (360f / coinCount) * i + Random.Range(-15f, 15f);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            Vector2 spreadEnd = buttonRect.anchoredPosition + (direction * coinSpreadRadius);

            // Posição final no topo da tela (em coordenadas de canvas)
            Vector2 screenTopCenter = new Vector2(0, canvas.GetComponent<RectTransform>().rect.height / 2f + 50);

            // Animar moeda
            StartCoroutine(AnimateSingleCoin(coinRect, buttonRect.anchoredPosition, spreadEnd, screenTopCenter, coinAnimationDuration));


            // Pequeno delay entre cada moeda
            yield return new WaitForSeconds(0.05f);
        }

        // Esperar todas as moedas terminarem
        yield return new WaitForSeconds(coinAnimationDuration + 0.5f);

        // Fechar tela
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateSingleCoin(RectTransform coinRect, Vector2 startPos, Vector2 spreadTarget, Vector2 finalTarget, float duration)
    {
        // Fase 1: Espalhamento (primeiro 30% da animação)
        float spreadDuration = duration * 0.3f;
        float elapsed = 0f;

        while (elapsed < spreadDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spreadDuration;

            // Interpolação quadrática para efeito mais suave
            float easeOut = 1f - (1f - t) * (1f - t);
            coinRect.anchoredPosition = Vector2.Lerp(startPos, spreadTarget, easeOut);

            yield return null;
        }

        // Fase 2: Voo até o topo (70% da animação)
        float flyDuration = duration * 0.7f;
        elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;

            // Interpolação suave (ease-in)
            float easeIn = t * t;
            coinRect.anchoredPosition = Vector2.Lerp(spreadTarget, finalTarget, easeIn);

            // Fade out
            CanvasGroup canvasGroup = coinRect.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }

        // Destruir moeda
        Destroy(coinRect.gameObject);
    }


    private void EnemyStealsCard()
    {

        var playerCollection = PlayerDeckManager.GetOwnedCardData();
        if (playerCollection.Count <= 5)
        {
            Debug.Log("Jogador tem apenas 5 cartas ou menos, inimigo não rouba");
            gameObject.SetActive(false);
            return;
        }
        else
        {

            var playerDeck = PlayerDeckManager.GetDeckCardData();
            CardData stolen = isBoss
            ? WeightedRandomSteal(playerDeck)
            : playerDeck[Random.Range(0, playerDeck.Count)];

            // Remover da coleção (e do deck se estiver equipado)
            PlayerDeckManager.RemoveCardFromCollection(stolen.id);

            Debug.Log($"Inimigo roubou: {stolen.cardName} ({stolen.rarity})");


            GameObject stolenCard = cards.Find(c => c.id == stolen.id) != null
                ? stealCardContainer.GetChild(cards.IndexOf(stolen)).gameObject
                : GameObject.Instantiate(cardPrefab, stealCardContainer);
            StartCoroutine(AnimateStolenCard(stolenCard, false, () =>
            {
                gameObject.SetActive(false);
            }));

        }


    }


    private CardData WeightedRandomSteal(List<CardData> pool)
    {
        List<CardData> weighted = new List<CardData>();

        foreach (var card in pool)
        {
            int weight = 1;
            switch (card.rarity)
            {
                case CardRarity.Common: weight = 1; break;
                case CardRarity.Uncommon: weight = 2; break;
                case CardRarity.Rare: weight = 4; break;
                case CardRarity.Epic: weight = 6; break;
                case CardRarity.Legendary: weight = 8; break;
            }
            for (int i = 0; i < weight; i++)
                weighted.Add(card);
        }

        return weighted[Random.Range(0, weighted.Count)];
    }

    private IEnumerator AnimateStolenCard(GameObject cardGO, bool playerStole, System.Action onComplete)
    {
        RectTransform rect = cardGO.GetComponent<RectTransform>();
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

        // 🔹 Garante que fique acima de todos os elementos de UI
        cardGO.transform.SetParent(canvas.transform, true);
        cardGO.transform.SetAsLastSibling();

        Vector3 startPos = rect.position;

        // 🔹 Calcula posição central no espaço do Canvas
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform, screenCenter, uiCamera, out Vector3 worldCenterPos
        );

        float elapsed = 0f;
        float duration = 0.5f;

        Vector3 targetScale = Vector3.one * 2f;
        Vector3 originalScale = rect.localScale;

        // 🔹 Move até o centro e aumenta o tamanho
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.position = Vector3.Lerp(startPos, worldCenterPos, t);
            rect.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // 🔹 Pausa no centro
        yield return new WaitForSeconds(1.5f);

        // 🔹 Calcula posição final (fora da tela)
        elapsed = 0f;
        duration = 0.5f;

        Vector2 screenEnd = playerStole
            ? new Vector2(Screen.width / 2f, -Screen.height * 0.5f)   // jogador → para baixo
            : new Vector2(Screen.width / 2f, Screen.height * 1.5f);   // inimigo → para cima

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform, screenEnd, uiCamera, out Vector3 worldEndPos
        );

        // 🔹 Move para fora da tela
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.position = Vector3.Lerp(worldCenterPos, worldEndPos, t);
            yield return null;
        }

        onComplete?.Invoke();
        Destroy(cardGO);
    }



}
