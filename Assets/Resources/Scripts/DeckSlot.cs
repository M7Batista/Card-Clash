using UnityEngine;
using UnityEngine.UI;

public class DeckSlot : MonoBehaviour
{
    public CardUI CurrentCard;      // card atual no slot
    //public GameObject arrowReplace; // seta de substituição (ícone na UI)

    public void SetCard(CardData data)
    {
        if (CurrentCard != null) Destroy(CurrentCard.gameObject);

        // cria card UI dentro do slot
        GameObject cardGO = Instantiate(DeckEditorUI.Instance.cardPrefab, transform);
        CurrentCard = cardGO.GetComponent<CardUI>();
        CurrentCard.SetCard(data, Owner.None);
    }

    public void ClearSlot()
    {
        if (CurrentCard != null)
        {
            Destroy(CurrentCard.gameObject);
            CurrentCard = null;
        }
    }

    /*public void ShowReplaceArrow(bool show)
    {
        if (arrowReplace != null)
            arrowReplace.SetActive(show);
    }*/
}
