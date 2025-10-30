using System;
using UnityEngine;
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
}
