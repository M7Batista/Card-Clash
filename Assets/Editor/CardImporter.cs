using UnityEngine;
using UnityEditor;
using System.IO;

public class CardImporter : EditorWindow
{
    private static string csvPath = "Assets/Resources/Files/cards.csv";
    private static string artworksFolder = "Assets/Resources/Art/Artworks/";

    [MenuItem("Tools/Card/Import Cards from CSV")]
    public static void ImportCards()
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError("CSV file not found at " + csvPath);
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);

        string cardsFolderPath = "Assets/Resources/ListCards";
        if (!Directory.Exists(cardsFolderPath))
        {
            Directory.CreateDirectory(cardsFolderPath);
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            if (values.Length < 7) continue; // Garante que a linha tem dados suficientes

            CardData card = ScriptableObject.CreateInstance<CardData>();

            card.id = int.Parse(values[0].Trim());
            
            card.rarity = (CardRarity)System.Enum.Parse(typeof(CardRarity), values[1].Trim());
            
            card.top = converteValues(values[2].Trim());
            card.right = converteValues(values[3].Trim());
            card.bottom = converteValues(values[4].Trim());
            card.left = converteValues(values[5].Trim());
            card.cardName = values[6].Trim();

            // --- NOVO: Carregando a imagem do Asset ---
            string artworkFileName = values[7].Trim(); // Pega o nome do arquivo da 8ª coluna (índice 7)
            string artworkPath = artworksFolder + artworkFileName + ".png"; // Constrói o caminho completo

            // Carrega o Sprite do disco. Isso só funciona no Editor!
            Sprite cardArtwork = AssetDatabase.LoadAssetAtPath<Sprite>(artworkPath);
            if (cardArtwork != null)
            {
                card.artwork = cardArtwork;
            }
            else
            {
                Debug.LogWarning("Artwork not found for card: " + card.cardName + " at path: " + artworkPath);
            }
            // --- FIM DA NOVA LÓGICA ---

            string assetPath = Path.Combine(cardsFolderPath, card.id + ".asset");
            AssetDatabase.CreateAsset(card, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Importação de cartas concluída!");
    }
    static int converteValues(string valueString)
    {
        int value = 0;
        if (valueString == "A")
        {
            value = 10;
        }
        else if (valueString == "B")
        {
            value = 11;
        }

        else
        {
            value = int.Parse(valueString);
        }
        return value;
    }
}