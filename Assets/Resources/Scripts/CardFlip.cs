using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardFlip : MonoBehaviour
{
    [Header("Refs")]
    public Image frameImage;    // frame que muda de cor conforme o dono
    public GameObject front;    // frente da carta
    public GameObject back;     // verso da carta
    public AudioSource audioSource;    // arraste um AudioSource no Inspector
    public AudioClip flipSound;        // som do flip

    [Header("Config")]
    public float flipDuration = 1f;   // tempo total do duplo flip (0 -> 360)
    public float jumpScale = 1.2f;    // quanto a carta cresce no "pulo"

    private Owner newOwner;
    

    public void FlipCard(Owner newOwner)
    {
        this.newOwner = newOwner;
        StopAllCoroutines();
        StartCoroutine(FlipTwoTimesSameDirection());
    }

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

                if (frameImage != null)
                {
                    frameImage.color = (newOwner == Owner.Player)
                        ? new Color32(52, 125, 255, 255)
                        : new Color32(255, 71, 71, 255);
                }

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

    private void PlayFlipSound()
    {
        if (audioSource != null && flipSound != null)
        {
            audioSource.PlayOneShot(flipSound);
        }
    }
}
