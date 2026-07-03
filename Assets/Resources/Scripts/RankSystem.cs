using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public struct RankInfo
{
    public int order;
    public string csvLeague;
    public string league;
    public string division;
    public string csvFullName;
    public string rankName;
    public int winsToPromote;
}

[Serializable]
public struct RankRarityConfig
{
    public string rankName;
    public float common;
    public float uncommon;
    public float rare;
    public float epic;
    public float legendary;
    public int maxLegendary;
    public int maxEpic;
    public int maxRare;
}

public static class RankSystem
{
    private const string PLAYER_RANK_KEY = "PlayerRank";
    private static bool initialized = false;
    private static readonly Dictionary<string, string> leagueMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Bronze", "Bronze" },
        { "Prata", "Silver" },
        { "Ouro", "Gold" },
        { "Platina", "Platinum" },
        { "Diamante", "Diamond" },
        { "Mestre", "Master" },
        { "Grão-Mestre", "Grandmaster" },
        { "Grão Mestre", "Grandmaster" },
        { "Lendário", "Legendary" },
        { "Lendario", "Legendary" }
    };

    private static readonly List<RankInfo> rankList = new List<RankInfo>();
    private static readonly Dictionary<string, RankInfo> rankByName = new Dictionary<string, RankInfo>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, RankRarityConfig> rarityTable = new Dictionary<string, RankRarityConfig>(StringComparer.OrdinalIgnoreCase);

    private static void EnsureInitialized()
    {
        if (initialized) return;
        LoadRankDefinitions();
        LoadRarityTable();
        initialized = true;
    }

    public static string GetDefaultRankName()
    {
        EnsureInitialized();
        if (rankList.Count == 0) return "Bronze V";
        return rankList[0].rankName;
    }

    public static string GetPlayerRankName()
    {
        EnsureInitialized();
        if (!PlayerPrefs.HasKey(PLAYER_RANK_KEY))
        {
            SetPlayerRankName(GetDefaultRankName());
            return GetDefaultRankName();
        }

        string stored = PlayerPrefs.GetString(PLAYER_RANK_KEY);
        if (string.IsNullOrWhiteSpace(stored))
        {
            SetPlayerRankName(GetDefaultRankName());
            return GetDefaultRankName();
        }

        string normalized = NormalizeRankName(stored);
        if (rankByName.ContainsKey(normalized))
            return normalized;

        SetPlayerRankName(GetDefaultRankName());
        return GetDefaultRankName();
    }

    public static void SetPlayerRankName(string rankName)
    {
        EnsureInitialized();
        string normalized = NormalizeRankName(rankName);
        if (!rankByName.ContainsKey(normalized))
            normalized = GetDefaultRankName();

        PlayerPrefs.SetString(PLAYER_RANK_KEY, normalized);
        PlayerPrefs.Save();
    }

    // Avança o jogador para o próximo rank disponível (se houver)
    public static void PromotePlayerRank()
    {
        EnsureInitialized();
        string current = GetPlayerRankName();
        if (string.IsNullOrEmpty(current))
            return;

        // Encontra o índice do rank atual na lista ordenada
        int index = rankList.FindIndex(r => string.Equals(r.rankName, current, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return;

        if (index >= rankList.Count - 1)
        {
            Debug.Log("RankSystem: jogador já está no rank máximo.");
            return;
        }

        string nextRank = rankList[index + 1].rankName;
        SetPlayerRankName(nextRank);
        Debug.Log($"RankSystem: jogador promovido de '{current}' para '{nextRank}'.");
    }

    // Retorna a posição zero-based do rank na lista ordenada (0 = primeiro rank carregado)
    public static int GetRankPosition(string rankName)
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(rankName))
            return 0;

        string normalized = NormalizeRankName(rankName);
        int idx = rankList.FindIndex(r => string.Equals(r.rankName, normalized, StringComparison.OrdinalIgnoreCase));
        return idx >= 0 ? idx : 0;
    }

    // Retorna o índice da liga (league) para mapear sprites por categoria de liga.
    // Exemplo: Bronze=0, Silver=1, Gold=2, Platinum=3, Diamond=4, Master=5, Grandmaster=6, Legendary=7
    public static int GetLeagueIndex(string rankName)
    {
        EnsureInitialized();
        string league = "Bronze";
        RankInfo? info = GetRankInfo(rankName);
        if (info.HasValue)
            league = info.Value.league ?? "Bronze";

        string[] leagueOrder = new[] { "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master", "Grandmaster", "Legendary" };
        int idx = Array.IndexOf(leagueOrder, league);
        return idx >= 0 ? idx : 0;
    }

    public static RankInfo? GetRankInfo(string rankName)
    {
        EnsureInitialized();
        if (rankName == null) return null;
        string normalized = NormalizeRankName(rankName);
        if (rankByName.TryGetValue(normalized, out var info))
            return info;

        return null;
    }

    public static RankRarityConfig GetRarityConfig(string rankName)
    {
        EnsureInitialized();
        if (rankName == null) return default;
        string normalized = NormalizeRankName(rankName);
        if (rarityTable.TryGetValue(normalized, out var config))
            return config;

        return rarityTable.ContainsKey(GetDefaultRankName()) ? rarityTable[GetDefaultRankName()] : default;
    }

    private static void LoadRankDefinitions()
    {
        rankList.Clear();
        rankByName.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>("Files/rank_system");
        if (csvFile == null)
        {
            Debug.LogWarning("RankSystem: rank_system.csv not found in Resources/Files.");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        bool firstLine = true;
        foreach (string rawLine in lines)
        {
            if (firstLine)
            {
                firstLine = false;
                continue;
            }

            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] values = line.Split(',');
            if (values.Length < 4)
                continue;

            if (!int.TryParse(values[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int order))
                continue;

            string csvLeague = values[1].Trim();
            string division = values[2].Trim();
            string csvFullName = values[3].Trim();
            int winsToPromote = 0;

            if (values.Length >= 5 && int.TryParse(values[4].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedWins))
                winsToPromote = parsedWins;

            string englishLeague = TranslateLeague(csvLeague);
            string rankName = englishLeague;
            if (!string.IsNullOrWhiteSpace(division) && division != "—")
                rankName = $"{englishLeague} {division}";

            RankInfo info = new RankInfo
            {
                order = order,
                csvLeague = csvLeague,
                league = englishLeague,
                division = division,
                csvFullName = csvFullName,
                rankName = rankName,
                winsToPromote = winsToPromote
            };

            rankList.Add(info);
            if (!rankByName.ContainsKey(rankName))
                rankByName[rankName] = info;
        }

        if (rankList.Count == 0)
            Debug.LogWarning("RankSystem: no rank definitions were loaded.");
    }

    private static void LoadRarityTable()
    {
        rarityTable.Clear();

        TextAsset csvFile = Resources.Load<TextAsset>("Files/table_rarity");
        if (csvFile == null)
        {
            Debug.LogWarning("RankSystem: table_rarity.csv not found in Resources/Files.");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        bool firstLine = true;
        foreach (string rawLine in lines)
        {
            if (firstLine)
            {
                firstLine = false;
                continue;
            }

            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] values = line.Split(',');
            if (values.Length < 9)
                continue;

            string rankValue = values[0].Trim();
            string rankName = NormalizeRankName(rankValue);
            if (string.IsNullOrWhiteSpace(rankName))
                continue;

            float common = ParseFloat(values[1]);
            float uncommon = ParseFloat(values[2]);
            float rare = ParseFloat(values[3]);
            float epic = ParseFloat(values[4]);
            float legendary = ParseFloat(values[5]);
            int maxLegendary = ParseInt(values[6]);
            int maxEpic = ParseInt(values[7]);
            int maxRare = ParseInt(values[8]);

            RankRarityConfig config = new RankRarityConfig
            {
                rankName = rankName,
                common = common,
                uncommon = uncommon,
                rare = rare,
                epic = epic,
                legendary = legendary,
                maxLegendary = maxLegendary,
                maxEpic = maxEpic,
                maxRare = maxRare
            };

            rarityTable[rankName] = config;
        }

        if (rarityTable.Count == 0)
            Debug.LogWarning("RankSystem: no rarity table entries were loaded.");
    }

    private static string TranslateLeague(string league)
    {
        if (string.IsNullOrWhiteSpace(league))
            return league;

        if (leagueMap.TryGetValue(league.Trim(), out var translated))
            return translated;

        return league.Trim();
    }

    public static string NormalizeRankName(string rankName)
    {
        if (string.IsNullOrWhiteSpace(rankName))
            return rankName;

        string trimmed = rankName.Trim();
        string[] parts = trimmed.Split(' ');
        if (parts.Length == 0)
            return trimmed;

        string leaguePart = parts[0];
        string mapped = TranslateLeague(leaguePart);
        if (parts.Length == 1)
            return mapped;

        string suffix = string.Join(" ", parts, 1, parts.Length - 1);
        return string.IsNullOrWhiteSpace(suffix) ? mapped : $"{mapped} {suffix}";
    }

    private static float ParseFloat(string text)
    {
        if (float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            return value;
        return 0f;
    }

    private static int ParseInt(string text)
    {
        if (int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            return value;
        return 0;
    }
}
