using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RouletteController : MonoBehaviour
{
    public RectTransform roulette; // Objeto da roleta (UI)
    public float spinDuration = 4f; // Duração do giro
    public float spinSpeed = 500f; // Velocidade inicial do giro
    public float idleSpinSpeed = 30f; // Velocidade lenta quando não usada

    private bool isSpinning = false;
    private bool finishedSpinning = false;
    public Text resultText; // Texto para mostrar o resultado

    void Update()
    {
        // 🔹 Gira lentamente quando não está sendo usada
        if (!isSpinning && !finishedSpinning && roulette != null)
        {
            roulette.Rotate(0, 0, -idleSpinSpeed * Time.deltaTime);
        }
    }

    public void StartRoulette()
    {
        spinDuration = UnityEngine.Random.Range(3f, 5f);
        spinSpeed = UnityEngine.Random.Range(500f, 800f);
        if (!isSpinning)
            StartCoroutine(SpinRoulette());
    }

    private IEnumerator SpinRoulette()
    {
        isSpinning = true;

        float elapsed = 0f;
        float currentSpeed = spinSpeed;

        while (elapsed < spinDuration)
        {
            float rotation = currentSpeed * Time.deltaTime;
            roulette.Rotate(0, 0, -rotation); // Gira no sentido horário

            elapsed += Time.deltaTime;
            currentSpeed = Mathf.Lerp(spinSpeed, 0, elapsed / spinDuration);

            yield return null;
        }

        // Normaliza o ângulo entre 0 e 360
        float finalZ = roulette.eulerAngles.z % 360;

        DecideWinner(finalZ);

        isSpinning = false;
        finishedSpinning = true;
    }

    private void DecideWinner(float angle)
    {
        // Cada setor tem 90°
        // Setores: 0°–90° = Azul | 90°–180° = Vermelho | 180°–270° = Azul | 270°–360° = Vermelho

        if (angle >= 0 && angle < 90)
        {
            Debug.Log("Vermelho → Inimigo começa!");
            resultText.text = "Inimigo começa!";
            BattleCardScreen.Instance.StartEnemyTurn();
        }
        else if (angle >= 90 && angle < 180)
        {
            resultText.text = "Player começa!";
            Debug.Log("Azul → Player começa!");
            BattleCardScreen.Instance.StartPlayerTurn();
        }
        else if (angle >= 180 && angle < 270)
        {
            resultText.text = "Inimigo começa!";
            Debug.Log("Vermelho → Inimigo começa!");
            BattleCardScreen.Instance.StartEnemyTurn();
        }
        else
        {
            resultText.text = "Player começa!";
            Debug.Log("Azul → Player começa!");
            BattleCardScreen.Instance.StartPlayerTurn();
        }

        // ✅ Ativa as cartas
        DraggableCard.CanDrag = true;

        // ✅ Destroi a roleta
        Destroy(gameObject, 1f);
    }
}
