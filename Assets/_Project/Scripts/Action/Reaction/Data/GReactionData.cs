using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReactionData", menuName = "Reaction", order = 0)]
public class GReactionData : ScriptableObject
{
    [Serializable]
    public class ActionReactionPair
    {
        [SerializeReference] public GAction actionTypeReference;
        [SerializeReference] public GReaction reaction;
        
        [NonSerialized] private Type _cachedType;
        
        public Type ActionType
        {
            get
            {
                if (_cachedType == null && actionTypeReference != null)
                {
                    _cachedType = actionTypeReference.GetType();
                }
                return _cachedType;
            }
        }
    }
    
    [SerializeField]
    private List<ActionReactionPair> _reactionPairs = new List<ActionReactionPair>();
    
    private Dictionary<Type, GReaction> _runtimeLookup;
    
    private void BuildLookup()
    {
        if (_runtimeLookup != null) return;
        
        _runtimeLookup = new Dictionary<Type, GReaction>();
        foreach (var pair in _reactionPairs)
        {
            if (pair.ActionType != null && pair.reaction != null)
            {
                _runtimeLookup[pair.ActionType] = pair.reaction;
            }
        }
    }

    public bool HasReaction(GAction action)
    {
        BuildLookup();
        return action != null && _runtimeLookup.ContainsKey(action.GetType());
    }
    
    public GReaction GetReaction(GAction action)
    {
        if (HasReaction(action))
        {
            return (GReaction)_runtimeLookup[action.GetType()].CloneAction();
        }
        return null;
    }
    
    private void OnValidate()
    {
        _runtimeLookup = null;
    }
}
