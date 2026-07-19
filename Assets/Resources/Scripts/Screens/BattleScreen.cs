using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleScreen : MonoBehaviour
{
    public Sprite[] spriteStars; // 0= estrela vazia, 1= estrela cheia
    public GameObject[] stars; // 0= estrela 0, 1= estrela 1, 2= estrela 2

    public Sprite[] spriteRanks; // 0=bronze, 1=prata, 2=ouro, 3=platina, 4=diamante, 5=mestre, 6=grão-mestre, 7=lendario
    public GameObject rankDisplay; // Objeto que contém o sprite do rank atual

    public TextMeshProUGUI txtRankName; // Texto que exibe o nome do rank atual

    void OnEnable()
    {
        SetupRankDisplayShortcut();
        UpdateStarsDisplay();
        UpdateRankDisplay();
        UpdateRankNameDisplay();
    }

    private void SetupRankDisplayShortcut()
    {
        if (rankDisplay == null)
        {
            Debug.LogWarning("rankDisplay não configurado corretamente em BattleScreen");
            return;
        }

        Button rankButton = rankDisplay.GetComponent<Button>();
        if (rankButton == null)
        {
            rankButton = rankDisplay.AddComponent<Button>();
        }

        rankButton.onClick.RemoveAllListeners();
        rankButton.onClick.AddListener(HandleRankDisplayClick);
    }

    public void HandleRankDisplayClick()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance não foi inicializado");
            return;
        }

        GameManager.Instance.AddRankWin();
        Debug.Log("Atalho de teste: vitória adicionada via clique no rank");

        UpdateStarsDisplay();
        UpdateRankDisplay();
        UpdateRankNameDisplay();
    }

    // Exibe o nome do rank atual no texto
    public void UpdateRankNameDisplay()
    {
        // Valida GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance não foi inicializado");
            return;
        }

        if (txtRankName == null)
        {
            Debug.LogWarning("txtRankName não configurado corretamente em BattleScreen");
            return;
        }

        // Obtém o rank atual
        string currentRank = RankSystem.GetPlayerRankName();
        txtRankName.text = currentRank;
    }

    /// <summary>
    /// Atualiza os sprites das estrelas de acordo com o número de vitórias para subir de rank
    /// </summary>
    public void UpdateStarsDisplay()
    {
        // Valida GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance não foi inicializado");
            return;
        }

        if (stars == null || stars.Length == 0 || spriteStars == null || spriteStars.Length < 2)
        {
            Debug.LogWarning("Stars ou spriteStars não configurados corretamente em BattleScreen");
            return;
        }

        // Obtém o rank atual e o número de vitórias
        string currentRank = RankSystem.GetPlayerRankName();
        RankInfo? rankInfoNullable = RankSystem.GetRankInfo(currentRank);
        
        if (!rankInfoNullable.HasValue)
        {
            Debug.LogWarning($"Rank '{currentRank}' não encontrado no sistema");
            return;
        }

        RankInfo rankInfo = rankInfoNullable.Value;
        int currentWins = GameManager.Instance.GetRankWins();

        int winsToPromote = rankInfo.winsToPromote;

        // Log do rank atual e vitórias
        Debug.Log($"Rank atual: {currentRank}, Vitórias: {currentWins}/{winsToPromote}");

        // Número de estrelas preenchidas: cada vitória preenche uma estrela,
        // limitado ao número de estrelas visíveis. Ex: 0->0,1->1,2->2,3->3,4->3.
        int starsFilled = Mathf.Clamp(currentWins, 0, stars.Length);

        // Atualiza cada estrela
        for (int i = 0; i < stars.Length; i++)
        {
            bool isFull = i < starsFilled;
            int spriteIndex = isFull ? 1 : 0;

            Image starImage = stars[i].GetComponent<Image>();
            if (starImage != null)
            {
                starImage.sprite = spriteStars[spriteIndex];
            }
        }
    }
    
    // 0=bronze, 1=prata, 2=ouro, 3=platina, 4=diamante, 5=mestre, 6=grão-mestre, 7=lendario
    public void UpdateRankDisplay()
    {
        // Valida GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance não foi inicializado");
            return;
        }

        if (spriteRanks == null || spriteRanks.Length == 0 || rankDisplay == null)
        {
            Debug.LogWarning("spriteRanks ou rankDisplay não configurados corretamente em BattleScreen");
            return;
        }

        // Obtém o rank atual
        string currentRank = RankSystem.GetPlayerRankName();
        RankInfo? rankInfoNullable = RankSystem.GetRankInfo(currentRank);
        
        if (!rankInfoNullable.HasValue)
        {
            Debug.LogWarning($"Rank '{currentRank}' não encontrado no sistema");
            return;
        }

        RankInfo rankInfo = rankInfoNullable.Value;

        // Atualiza o sprite do rank usando o índice da liga (Bronze/Prata/etc.)
        Image rankImage = rankDisplay.GetComponent<Image>();
        if (rankImage != null)
        {
            int rankIndex = RankSystem.GetLeagueIndex(currentRank);
            rankIndex = Mathf.Clamp(rankIndex, 0, spriteRanks.Length - 1);
            rankImage.sprite = spriteRanks[rankIndex];
        }
    }


}
