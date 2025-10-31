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
    void Awake()
    {
        if (!GGameManager.Instance)
        {
            Instantiate(_systemPrefab);
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        GGameManager.Instance.SetSceneToLoad(SceneManager.GetActiveScene().name);
        GGameManager.Instance.ChangeState(EMacroStates.Play);
    }
}
