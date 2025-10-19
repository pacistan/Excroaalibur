using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GActionList : MonoBehaviour
{
    public event Action<int> OnActionSelected;
    
    [SerializeField] VerticalLayoutGroup _listLayout;
    [SerializeField] Button _buttonPrefab;


    List<Button> _buttons = new List<Button>();
    
    public void UpdateButtons(GAction[] actions)
    {
        foreach (var button in _buttons)
        {
            button.onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }
        
        _buttons.Clear();

        for (int i = 0; i < actions.Length; i++)
        {
            int index = i;
            Button button = Instantiate(_buttonPrefab, _listLayout.transform);
            button.onClick.AddListener(() => SelectAction(index));
            _buttons.Add(button);
            button.interactable = actions[i].GetValidCells().Length > 0;
            
            TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp)
                tmp.text = actions[i].ToString();
        }
    }

    private void SelectAction(int id)
    {
        OnActionSelected?.Invoke(id);
    }
}
