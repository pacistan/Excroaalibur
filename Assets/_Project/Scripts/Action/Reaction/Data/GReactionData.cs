using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;   
using Sirenix.Serialization;

[CreateAssetMenu(fileName = "ReactionData", menuName = "Reaction", order = 0)]
public class GReactionData : ScriptableObject
{
    /// <summary>
    /// Pair "Action Type" -> "Reaction".
    /// The key is selected via a dropdown (all concrete classes derived from GAction).
    /// The value is the reaction action to be cloned at runtime.
    /// </summary>
    [Serializable]
    public class ActionReactionPair
    {
        [LabelText("Action Type")]
        [SerializeField] public SerializableType<GAction> actionType;

        [LabelText("Reaction")]
        [SerializeReference] public GAction reaction;

        // Type of the action
        public Type ActionType => actionType != null ? actionType.Type : null;
    }

    // Editable list of action-reaction pairs
    [SerializeField]
    private List<ActionReactionPair> _reactionPairs = new List<ActionReactionPair>();

    // Dictionnaire runtime pour le lookup rapide
    private Dictionary<Type, GAction> _runtimeLookup;
    
    /** Build the runtime lookup dictionary */
    private void BuildLookup()
    {
        if (_runtimeLookup != null) return;

        _runtimeLookup = new Dictionary<Type, GAction>();
        foreach (var pair in _reactionPairs)
        {
            var t = pair?.ActionType;
            var r = pair?.reaction;
            if (t == null || r == null) continue;

            // La dernière entrée l’emporte si doublon de Type
            _runtimeLookup[t] = r;
        }
    }

  
    /** Check if there is a reaction for the given action */
    public bool HasReaction(GAction action)
    {
        BuildLookup();
        return action != null && _runtimeLookup.ContainsKey(action.GetType());
    }

   
    /** Get the reaction for the given action, if there is */
    public GAction GetReaction(GAction action)
    {
        if (action == null) return null;

        BuildLookup();

        if (_runtimeLookup.TryGetValue(action.GetType(), out var reaction) && reaction != null)
        {
            return (GAction)reaction.CloneAction();
        }
        return null;
    }

#if UNITY_EDITOR
    [Button("Rebuild Lookup (Editor)")]
    private void RebuildLookupEditor()
    {
        _runtimeLookup = null;
        BuildLookup();
    }
#endif

    private void OnValidate()
    {
        _runtimeLookup = null;
    }
}
