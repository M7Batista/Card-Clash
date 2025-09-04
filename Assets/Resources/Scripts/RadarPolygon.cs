using UnityEngine;
using UnityEngine.UI;

public class RadarPolygon : Graphic
{
    [Range(0, 10)] public float top = 3f;
    [Range(0, 10)] public float right = 7f;
    [Range(0, 10)] public float bottom = 3f;
    [Range(0, 10)] public float left = 9f;

    float maxValue = 10f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float w = rectTransform.rect.width / 2f;
        float h = rectTransform.rect.height / 2f;

        // Normalizar valores
        float topNorm = (top / maxValue) * h;
        float rightNorm = (right / maxValue) * w;
        float bottomNorm = (bottom / maxValue) * h;
        float leftNorm = (left / maxValue) * w;

        // Definir pontos (ordem horária)
        Vector2 center = Vector2.zero;
        Vector2 pTop = new Vector2(0, topNorm);
        Vector2 pRight = new Vector2(rightNorm, 0);
        Vector2 pBottom = new Vector2(0, -bottomNorm);
        Vector2 pLeft = new Vector2(-leftNorm, 0);

        // Adicionar vértices
        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        int centerIndex = 0;
        v.position = center; vh.AddVert(v);

        v.position = pTop; vh.AddVert(v);
        v.position = pRight; vh.AddVert(v);
        v.position = pBottom; vh.AddVert(v);
        v.position = pLeft; vh.AddVert(v);

        // Triângulos (fan do centro)
        vh.AddTriangle(centerIndex, 1, 2);
        vh.AddTriangle(centerIndex, 2, 3);
        vh.AddTriangle(centerIndex, 3, 4);
        vh.AddTriangle(centerIndex, 4, 1);
    }
}
