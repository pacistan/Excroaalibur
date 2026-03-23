using System;
using UnityEngine;

public class GMirror_Camera : MonoBehaviour
{
    [SerializeField]
    Camera _targetCamera;
    
    Camera _camera;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        _camera.fieldOfView = _targetCamera.fieldOfView;
    }
}
