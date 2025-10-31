using Unity.Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;

public partial class GMenuManager
{
    [System.Serializable]
    public struct GMenuSetting
    {
        [FoldoutGroup("$menu")]
        public EMacroStates menu;
        
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
    }
}
