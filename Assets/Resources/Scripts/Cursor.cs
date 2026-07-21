using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Cursor : MonoBehaviour
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
    // Faz um movimento suave da posição atual até a posição do alvo usando Lerp
    public void MoveCursorTo(Transform target)
    {
        StartCoroutine(MoveToTarget(target));
    }
    IEnumerator MoveToTarget(Transform target)
    {
        float duration = 0.5f; // duração do movimento
        float elapsed = 0f;
        Vector3 startingPos = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startingPos, target.position, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target.position; // garante que a posição final seja exata
    }
}
