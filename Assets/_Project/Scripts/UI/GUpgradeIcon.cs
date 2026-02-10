using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUpgradeIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /* int : Index of the icon in the upgrade list */
    public Action<int> OnUpgradeIconHovered;
    public Action<int> OnUpgradeIconUnhovered;
    public int index;

    [SerializeField]
    Image _upgradeImage;

    [SerializeField]
    Image _upgradeHighlight;

    public void SetUpgradeIcon(Sprite icon)
    {
        _upgradeImage.sprite = icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnUpgradeIconHovered?.Invoke(index);
        _upgradeHighlight.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnUpgradeIconUnhovered?.Invoke(index);
        _upgradeHighlight.color = Color.black;
    }
}