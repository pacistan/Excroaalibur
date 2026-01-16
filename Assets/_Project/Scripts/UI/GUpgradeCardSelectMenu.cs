using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GUpgradeCardSelectMenu : MonoBehaviour
{
    [SerializeField]
    List<GUpgradeCard> _upgrades = new List<GUpgradeCard>();

    void Start()
    {
        for (var i = 0; i < _upgrades.Count; i++)
        {
            _upgrades[i].Initialize(i);
        }
    }

    public void BuildUpgradesUI(ref List<GSOUpgrade> upgradesData)
    {
        for(int i = 0; i < upgradesData.Count; i++)
        {
            _upgrades[i].Build(upgradesData[i]);
        }
    }    
}