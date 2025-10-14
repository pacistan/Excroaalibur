using System;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class GWorldSpaceCanvas : MonoBehaviour
{
    private Canvas _canvas;
    void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 directionToCamera = _canvas.worldCamera.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-directionToCamera);
    }
}
