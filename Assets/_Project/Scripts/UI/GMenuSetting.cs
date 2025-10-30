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
        public EMacroStates menu;
        
        [HideIf("menu", EMacroStates.None)] 
        public GameObject menuFolder;
        
        [HideIf("menu", EMacroStates.None)] 
        public CanvasGroup canvasGroup;
        
        [HideIf("menu", EMacroStates.None)] 
        public List<GUiTransition> uiTransitions;
        
        [HideIf("menu", EMacroStates.None), ShowIf("_hideParameterForUiTransitions")] 
        public bool waitForTransitionsToEnableClicking;
        
        public CinemachineCamera virtualCamera;
        private bool _hideParameterForUiTransitions {get => uiTransitions != null && uiTransitions.Count > 0;}
    }
}
