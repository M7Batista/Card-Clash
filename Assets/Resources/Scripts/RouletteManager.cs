using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class RouletteManager : MonoBehaviour
{
    public RectTransform roulette; // Objeto da roleta (UI)
    public float spinDuration = 4f; // Duração do giro
    public float spinSpeed = 500f; // Velocidade inicial do giro
    private bool isSpinning = false;
    public TextMeshProUGUI resultText; // Texto para mostrar o resultado

    void Start()
    {
        // Inicializa o texto do resultado
        if (resultText != null)
        {
            resultText.text = "";
        }
        StartCoroutine(AnimateRouletteAppearance());
    }

    private IEnumerator AnimateRouletteAppearance()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Estado inicial
        roulette.localScale = Vector3.zero;
        roulette.localRotation = Quaternion.Euler(0, 0, 45f); // Rotaciona 45°
        canvasGroup.alpha = 0f;

        float duration = 0.6f;
        float elapsed = 0f;
        
        // Bounce easing: começar rápido e desacelerar com bounce
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Bounce easing function
            float bounce = BounceEaseOut(t);
            
            // Scale: de 0 para 1 com bounce
            roulette.localScale = Vector3.one * bounce;
            
            // Fade in suave
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            
            // Rotação suave de volta (45° para 0°)
            roulette.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(45f, 0f, t));
            
            yield return null;
        }
        
        // Estado final
        roulette.localScale = Vector3.one;
        roulette.localRotation = Quaternion.identity;
        canvasGroup.alpha = 1f;
        
        // Aguarda 1 segundo antes de começar a girar
        yield return new WaitForSeconds(0.5f);
        
        StartRoulette();
    }

    private float BounceEaseOut(float t)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;
        
        if (t < 1f / d1)
            return n1 * t * t;
        else if (t < 2f / d1)
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        else if (t < 2.5f / d1)
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        else
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }

    public void StartRoulette()
    {
        spinDuration = UnityEngine.Random.Range(2f, 3f);
        spinSpeed = UnityEngine.Random.Range(500f, 800f);
        if (!isSpinning)
            StartCoroutine(SpinRoulette());
    }

    private IEnumerator SpinRoulette()
    {
        isSpinning = true;

        float elapsed = 0f;
        float currentSpeed = 0f;
        float accelerationDuration = spinDuration * 0.4f; // Acelera durante 40% da duração total

        while (elapsed < spinDuration)
        {
            // Calcula a velocidade com aceleração gradativa seguida de desaceleração
            if (elapsed < accelerationDuration)
            {
                // Fase de aceleração: começa em 0 e aumenta para spinSpeed
                float accelerationProgress = elapsed / accelerationDuration;
                currentSpeed = Mathf.Lerp(0f, spinSpeed, accelerationProgress);
            }
            else
            {
                // Fase de desaceleração: reduz spinSpeed para 0
                float decelerationProgress = (elapsed - accelerationDuration) / (spinDuration - accelerationDuration);
                currentSpeed = Mathf.Lerp(spinSpeed, 0f, decelerationProgress);
            }

            float rotation = currentSpeed * Time.deltaTime;
            roulette.Rotate(0, 0, -rotation); // Gira no sentido horário

            elapsed += Time.deltaTime;

            yield return null;
        }

        // Normaliza o ângulo entre 0 e 360
        float finalZ = roulette.eulerAngles.z % 360;
        
        DecideWinner(finalZ);
        //yield return new WaitForSeconds(1f);
        isSpinning = false;
        //finishedSpinning = true;
    }

    private void DecideWinner(float angle)
    {
        // Cada setor tem 90°
        // Setores: 0°–90° = Azul | 90°–180° = Vermelho | 180°–270° = Azul | 270°–360° = Vermelho

        if (angle >= 0 && angle < 90)
        {
            resultText.text = "Enemy starts!";
            Debug.Log("Vermelho → Inimigo começa!");
            BattleSetupManager.Instance.StartEnemyTurn();
        }
        else if (angle >= 90 && angle < 180)
        {
            resultText.text = "You starts!";
            Debug.Log("Azul → Player começa!");
            BattleSetupManager.Instance.StartPlayerTurn();
        }
        else if (angle >= 180 && angle < 270)
        {
            resultText.text = "Enemy starts!";
            Debug.Log("Vermelho → Inimigo começa!");
            BattleSetupManager.Instance.StartEnemyTurn();
        }
        else
        {
            resultText.text = "You starts!";
            Debug.Log("Azul → Player começa!");
            BattleSetupManager.Instance.StartPlayerTurn();
        }

        // ✅ Ativa as cartas
        DraggableCard.CanDrag = true;

        // ✅ Ativa os botões de controle
        BattleSetupManager.Instance.EnableControlButtons();

        // ✅ Destroi a roleta
        Destroy(gameObject, 2f);
    }
}
