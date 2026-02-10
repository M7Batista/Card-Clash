using UnityEngine;

public class PuzzleCompletionController : MonoBehaviour
{
    public static PuzzleCompletionController Instance;

    private PuzzlePiece[] allPieces;
    private int fixedCount;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        allPieces = FindObjectsByType<PuzzlePiece>(FindObjectsSortMode.None);
        fixedCount = 0;
    }

    public void OnPieceFixed()
    {
        fixedCount++;

        if (fixedCount == allPieces.Length)
        {
            PuzzleCompleted();
        }
    }

    void PuzzleCompleted()
    {
        Debug.Log("🎉 PUZZLE COMPLETO!");

        // Aqui você pode:
        // - bloquear input
        // - tocar som
        // - animar
        // - salvar progresso
        // - mostrar popup
    }
}
