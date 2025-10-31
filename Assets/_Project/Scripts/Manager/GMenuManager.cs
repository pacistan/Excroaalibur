using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif



/// <summary>
/// Handles transitions between menus depending on the macro states
/// </summary>
public partial class GMenuManager : GSingleton<GMenuManager>
{
    [SerializeField] private bool _AllowGizmos;
    [ShowIf("_AllowGizmos"), SerializeField, FoldoutGroup("Gizmos")] private EMacroStates _menuGizmos;
    [ShowIf("_AllowGizmos"), SerializeField, FoldoutGroup("Gizmos")] private int _menuGizmosResolution;
    [ShowIf("_AllowGizmos"), SerializeField, FoldoutGroup("Gizmos")] private float _menuLineRadius;
    [ShowIf("_AllowGizmos"), SerializeField, FoldoutGroup("Gizmos")] private float _menuInnerLineRadius;
    [ShowIf("_AllowGizmos"), SerializeField, FoldoutGroup("Gizmos")] private float _menuGizmosAnchoreRadius;
    [ShowIf("_AllowGizmos"), SerializeField, FoldoutGroup("Gizmos")] private float _menuGizmosLinePointRadius;

    [field : SerializeField] 
    public GMenuSetting[] menuSettings { get; private set; }

    private Dictionary<EMacroStates, GMenuSetting> _menuDictionary;

    protected override void Awake()
    {
        base.Awake();
        _menuDictionary = new Dictionary<EMacroStates, GMenuSetting>();
        if (menuSettings == null) return;
        foreach(var menuSetting in menuSettings)
        {
            if (_menuDictionary.ContainsKey(menuSetting.menu))
            {
                Debug.LogError("Multiple menu with the same type in settings");
                continue;
            }
            else
            {
                _menuDictionary.Add(menuSetting.menu, menuSetting);
                if(_menuDictionary[menuSetting.menu].menuFolder)
                {
                    _menuDictionary[menuSetting.menu].menuFolder.SetActive(menuSetting.menu == GGameManager.Instance.currentState);
                }
            }
        }
    }

    private void OnEnable()
    {
        GGameManager.Instance.OnChangeMacroStateEvent += OnMacroStateChange;
    }

    private void OnDisable()
    {
        GGameManager.Instance.OnChangeMacroStateEvent -= OnMacroStateChange;
    }

    void OnMacroStateChange(EMacroStates newState, EMacroStates oldState)
    {
        var newMenu = _menuDictionary[newState];
        var oldMenu = _menuDictionary[oldState];

        newMenu.menuFolder.SetActive(true);

        newMenu.menuFolder.transform.SetSiblingIndex(0);



        if (oldMenu.menu != EMacroStates.None && newMenu.menu != EMacroStates.Options) // No closing Transitions when opening Options
        {
            Action action = () => DisableMenu(oldMenu.menuFolder);

            StartCoroutine(DoTransitions(oldMenu, false, action));
        }

        if (oldMenu.menu != EMacroStates.Options) // No opening Transitions when closing Options
        {
            Action action = () =>
            {
                if (newMenu.menu == EMacroStates.LoadingScreen)
                {
                    GGameManager.Instance.LoadScene();
                }
            };
            StartCoroutine(DoTransitions(newMenu, true, action));
        }

        if (oldMenu.virtualCamera) newMenu.virtualCamera.Priority = 0;
        if (newMenu.virtualCamera) newMenu.virtualCamera.Priority = 0;

        switch (oldState)
        {
            default: break;
        }

        switch (newState)
        {
            case EMacroStates.End : 
                GHudManager.Instance.gameOverMenu.OnPanelOpen();
                break;
            default: break;
        }
    }

    private void DisableMenu(GameObject menu)
    {
        menu.SetActive(false);
    }

    public IEnumerator DoTransitions(GMenuSetting menuSetting, bool toActive, Action OnTransitionsOver = null)
    {
        int pendingTransitions = 0;
        if (menuSetting.uiTransitions != null)
        {
            foreach(var tr in menuSetting.uiTransitions)
            {
                pendingTransitions++;
                tr.DoTransition(this, toActive, () => pendingTransitions = pendingTransitions - 1);
            }
        }

        if (!menuSetting.waitForTransitionsToEnableClicking)
        {
            menuSetting.canvasGroup.interactable = toActive;
            menuSetting.canvasGroup.blocksRaycasts = toActive;
        }
        
        yield return new WaitUntil(() => pendingTransitions == 0);

        if (menuSetting.waitForTransitionsToEnableClicking)
        {
            menuSetting.canvasGroup.interactable = toActive;
            menuSetting.canvasGroup.blocksRaycasts = toActive;
        }
        OnTransitionsOver?.Invoke();
    }




#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_AllowGizmos || Application.isPlaying) return;

        foreach (var ms in menuSettings)
        {
            if(ms.menuFolder&& ms.menuFolder.activeInHierarchy && _menuGizmos != ms.menu) ms.menuFolder.SetActive(false);
            if(ms.menuFolder && !ms.menuFolder.activeInHierarchy && _menuGizmos == ms.menu) ms.menuFolder.SetActive(true);
        }
        
        GMenuSetting menuSetting = menuSettings.Where(x => x.menu == _menuGizmos).First();
        foreach (var tr in menuSetting.uiTransitions)
        {
            if (tr.rectTransform == null || tr.transitionType != GUiTransition.ETransitionType.Move) continue;
            Vector2 offset = tr.rectTransform.position - tr.rectTransform.TransformPoint(tr.rectTransform.anchoredPosition);

            Vector2? previousPos = null;
            for (int i = 0; i < _menuGizmosResolution + 1; i++)
            {
                float xLerp = Mathf.Lerp(tr.rectTransform.anchoredPosition.x + tr.offsetDistance.x, tr.rectTransform.anchoredPosition.x, tr.xCurve.Evaluate((float)i / _menuGizmosResolution));
                float yLerp = Mathf.Lerp(tr.rectTransform.anchoredPosition.y + tr.offsetDistance.y, tr.rectTransform.anchoredPosition.y, tr.yCurve.Evaluate((float)i / _menuGizmosResolution));
                Vector3 lerpPos = new Vector3(xLerp, yLerp, 0);

                // Convert lerpPos from anchored position to world position
                Vector3 worldLerpPos = tr.rectTransform.TransformPoint(lerpPos) + (Vector3)offset;
                if (previousPos == null) previousPos = worldLerpPos;
                if (i == 0 || i == _menuGizmosResolution)
                {
                    Gizmos.color = Color.black;
                    Handles.color = Color.black;
                    Gizmos.DrawSphere(worldLerpPos, _menuGizmosAnchoreRadius);
                }

                Gizmos.DrawSphere(worldLerpPos, _menuGizmosLinePointRadius);
                Handles.color = tr.gizmosColor;
                Handles.DrawLine(previousPos.Value, worldLerpPos, _menuLineRadius);
                Handles.color = Color.black;
                Handles.DrawLine(previousPos.Value, worldLerpPos, _menuInnerLineRadius);
                previousPos = worldLerpPos;
            }
        }
    }
#endif



}
