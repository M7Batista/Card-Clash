using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardFlip : MonoBehaviour
{
    [Header("Refs")]
    public GameObject front;    // frente da carta
    public GameObject back;     // verso da carta
    public AudioSource audioSource;    // arraste um AudioSource no Inspector
    public AudioClip flipSound;        // som do flip

    [Header("Config")]
    public float flipDuration = 1f;   // tempo total do duplo flip (0 -> 360)
    public float jumpScale = 1.2f;    // quanto a carta cresce no "pulo"

    // Mantém o comportamento original de batalha: a carta começa virada para a frente,
    // passa pelo verso durante o primeiro giro e termina com a frente exibida novamente.
    public void FlipCard(Owner newOwner, CardUI cardUI)
    {
        StopAllCoroutines();
        StartCoroutine(FlipTwoTimesSameDirection());
    }

    // Cria um efeito específico para a tela de gacha: a carta começa com o verso visível,
    // gira uma única vez e só exibe a frente ao final da animação.
    public void FlipCardForGacha(CardUI cardUI)
    {
        StopAllCoroutines();
        StartCoroutine(FlipSingleRotation(cardUI));
    }

    // Animação original usada na captura de cartas, com dois giros completos.
    private IEnumerator FlipTwoTimesSameDirection()
    {
        if (front != null) front.SetActive(true);
        if (back != null) back.SetActive(false);

        float elapsed = 0f;
        float total = Mathf.Max(0.01f, flipDuration);

        bool switchedToBack = false;
        bool switchedToFrontAgain = false;

        while (elapsed < total)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / total);

            // --- ROTATION ---
            float angle = Mathf.Lerp(0f, 360f, Mathf.SmoothStep(0f, 1f, t));
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);

            // --- SCALE "JUMP" ---
            float scaleFactor = Mathf.Lerp(1f, jumpScale, 1f - Mathf.Abs(0.5f - t) * 2f);
            transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

            // --- VISUAL + AUDIO ---
            if (!switchedToBack && angle >= 90f)
            {
                if (front != null) front.SetActive(false);
                if (back != null) back.SetActive(true);

                PlayFlipSound();
                switchedToBack = true;
            }

            if (!switchedToFrontAgain && angle >= 270f)
            {
                if (back != null) back.SetActive(false);
                if (front != null) front.SetActive(true);

                PlayFlipSound();
                switchedToFrontAgain = true;
            }

            yield return null;
        }

        // --- FINAL STATE ---
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (front != null) front.SetActive(true);
        if (back != null) back.SetActive(false);
    }

    // Animação simples para a tela de gacha, com um único giro da carta.
    private IEnumerator FlipSingleRotation(CardUI cardUI)
    {
        if (front != null) front.SetActive(false);
        if (back != null) back.SetActive(true);

        float elapsed = 0f;
        float total = Mathf.Max(0.01f, flipDuration/2f); // metade do tempo para um giro único

        while (elapsed < total)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / total);

            float angle = Mathf.Lerp(0f, 180f, Mathf.SmoothStep(0f, 1f, t));
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);

            float scaleFactor = Mathf.Lerp(1f, jumpScale, 1f - Mathf.Abs(0.5f - t) * 2f);
            transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

            if (elapsed >= total * 0.5f)
            {
                PlayFlipSound();
            }

            yield return null;
        }

        // Garante que a frente só apareça após a animação terminar, preservando o efeito visual.
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (cardUI != null)
        {
            cardUI.ShowFront();
        }
        else
        {
            if (front != null) front.SetActive(true);
            if (back != null) back.SetActive(false);
        }
    }

    // Reproduz o som do flip quando a animação passa por um ponto importante do giro.
    private void PlayFlipSound()
    {
        if (audioSource != null && flipSound != null)
        {
            audioSource.PlayOneShot(flipSound);
        }
    }
}
