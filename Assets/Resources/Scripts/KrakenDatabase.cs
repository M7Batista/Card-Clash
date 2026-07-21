using UnityEngine;

public class KrakenDatabase : MonoBehaviour
{
    public static KrakenDatabase Instance;

    [Header("Kraken Expressions")]
    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite explaining;
    [SerializeField] private Sprite warning;
    [SerializeField] private Sprite happy;
    [SerializeField] private Sprite proud;
    [SerializeField] private Sprite cute;
    [SerializeField] private Sprite mischievous;
    [SerializeField] private Sprite afraid;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetExpression(KrakenExpression expression)
    {
        switch (expression)
        {
            case KrakenExpression.Explaining:
                return explaining;

            case KrakenExpression.Warning:
                return warning;

            case KrakenExpression.Happy:
                return happy;

            case KrakenExpression.Proud:
                return proud;

            case KrakenExpression.Cute:
                return cute;

            case KrakenExpression.Mischievous:
                return mischievous;

            case KrakenExpression.Afraid:
                return afraid;

            default:
                return normal;
        }
    }
}