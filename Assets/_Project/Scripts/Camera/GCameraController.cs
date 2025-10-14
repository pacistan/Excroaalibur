using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GCameraController : MonoBehaviour
{
    [SerializeField]
    private float _rotationTime;
    [SerializeField] 
    private AnimationCurve _rotationCurve; 
    [SerializeField] 
    private Slider _rotationSlider;
    
    private float _inputRotationOffset;
    private IEnumerator _rotationEnum;

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, _inputRotationOffset, 0f);
        
        if (_rotationEnum != null) return;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _rotationEnum = RotationEnum(false);
            StartCoroutine(_rotationEnum);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _rotationEnum = RotationEnum(true);
            StartCoroutine(_rotationEnum);
        }
    }

    IEnumerator RotationEnum(bool turnRight)
    {
        float progress = 0;
        float startRotation = _inputRotationOffset;
        float targetRotation = turnRight ? startRotation - 90 : startRotation + 90;
        while (progress < 1)
        {
            progress += Time.deltaTime / _rotationTime;
            _inputRotationOffset = Mathf.Lerp(startRotation, targetRotation, _rotationCurve.Evaluate(progress));
            yield return null;
        }
        _inputRotationOffset = targetRotation;
        _rotationEnum = null;
    }
}
