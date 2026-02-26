using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GSystemLoader : MonoBehaviour
{
    [FormerlySerializedAs("_pawnPrefab")]
    [SerializeField]
    private GameObject _systemPrefab;
    [SerializeField]
    GSOMapData _mapDataToLoad;
    
    bool _hasLoaded = false;
    
    void Awake()
    {
        if (!GGameManager.Instance)
        {
            _hasLoaded = true;
            Instantiate(_systemPrefab);
        }
    }

    void Start()
    {
        StartCoroutine(OnStart());
    }

    IEnumerator OnStart()
    {
        yield return new WaitForSecondsRealtime(1);
        if (_hasLoaded && _mapDataToLoad != null)
        {
            GGameManager.Instance.isLoadingTutorial = false;
            GGameManager.Instance.SetSceneToLoad(_mapDataToLoad);
            GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen);
        }
    }
}
