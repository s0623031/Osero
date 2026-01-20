using UnityEngine;
using UnityEngine.EventSystems; // ★マウスイベントに必要

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("表示させたい説明パネル")]
    public GameObject tooltipPanel;

    // ゲーム開始時は隠しておく
    void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    // カーソルが入った時（ホバー開始）
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }
    }

    // カーソルが出た時（ホバー終了）
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}