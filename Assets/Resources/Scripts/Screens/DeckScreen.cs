using UnityEngine;
using UnityEngine.UI;
public class DeckScreen : MonoBehaviour
{
    public Button buttonTabDeck;
    public Button buttonTabCollection;
    public GameObject tabDeck;
    public GameObject tabCollection;
    public GameObject tabSelected;
    void OnEnable()
    {
        tabSelected.transform.SetParent(buttonTabDeck.transform, false);
        tabDeck.SetActive(true);
        tabCollection.SetActive(false);

        buttonTabDeck.onClick.AddListener(OnClickTabDeck);
        buttonTabCollection.onClick.AddListener(OnClickTabCollection);
    }
    void OnDisable()
    {
        buttonTabDeck.onClick.RemoveListener(OnClickTabDeck);
        buttonTabCollection.onClick.RemoveListener(OnClickTabCollection);
    }
    void OnClickTabDeck()
    {
        // tabSelected é uma linha visual que indica a aba selecionada
        tabSelected.transform.SetParent(buttonTabDeck.transform, false);
        tabDeck.SetActive(true);
        tabCollection.SetActive(false);
    }
    void OnClickTabCollection()
    {
        tabSelected.transform.SetParent(buttonTabCollection.transform, false);
        tabDeck.SetActive(false);
        tabCollection.SetActive(true);
    }
}
