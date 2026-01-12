using FMODUnity;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "PawnData", fileName = "Pawn Data")]
public class GPawnData : SerializedScriptableObject
{
    [SerializeField]
    public EGridObjectType gridObjectType = EGridObjectType.None;
    
    [field: SerializeField]
    public bool isPlayer { get; private set; }

    // [field: SerializeField]
    // public int startHp { get; protected set; }

    /** Attribute Profile defining base attributes for this pawn */
    [SerializeField]
    public GAttributeProfile attributeProfile;
    
    [SerializeField]
    public ETileType[] _walkingTileType = new[]{ETileType.Normal};

    [SerializeField]
    public ETileType[] _endMovementTileType = new[]{ETileType.Normal};
    
    [SerializeReference, FoldoutGroup("Actions"), ShowIf("isPlayer")]
    public List<GAction> actionList = new List<GAction>();
    
    [SerializeField, FoldoutGroup("Actions")]
    public GReactionData baseReactionData;
    
    // Cache in _overrideCache at Awake and OnValidate
    [OdinSerialize, FoldoutGroup("Actions"), DictionaryDrawerSettings(KeyLabel = "Action Type", ValueLabel = "Reaction"), Tooltip("Dictionary mapping action types to reaction actions that override both base reactions and default reactions.")] 
    public Dictionary<SerializableType<GAction>, GAction> overrideReactionByType = new();
    
    [field: SerializeField, BoxGroup("Feedbacks")]
    public float previsuHeightOffset { get; private set; } = .5f;
    
    [FoldoutGroup("Other", false)]
    [SerializeField, FoldoutGroup("Other/Sound")]
    public EventReference hoverSound;
    
    [SerializeField, FoldoutGroup("Other/Sound")]
    public EventReference SelectSound;
    

}
