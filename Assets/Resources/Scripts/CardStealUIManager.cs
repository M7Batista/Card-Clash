using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public Button skipButtom;

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
        //gameObject.SetActive(false);
    }
    void Start()
    {
         skipButtom.onClick.AddListener(() => EndStealScreen());
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
            skipButtom.gameObject.SetActive(true);
        }
        else
        {
            skipButtom.gameObject.SetActive(false);
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
        //confirmText.text = $"Deseja roubar a carta \"{card.cardName}\" ({card.rarity})?";
        confirmText.text = $"Do you want to get the \"{card.cardName}\" ({card.rarity}) card?";

        confirmYesButton.onClick.RemoveAllListeners();
        confirmYesButton.onClick.AddListener(() => ConfirmSteal());

        confirmNoButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.AddListener(() => CancelSteal());
    }


    private void ConfirmSteal()
    {
        Debug.Log($"Jogador roubou: {selectedCard.cardName} ({selectedCard.rarity})");

        // Salvar na coleção oficial
        PlayerDeckManager.AddCardToCollection(selectedCard.id);

        confirmModal.SetActive(false);
        StartCoroutine(AnimateStolenCard(selectedCardGO, true, () =>
         {
             EndStealScreen();
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


    private void EnemyStealsCard()
    {

        var playerCollection = PlayerDeckManager.GetOwnedCardData();
        if (playerCollection.Count <= 5)
        {
            Debug.Log("Jogador tem apenas 5 cartas ou menos, inimigo não rouba");
            EndStealScreen();
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
                EndStealScreen();
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

    private void EndStealScreen()
    {
        BattleCardScreen.Instance.OnScreenClosed();

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
        yield return new WaitForSeconds(1f);

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
