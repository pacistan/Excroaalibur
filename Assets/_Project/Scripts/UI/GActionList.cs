using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GActionList : MonoBehaviour
{
    
    [SerializeField] 
    HorizontalLayoutGroup _listLayout;
    
    [SerializeField] 
    Image _buttonPrefab;

    int _currentActionIndex;

    List<Image> _images = new List<Image>();
    
    public void UpdateButtons(GPawn pawn, bool isFirstAction)
    {
        foreach (var image in _images)
        {
            Destroy(image.gameObject);
        }
        
        _images.Clear();

        if (pawn && pawn.isPlayer && pawn.actions != null)
        {
            if (isFirstAction)
            {
                CreateActionSlot(pawn.actions[1],false);
            }
            else if (pawn.GetCell().data.tileType == ETileType.Hole)
            {
                CreateActionSlot(pawn.actions[3], true);
            }
            else
            {
                CreateActionSlot(pawn.actions[0], true);
                if (pawn.equipment && pawn.equipment is GCrown)
                {
                    CreateActionSlot(pawn.actions[2], false);
                }
                else
                {
                    CreateActionSlot(pawn.actions[1],false);
                }
            }
            _currentActionIndex = 0;
        }
    }

    public void SwitchActionIndex()
    {
        _currentActionIndex = _currentActionIndex == 0 ? 1 : 0;
        _images[_currentActionIndex].color = Color.white;  
        _images[(_currentActionIndex + 1) % 2].color = Color.gray;
    }
    
    private void CreateActionSlot(GAction action, bool isOn)
    {
        _images.Add(Instantiate(_buttonPrefab, _listLayout.transform));
        _images.Last().sprite = action.actionIcon;
        _images.Last().color = isOn ? Color.white : Color.gray;
    }
    

}
