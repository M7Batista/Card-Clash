using UnityEngine;
using UnityEngine.UI;

public class TurnArrow : MonoBehaviour
{
    public float pulseSpeed = 2f;       // velocidade da animação
    public float pulseScale = 1.2f;     // quanto cresce
    private Vector3 originalScale;
    private bool pulsing = false;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        pulsing = true;
    }

    void OnDisable()
    {
        pulsing = false;
        transform.localScale = originalScale;
    }

    void Update()
    {
        if (!pulsing) return;

        float scale = 1 + (Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1));
        transform.localScale = originalScale * scale;
    }
}
