using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rt;
    private Canvas canvas;
    private CanvasGroup cg;
    private Vector3 startPos;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        startPos = rt.position;
        cg.blocksRaycasts = false; // permite que os cells recebam o drop
    }

    public void OnDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, e.position, e.pressEventCamera, out var worldPos);
        rt.position = worldPos;
    }

    public void OnEndDrag(PointerEventData e)
    {
        cg.blocksRaycasts = true;

        // Verifica se soltou em uma célula
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(e, results);

        foreach (var r in results)
        {
            var cell = r.gameObject.GetComponent<Cell>();
            if (cell != null && cell.IsEmpty)
            {
                // Coloca a carta no tabuleiro
                cell.PlaceCard(GetComponent<Image>().sprite);

                // Destroi a carta da mão (simples por enquanto)
                Destroy(gameObject);
                return;
            }
        }

        // Se não caiu em célula, volta pro lugar original
        rt.position = startPos;
    }
}
