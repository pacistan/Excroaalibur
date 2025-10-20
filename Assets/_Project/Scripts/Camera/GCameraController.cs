using Sirenix.OdinInspector;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GCameraController : MonoBehaviour 
{
    [SerializeField]
    float TopHeight = 50;
    [SerializeField]
    float BottomHeight = 10;

    [SerializeField]
    private float _quarterRotationSpeed = 1;
    [SerializeField]
    private float _rotationSpeed = 3;
    [SerializeField]
    private float _heightSpeed  = 1;
    [SerializeField]
    private float _moveSpeed  = 1;
    [SerializeField] 
    private AnimationCurve _rotationCurve;

    [SerializeField, FoldoutGroup("Components")]
    private CinemachineFollow follow;

    [SerializeField, FoldoutGroup("Components")]
    Transform _cameraTargetTr;
    
    float _currentHeight;
    Vector3 _targetPos;
    private float _horizontalRotationOffset;
    private IEnumerator _rotationEnum;
    
    private InputAction _quartTurnLeft;
    private InputAction _quartTurnRight;
    private InputAction _horizontalRot;
    private InputAction _height;
    private InputAction _camMove;
    
        
    void Start()
    {
        _quartTurnLeft = InputSystem.actions.FindAction("QuartTurnLeft");
        _quartTurnRight = InputSystem.actions.FindAction("QuartTurnRight");
        _horizontalRot = InputSystem.actions.FindAction("HorizontalTurn");
        _height = InputSystem.actions.FindAction("MoveUpDown");
        _camMove = InputSystem.actions.FindAction("CamMove");
        _targetPos = _cameraTargetTr.position;
        _currentHeight = follow.FollowOffset.y;
    }

    private void Update()
    {
        if (_quartTurnLeft.WasPressedThisFrame())
            StartHorizontalQuartRotation(false);
        if (_quartTurnRight.WasPressedThisFrame())
            StartHorizontalQuartRotation(true);

        if (_horizontalRot.IsPressed())
            AddHorizontalQuartRotation();

        if (_height.IsPressed())
            MoveHeight();

        Move();
        
        _cameraTargetTr.rotation = Quaternion.Euler(0f, _horizontalRotationOffset, 0f);
        _cameraTargetTr.position = Vector3.Lerp(_cameraTargetTr.position, _targetPos, Time.deltaTime * _moveSpeed);
        if(follow)
            follow.FollowOffset.y = Mathf.Lerp(follow.FollowOffset.y, _currentHeight, Time.deltaTime * _heightSpeed);
    }

    private void Move()
    {
        Vector2 input = _camMove.ReadValue<Vector2>() * (_moveSpeed * 10 * Time.deltaTime);
        
        // Convert the input to a direction relative to the camera's rotation
        float yRotation = _horizontalRotationOffset * Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(yRotation), 0, Mathf.Cos(yRotation));
        Vector3 right = new Vector3(Mathf.Cos(yRotation), 0, -Mathf.Sin(yRotation));
        
        // Apply movement relative to camera orientation
        Vector3 movement = (forward * input.y) + (right * input.x);
        _targetPos += movement;
    }

    void MoveHeight()
    {
        _currentHeight += _height.ReadValue<float>() * (_heightSpeed * 10) * Time.deltaTime;
        _currentHeight = Mathf.Clamp(_currentHeight, BottomHeight, TopHeight);
    }
    
    public void AddHorizontalQuartRotation()
    {
        float Axis = _horizontalRot.ReadValue<float>();
        _horizontalRotationOffset += Axis * (_rotationSpeed * 10) * Time.deltaTime;
    }
    
    public void StartHorizontalQuartRotation(bool turnRight)
    {
        if (_rotationEnum != null) 
            StopCoroutine(_rotationEnum);
        
        _rotationEnum = HorizontalQuartRotation(turnRight);
        StartCoroutine(_rotationEnum);
    }

    IEnumerator HorizontalQuartRotation(bool turnRight)
    {
        float progress = 0;
        float startRotation = _horizontalRotationOffset;
        float targetRotation = turnRight ? startRotation - 90 : startRotation + 90;
        while (progress < 1)
        {
            progress += Time.deltaTime / _quarterRotationSpeed;
            _horizontalRotationOffset = Mathf.Lerp(startRotation, targetRotation, progress/*_rotationCurve.Evaluate(progress)*/);
            yield return null;
        }
        _horizontalRotationOffset = targetRotation;
        _rotationEnum = null;
    }
}