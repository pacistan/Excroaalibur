using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GCameraController : MonoBehaviour 
{
    [SerializeField]
    private float _rotationTime;
    [SerializeField] 
    private AnimationCurve _rotationCurve; 
    
    private float _horizontalRotationOffset;
    private IEnumerator _rotationEnum;
    
    [SerializeField]
    private InputAction _horizontalQuartRotationAction;

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, _horizontalRotationOffset, 0f);
    }
    
    public void AddHorizontalQuartRotation(InputAction.CallbackContext context)
    {
        float Axis = context.ReadValue<float>();
        _horizontalRotationOffset += Axis;
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
            progress += Time.deltaTime / _rotationTime;
            _horizontalRotationOffset = Mathf.Lerp(startRotation, targetRotation, progress/*_rotationCurve.Evaluate(progress)*/);
            yield return null;
        }
        _horizontalRotationOffset = targetRotation;
        _rotationEnum = null;
    }
}
