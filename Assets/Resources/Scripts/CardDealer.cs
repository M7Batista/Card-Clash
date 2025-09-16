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
    /// Distribui as cartas do jogador (deck fixo) e inimigo para suas áreas na UI.
    /// </summary>
    public IEnumerator DealCards(
        List<CardData> playerDeck,
        List<CardData> enemyHand,
        Transform playerHandArea,
        Transform enemyHandArea,
        GameObject cardPrefab
    )
    {
        // 🔹 Distribui cartas do jogador
        for (int i = 0; i < playerDeck.Count; i++)
        {
            CardData card = playerDeck[i];
            GameObject cardIstance = GameObject.Instantiate(cardPrefab, playerHandArea);
            CardUI cardUI = cardIstance.GetComponent<CardUI>();
            cardUI.SetCard(card, Owner.Player);
            cardIstance.AddComponent<DraggableCard>(); //Adicione o drag
            StartCoroutine(AnimateCard(cardIstance, playerHandArea));
            yield return new WaitForSeconds(0.1f);
        }

        // 🔹 Distribui cartas do inimigo
        for (int i = 0; i < enemyHand.Count; i++)
        {
            CardData card = enemyHand[i];
            GameObject cardInstance = GameObject.Instantiate(cardPrefab, enemyHandArea);
            CardUI cardUI = cardInstance.GetComponent<CardUI>();
            cardUI.SetCard(card, Owner.Enemy);

            StartCoroutine(AnimateCard(cardInstance, enemyHandArea));
            yield return new WaitForSeconds(0.1f);
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
        Vector2 startPos = new Vector2(-Screen.width, Screen.height/2);
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