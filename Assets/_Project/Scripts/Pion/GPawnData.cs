using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PawnData", fileName = "Pawn Data")]
public class GPawnData : SerializedScriptableObject
{
    [field : SerializeField, Min(1)]
    public int actionTokens { get; private set; }
    
    [field : SerializeField]
    public bool isPlayer { get; private set; }
    
    [field : SerializeField]
    public int startHp { get; protected set; } 
    
    [SerializeReference, ShowIf("isPlayer")]
    public List<GAction> actions = new List<GAction>();
    
    [SerializeField]
    public GReactionData baseReactionData;
    
    // Cache in _overrideCache at Awake and OnValidate
    [OdinSerialize, DictionaryDrawerSettings(KeyLabel = "Action Type", ValueLabel = "Reaction"), Tooltip("Dictionary mapping action types to reaction actions that override both base reactions and default reactions.")] 
    public Dictionary<SerializableType<GAction>, GAction> overrideReactionByType = new();

    [FoldoutGroup("Other", false)]
    [SerializeField, FoldoutGroup("Other/Sound")]
    public EventReference hoverSound;

    [FoldoutGroup("Other", false)]
    [SerializeField, FoldoutGroup("Other/Sound")]
    public EventReference SelectSound;

}
