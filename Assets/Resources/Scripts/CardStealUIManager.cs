using System.Collections.Generic;
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
    public Button confirmYesButton;
    public Button confirmNoButton;

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

    // Inicialização da tela de roubo
    public void OpenStealScreen(List<CardData> player, List<CardData> enemy, bool playerWon, bool isBoss)
    {
        Debug.Log("Abrindo tela de roubo de cartas");
        Debug.Log(player.Count + " cartas do jogador");
        Debug.Log(enemy.Count + " cartas do inimigo");
        gameObject.SetActive(true);

        this.playerCards = player;
        this.enemyCards = enemy;
        this.playerWon = playerWon;
        this.isBoss = isBoss;
        confirmModal.SetActive(false);
        ClearContainers();
        PopulateCards(playerWon);



        if (!playerWon)
        {
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
            Debug.Log($"Mostrando carta: {card.cardName} ({card.rarity})");

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
        selectedCard = card;
        selectedCardGO = go;

        // animação simples: mover um pouco para cima
        selectedCardGO.transform.localPosition += Vector3.up * 20f;

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
            selectedCardGO.transform.localPosition -= Vector3.up * 20f;

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
            StartCoroutine(AnimateStolenCard(stolenCard, true, () =>
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

    private System.Collections.IEnumerator AnimateStolenCard(GameObject cardGO, bool playerStole, System.Action onComplete)

{
    RectTransform rect = cardGO.GetComponent<RectTransform>();

    // 🔹 Garante que fique acima de todos elementos de UI
    cardGO.transform.SetParent(transform, true); 
    cardGO.transform.SetAsLastSibling();

    Vector3 startPos = rect.position;
    Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

    float elapsed = 0f;
    float duration = 0.5f;

    Vector3 targetScale = Vector3.one * 2f;
    Vector3 originalScale = rect.localScale;

    // 🔹 Move até o centro e aumenta o tamanho
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        rect.position = Vector3.Lerp(startPos, centerPos, t);
        rect.localScale = Vector3.Lerp(originalScale, targetScale, t);
        yield return null;
    }

    // 🔹 Pausa 1 segundo no centro
    yield return new WaitForSeconds(1f);

    // 🔹 Move para fora da tela (cima ou baixo)
    elapsed = 0f;
    duration = 0.5f;
    Vector3 endPos = playerStole
        ? new Vector3(Screen.width / 2f, -Screen.height, 0f)  // jogador → vai para baixo
        : new Vector3(Screen.width / 2f, Screen.height * 2f, 0f); // inimigo → vai para cima

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        rect.position = Vector3.Lerp(centerPos, endPos, t);
        yield return null;
    }

    onComplete?.Invoke();

    // 🔹 Destroi a cópia temporária da carta
    Destroy(cardGO);
}


}
