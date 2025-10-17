using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReactionData", menuName = "Reaction", order = 0)]
public class GReactionData : ScriptableObject
{
    /// <summary>
    /// Pair of action and reaction with editable data.
    /// <see cref="ActionType"/> is the cached type of the ActionTypeReference to build the look-up Dictionary .
    /// </summary>
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
    
    /// <summary>
    /// List of <see cref="ActionReactionPair"/> easily editable from editor.
    /// </summary>
    [SerializeField]
    private List<ActionReactionPair> _reactionPairs = new List<ActionReactionPair>();
    
    private Dictionary<Type, GReaction> _runtimeLookup;
    
    /// <summary>
    /// Build the Type driven dictionary for quick look-up from the editor interactable list <see cref="_reactionPairs"/> of <see cref="ActionReactionPair"/>.
    /// </summary>
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

    /// <summary>
    /// Check if an action is available in the table.
    /// </summary>
    /// <param name="action">Action to search a reaction for.</param>
    /// <returns>True if a reaction was found.</returns>
    public bool HasReaction(GAction action)
    {
        BuildLookup();
        return action != null && _runtimeLookup.ContainsKey(action.GetType());
    }
    
    /// <summary>
    /// Get the reaction for a given action (Can check if there is a reaction with <see cref="HasReaction"/>.
    /// </summary>
    /// <param name="action">Action to search a reaction for.</param>
    /// <returns>Reaction fot the given action, null if none were found.</returns>
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
