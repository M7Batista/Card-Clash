using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
    public static CardDealer Instance;
    public GameObject cardPrefab;

    [Header("Animação")]
    float dealDelay = 0.1f;       // tempo entre as cartas
    float animDuration = 0.6f;    // duração da animação
    float startOffsetX = -600f;   // posição inicial X fora da tela
    float startOffsetY = 200f;    // altura inicial fora da tela

    private Canvas mainCanvas;
    private Camera uiCamera;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas != null && mainCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            uiCamera = mainCanvas.worldCamera;
    }

    /// <summary>
    /// Distribui as cartas do jogador e inimigo com animação fluida e otimizada.
    /// </summary>
    public IEnumerator DealCards(
        List<CardData> playerDeck,
        List<CardData> enemyHand,
        Transform playerHandArea,
        Transform enemyHandArea,
        GameObject prefab = null
    )
    {
        if (prefab == null) prefab = cardPrefab;

        // 🔹 Pré-carrega todos os cards em listas (para reduzir Instantiate no meio da animação)
        List<GameObject> playerCards = new List<GameObject>();
        List<GameObject> enemyCards = new List<GameObject>();

        foreach (var card in playerDeck)
        {
            var cardGO = Instantiate(prefab, playerHandArea.parent); // cria fora do layout
            cardGO.SetActive(false);
            var ui = cardGO.GetComponent<CardUI>();
            ui.SetCard(card, Owner.Player);
            cardGO.AddComponent<DraggableCard>();
            playerCards.Add(cardGO);
        }

        foreach (var card in enemyHand)
        {
            var cardGO = Instantiate(prefab, enemyHandArea.parent);
            cardGO.SetActive(false);
            var ui = cardGO.GetComponent<CardUI>();
            ui.SetCard(card, Owner.Enemy);
            enemyCards.Add(cardGO);
        }

        // 🔹 Distribuição animada do jogador
        for (int i = 0; i < playerCards.Count; i++)
        {
            var cardGO = playerCards[i];
            cardGO.SetActive(true);
            StartCoroutine(AnimateCard(cardGO, playerHandArea));
            yield return new WaitForSeconds(dealDelay);
        }

        // 🔹 Distribuição animada do inimigo
        for (int i = 0; i < enemyCards.Count; i++)
        {
            var cardGO = enemyCards[i];
            cardGO.SetActive(true);
            StartCoroutine(AnimateCard(cardGO, enemyHandArea));
            yield return new WaitForSeconds(dealDelay);
        }

        DraggableCard.CanDrag = false;
        Debug.Log("✅ Distribuição de cartas concluída.");

        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// Anima a carta do ponto inicial até a mão do jogador/inimigo.
    /// </summary>
    private IEnumerator AnimateCard(GameObject card, Transform handParent)
    {
        if (mainCanvas == null)
        {
            mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas != null && mainCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                uiCamera = mainCanvas.worldCamera;
        }

        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null) cg = card.AddComponent<CanvasGroup>();

        RectTransform rt = card.GetComponent<RectTransform>();
        cg.alpha = 0f;

        // 🔹 Define posição inicial (fora da tela)
        Vector2 startScreenPos = new Vector2(
            Screen.width / 2f + startOffsetX,
            Screen.height / 2f + startOffsetY
        );

        Vector3 startWorldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            mainCanvas.transform as RectTransform,
            startScreenPos,
            uiCamera,
            out startWorldPos
        );

        rt.position = startWorldPos;

        Vector3 endPos = handParent.position;
        Vector3 originalScale = rt.localScale;

        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);

            rt.position = Vector3.Lerp(startWorldPos, endPos, t);
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            rt.localScale = Vector3.Lerp(originalScale * 0.7f, originalScale, t);

            yield return null;
        }

        // 🔹 Finaliza a animação
        rt.position = endPos;
        cg.alpha = 1f;
        rt.localScale = originalScale;

        // 🔹 Reparenta ao layout (apenas uma vez)
        card.transform.SetParent(handParent, false);
        rt.anchoredPosition = Vector2.zero;

        // 🔊 Som de distribuição
        AudioManager.Instance?.PlaySFX("card-slide-8");
    }

    /// <summary>
    /// Embaralha uma lista de cartas.
    /// </summary>
    void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}
