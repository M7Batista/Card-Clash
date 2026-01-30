using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TopPanelController : MonoBehaviour
{
    public static TopPanelController Instance;
    public TextMeshProUGUI txtTickets, txtCoins;
    
    private void Awake()
    {
        Instance = this;
        
    }
    void Start()
    {
        UpdateTicketsDisplay();
        UpdateCoinsDisplay();
    }
    void OnEnable()
    {
        UpdateTicketsDisplay();
        UpdateCoinsDisplay();
    }
    public void UpdateTicketsDisplay()
    {
        if (txtTickets != null && BattleTicketSystem.Instance != null)
        {
            txtTickets.text = $"{BattleTicketSystem.Instance.GetCurrentTickets()}/30";
        }
    }


    public void UpdateCoinsDisplay()
    {

        if (txtCoins != null && GameManager.Instance != null)
        {
            txtCoins.text = $"{GameManager.Instance.coins}";
        }
    }
}
