using UnityEngine;
using UnityEngine.Events;
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

    private void Start()
    {
        SetupTestShortcuts();
        UpdateTicketsDisplay();
        UpdateCoinsDisplay();
    }

    private void OnEnable()
    {
        UpdateTicketsDisplay();
        UpdateCoinsDisplay();
    }

    private void SetupTestShortcuts()
    {
        AttachClickShortcut(txtCoins, AddCoinsShortcut);
        AttachClickShortcut(txtTickets, AddTicketsShortcut);
    }

    private void AddCoinsShortcut()
    {
        GameManager.Instance?.AddCoins(100);
        UpdateCoinsDisplay();
        Debug.Log("Atalho de teste: +100 moedas");
    }

    private void AddTicketsShortcut()
    {
        BattleTicketSystem.Instance?.AddTickets(10);
        UpdateTicketsDisplay();
        Debug.Log("Atalho de teste: +10 tickets");
    }

    private void AttachClickShortcut(TextMeshProUGUI target, UnityAction callback)
    {
        if (target == null)
        {
            return;
        }

        Button button = target.GetComponent<Button>();
        if (button == null)
        {
            button = target.gameObject.AddComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
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
