using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardFlip : MonoBehaviour
{
    public Image frameImage;            // referência ao frame da carta
    public Color playerColor = Color.red;
    public Color enemyColor = Color.blue;

    public float flipDuration = 0.5f;
    Owner newOwner;

    public void FlipCard(Owner newOwner)
    {
        this.newOwner = newOwner;
        StartCoroutine(FlipAnimation());
    }

    private IEnumerator FlipAnimation()
    {
        float halfDuration = flipDuration / 2f;
        float elapsed = 0f;

        // Fase 1 → reduz escala até 0
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        // Troca de dono (muda a cor do frame)
        //isPlayerOwner = !isPlayerOwner;
        //frameImage.color = isPlayerOwner ? new Color32(52, 125, 255, 255) : new Color32(255, 71, 71, 255);
        //Owner newOwner = gameObject.GetComponent<CardUI>().owner;
        frameImage.color =
            (newOwner == Owner.Player) ? new Color32(52, 125, 255, 255) : new Color32(255, 71, 71, 255);

        elapsed = 0f;

        // Fase 2 → volta a escala para 1
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}
