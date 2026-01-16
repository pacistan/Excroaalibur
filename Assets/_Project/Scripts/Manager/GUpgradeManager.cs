using Sirenix.OdinInspector;
using Stanpac.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GUpgradeManager : GSerializedSingleton<GUpgradeManager>
{
    [SerializeField, HideInPlayMode, LabelText("Upgrades")]
    Dictionary<ERarity, List<GSOUpgrade>> upgrades;
    
    [SerializeField, HideInEditorMode, LabelText("Upgrades")] 
    Dictionary<ERarity, List<GSOUpgrade>> _runTimeUpgrades = new Dictionary<ERarity, List<GSOUpgrade>>();
    
    [field : SerializeField, LabelText("Rarity Weights")] 
    public Dictionary<ERarity, float> weigthtDictionary { get; private set; } = new Dictionary<ERarity, float>();
    
    [FormerlySerializedAs("_upgradeMenu")]
    [SerializeField, ReadOnly, HideInEditorMode] 
    GUpgradeCardSelectMenu upgradeCardSelectMenu;
    
    [SerializeField] 
    int _numberOfPendingUpgrades = 3;
   
    List<GSOUpgrade> _pendingUpgrades = new List<GSOUpgrade>();
    int _selectedCardIndex = -1;
    
    [Button]
    public void StartUpgradeSequence()
    {
        _pendingUpgrades = GetUpgradesAtRarityLevel(GetRandomRarity(), _numberOfPendingUpgrades);
        upgradeCardSelectMenu.BuildUpgradesUI(ref _pendingUpgrades);
        GGameManager.Instance.ChangeState(EMacroStates.Upgrade_Select_Card);
    }

    public void OnUpgradeSelected(int upgradeIndex)
    {
        GGameManager.Instance.ChangeState(EMacroStates.Upgrade_Select_Character);
        _selectedCardIndex = upgradeIndex;
    }

    public void ReturnToUpgradeCardSelection()
    {
        GGameManager.Instance.ChangeState(EMacroStates.Upgrade_Select_Card);
        _selectedCardIndex = -1;
    }

    public void OnCharacterSelected(GPawn selectedCharacter)
    {
        Debug.Log("OnCharacterSelected");
        GGameManager.Instance.ChangeState(EMacroStates.Play);
        // Resume
    }
    
    private ERarity GetRandomRarity()
    {
        List<ERarity> rarityList = new List<ERarity>();
        float totalWeight = 0;
        foreach (ERarity rarity in Enum.GetValues(typeof(ERarity)))
        {
            rarityList.Add(rarity);
            totalWeight += weigthtDictionary[rarity];
        }
        float randomWeight = Random.Range(0, totalWeight);
        float progressWeight = 0;
        for (int i = 0; i < rarityList.Count; i++)
        {
            progressWeight += weigthtDictionary[rarityList[i]];
            if (progressWeight >= randomWeight)
            {
                return rarityList[i];
            }
        }
        
        return ERarity.Common;
    }
    
    private List<GSOUpgrade> GetUpgradesAtRarityLevel(ERarity rarity, int numberOfUpgrades)
    {
        List<GSOUpgrade> upgrades = new List<GSOUpgrade>();
        for (int i = numberOfUpgrades - 1; i >= 0 ; i--)
        {
            int index = Mathf.Min(_runTimeUpgrades[rarity].Count - 1, i);
            if (index == -1) break;
            upgrades.Add(_runTimeUpgrades[rarity][index]);
            _runTimeUpgrades[rarity].RemoveAt(index);
        }
        _runTimeUpgrades[rarity].AddRange(upgrades);
        return upgrades;
    }

    [Button]
    private void RarityTest()
    {
        Debug.Log(GetRandomRarity().ToString());    
    }
    
    
    void Start()
    {
        foreach (var upgradeRarity in upgrades.Keys)
        {
            _runTimeUpgrades.Add(upgradeRarity, new List<GSOUpgrade>());
            for (int i = 0; i < upgrades[upgradeRarity].Count; i++)
            {
                _runTimeUpgrades[upgradeRarity].Add(upgrades[upgradeRarity][i].CreateInstance());
            }
        }

        upgradeCardSelectMenu = FindFirstObjectByType<GUpgradeCardSelectMenu>(FindObjectsInactive.Include);
    }
}

