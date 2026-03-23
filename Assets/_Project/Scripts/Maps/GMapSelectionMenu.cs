using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMapSelectionMenu : MonoBehaviour
{
    [SerializeField]
    List<GMapCard> _mapCards;
    
    [SerializeField]
    Button _exitBtn;

    void Start()
    {
        _exitBtn.onClick.AddListener(() => GGameManager.Instance.ChangeState(GGameManager.Instance.previousState));
    }

}
