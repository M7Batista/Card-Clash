using UnityEngine;
using UnityEngine.UI;

public class BattleScreen : MonoBehaviour
{
    public Sprite[] spriteStars; // 0= estrela vazia, 1= estrela cheia
    public GameObject[] stars; // 0= estrela 1, 1= estrela 2, 2= estrela 3
    void OnEnable()
    {
        Debug.Log("BattleScreen OnEnable");
        UpdateStarsDisplay();
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

        // Calcula a porcentagem de progress para cada estrela (máximo 3 estrelas)
        int winsToPromote = rankInfo.winsToPromote;
        int winsPerStar = Mathf.CeilToInt(winsToPromote / 3f);

        // Atualiza cada estrela
        for (int i = 0; i < stars.Length; i++)
        {
            int winsNeededForThisStar = (i + 1) * winsPerStar;

            // Determina se a estrela deve estar cheia ou vazia
            bool isFull = currentWins >= winsNeededForThisStar;

            // 0 = estrela vazia, 1 = estrela cheia
            int spriteIndex = isFull ? 1 : 0;
            
            Image starImage = stars[i].GetComponent<Image>();
            if (starImage != null)
            {
                starImage.sprite = spriteStars[spriteIndex];
            }
        }
    }


}
