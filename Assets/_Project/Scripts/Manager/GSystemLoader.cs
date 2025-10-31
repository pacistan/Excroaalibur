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
    bool _hasLoaded = false;
    
    void Awake()
    {
        if (!GGameManager.Instance)
        {
            _hasLoaded = true;
            Instantiate(_systemPrefab);
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        if (_hasLoaded)
        {
            GGameManager.Instance.SetSceneToLoad(SceneManager.GetActiveScene().name);
            GGameManager.Instance.ChangeState(EMacroStates.Play);
        }
    }
}
