using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
    public static CardDealer Instance;
    public GameObject cardPrefab;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Distribui cartas alternadamente para Player e Enemy com delay.
    /// </summary>
    public IEnumerator DealCards(List<CardData> deck, List<CardData> playerHand, List<CardData> enemyHand,
                                Transform playerHandArea, Transform enemyHandArea, GameObject cardPrefab)
    {
        // embaralhar deck
        List<CardData> shuffledDeck = new List<CardData>(deck);
        Shuffle(shuffledDeck);

        // pega as 10 primeiras cartas (5 para cada)
        for (int i = 0; i < 5; i++)
        {
            // Player recebe carta
            CardData pCard = shuffledDeck[i];
            playerHand.Add(pCard);
            CreateCard(pCard, playerHandArea, Owner.Player);

            yield return new WaitForSeconds(0.3f);

            // Enemy recebe carta
            CardData eCard = shuffledDeck[i + 5];
            enemyHand.Add(eCard);
            CreateCard(eCard, enemyHandArea, Owner.Enemy);

            yield return new WaitForSeconds(0.3f);
        }

        // desativa drag até começar o turno real
        DraggableCard.CanDrag = false;

        Debug.Log("Distribuição de cartas concluída.");
        yield return new WaitForSeconds(0.5f);
        // 🔹 Agora que TODAS entraram, faz o flip de todas
        foreach (Transform playerCard in playerHandArea.transform)
        {
            var flip = playerCard.GetComponent<CardFlip>();
            if (flip != null) flip.FlipCard(Owner.Player);
        }
        foreach (Transform enemyCard in enemyHandArea.transform)
        {
            var flip = enemyCard.GetComponent<CardFlip>();
            if (flip != null) flip.FlipCard(Owner.Enemy);
        }
        
    }

    void CreateCard(CardData cardData, Transform parent, Owner owner)
    {
        var cardObj = Instantiate(cardPrefab, parent);
        var cardUI = cardObj.GetComponent<CardUI>();
        cardUI.SetCard(cardData, owner);

        // comportamento diferente entre Player e Enemy
        if (owner == Owner.Player)
        {
            var drag = cardObj.AddComponent<DraggableCard>();
            drag.OnCardPlaced += GameManager.Instance.OnPlayerCardPlaced;
            //cardObj.GetComponent<CardFlip>().FlipCard(Owner.Player);
        }
        else
        {
            var drag = cardObj.GetComponent<DraggableCard>();
            if (drag != null) drag.enabled = false;
            //cardObj.GetComponent<CardFlip>().FlipCard(Owner.Enemy);
        }

        // animação de entrada
        StartCoroutine(AnimateCard(cardObj, parent));
        Debug.Log($"Carta {cardData.cardName} criada para {owner}");
    }

    IEnumerator AnimateCard(GameObject card, Transform handParent)
    {
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null) cg = card.AddComponent<CanvasGroup>();

        RectTransform rt = card.GetComponent<RectTransform>();

        // Temporário: colocar fora do layout
        Transform originalParent = card.transform.parent;
        card.transform.SetParent(handParent.parent, true); // fora do VerticalLayoutGroup

        cg.alpha = 0f;

        // Posição inicial (fora da tela, parte inferior)
        Vector2 startPos = new Vector2(0, -Screen.height);
        Vector2 endPos = handParent.position; // alvo = posição da mão

        rt.position = startPos;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f; // 0.5s
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            rt.position = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Garante posição final
        cg.alpha = 1f;

        // Agora sim: volta pro layout
        card.transform.SetParent(handParent, false);
        rt.anchoredPosition = Vector2.zero; // LayoutGroup organiza certinho
    }

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
