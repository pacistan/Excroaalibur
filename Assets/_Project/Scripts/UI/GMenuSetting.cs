using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public partial class GMenuManager
{
    [System.Serializable]
    public struct GMenuSetting
    {
        [FoldoutGroup("$menu")]
        public EMacroStates menu;

        [FoldoutGroup("$menu")]
        [HideIf("menu", EMacroStates.None)]
        public Texture2D cursor;

        [FoldoutGroup("$menu")]
        [HideIf("menu", EMacroStates.None)]
        [ShowIf("cursor")]
        public Vector2 cursorOffset;

        [FoldoutGroup("$menu")]
        [HideIf("menu", EMacroStates.None)] 
        public GameObject menuFolder;
        
        [FoldoutGroup("$menu")]
        [HideIf("menu", EMacroStates.None)] 
        public CanvasGroup canvasGroup;
        
        [FoldoutGroup("$menu")]
        [HideIf("menu", EMacroStates.None)] 
        public List<GUiTransition> uiTransitions;
        
        [FoldoutGroup("$menu")]
        [HideIf("menu", EMacroStates.None), ShowIf("_hideParameterForUiTransitions")] 
        public bool waitForTransitionsToEnableClicking;
        
        [FoldoutGroup("$menu")]
        public CinemachineCamera virtualCamera;
        
        private bool _hideParameterForUiTransitions {get => uiTransitions != null && uiTransitions.Count > 0;}

        [FoldoutGroup("$menu")]
        public bool _disableMenuOnHide;
    }
}
