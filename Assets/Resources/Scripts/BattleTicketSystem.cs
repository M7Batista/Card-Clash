using UnityEngine;
using System;

public class BattleTicketSystem : MonoBehaviour
{
    public static BattleTicketSystem Instance;

    [Header("Configurações de Tickets")]
    public int maxTickets = 30;
    public int dailyRecharge = 10;

    [Header("Estado Atual")]
    [SerializeField] private int currentTickets = 0;
    private DateTime lastRechargeDate;

    private const string KEY_TICKETS = "PLAYER_TICKETS";
    private const string KEY_LAST_RECHARGE = "LAST_RECHARGE_DATE";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadTickets();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ✅ Recupera ou inicializa os dados do jogador
    private void LoadTickets()
    {
        if (!PlayerPrefs.HasKey(KEY_TICKETS))
        {
            // Primeira vez jogando → começa com o máximo
            currentTickets = maxTickets;
            lastRechargeDate = DateTime.Now;
            SaveTickets();
        }
        else
        {
            currentTickets = PlayerPrefs.GetInt(KEY_TICKETS, maxTickets);
            string dateStr = PlayerPrefs.GetString(KEY_LAST_RECHARGE, DateTime.Now.ToString());
            lastRechargeDate = DateTime.Parse(dateStr);

            TryDailyRecharge();
        }
    }

    // ✅ Salva os dados
    private void SaveTickets()
    {
        PlayerPrefs.SetInt(KEY_TICKETS, currentTickets);
        PlayerPrefs.SetString(KEY_LAST_RECHARGE, lastRechargeDate.ToString());
        PlayerPrefs.Save();
    }

    // ✅ Tenta recarregar automaticamente 1x por dia
    private void TryDailyRecharge()
    {
        DateTime now = DateTime.Now;

        if (now.Date > lastRechargeDate.Date)
        {
            // Passou um novo dia
            currentTickets = Mathf.Min(currentTickets + dailyRecharge, maxTickets);
            lastRechargeDate = now;
            SaveTickets();

            Debug.Log($"Tickets recarregados automaticamente! ({currentTickets}/{maxTickets})");
        }
    }

    // ✅ Consome um ticket antes da batalha
    public bool ConsumeTicket()
    {
        TryDailyRecharge();

        if (currentTickets > 0)
        {
            currentTickets--;
            SaveTickets();
            return true;
        }
        else
        {
            Debug.Log("Sem tickets disponíveis!");
            return false;
        }
    }

    // ✅ Adiciona tickets (ex: recompensa, evento, anúncio)
    public void AddTickets(int amount)
    {
        currentTickets = Mathf.Min(currentTickets + amount, maxTickets);
        SaveTickets();
    }

    // ✅ Obtém valor atual
    public int GetCurrentTickets()
    {
        TryDailyRecharge();
        return currentTickets;
    }

    // ✅ Reseta para testes
    [ContextMenu("Reset Tickets")]
    public void ResetTickets()
    {
        PlayerPrefs.DeleteKey(KEY_TICKETS);
        PlayerPrefs.DeleteKey(KEY_LAST_RECHARGE);
        LoadTickets();
    }
}
