using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
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

    [SerializeField]
    private CinemachineFollow follow;
    
    float _currentHeight = 50;
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
        _targetPos = transform.position;
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
        
        transform.rotation = Quaternion.Euler(0f, _horizontalRotationOffset, 0f);
        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * _moveSpeed);
        if(follow)
            follow.FollowOffset.y = Mathf.Lerp(follow.FollowOffset.y, _currentHeight, Time.deltaTime * _heightSpeed);
    }

    private void Move()
    {
        Vector2 movement = _camMove.ReadValue<Vector2>() * (_moveSpeed * 10 * Time.deltaTime);
        _targetPos += new Vector3(movement.x, 0, movement.y);
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
