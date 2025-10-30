using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GUiTransition 
{
    public enum ETransitionType { Disable, Fade, Move};
    public ETransitionType transitionType;
    [ShowIf("transitionType", ETransitionType.Move)] public RectTransform rectTransform;
    [ShowIf("transitionType", ETransitionType.Move)] public Vector2 offsetDistance;
    [ShowIf("transitionType", ETransitionType.Move)] public AnimationCurve xCurve;
    [ShowIf("transitionType", ETransitionType.Move)] public AnimationCurve yCurve;
    [ShowIf("transitionType", ETransitionType.Move)] public Color gizmosColor = Color.red;
    [ShowIf("transitionType", ETransitionType.Fade)] public CanvasGroup fadeCanvas;
    [ShowIf("transitionType", ETransitionType.Fade)] public AnimationCurve fadeCurve;
    [ShowIf("transitionType", ETransitionType.Disable)] public GameObject target;
    [HideIf("transitionType", ETransitionType.Disable)]public float duration;
    public float delayOnActivation;
    public float delayOnDeactivation;
    private Vector2? _basePosition = null;
    private IEnumerator _enumInstance;
    private float? _maxTransparence;

    public void DoTransition(MonoBehaviour objRef, bool toActivated, Action endAction = null)
    {
        if (transitionType == ETransitionType.Disable)
        {
            if(_enumInstance != null)
            {
                objRef.StopCoroutine(_enumInstance);
            }
            _enumInstance = DisableCoroutine(endAction, toActivated);
            objRef.StartCoroutine(_enumInstance);
            return;
        }
        if(_basePosition == null && transitionType == ETransitionType.Move)
        {
            _basePosition = rectTransform.anchoredPosition;
        }
        else if(_maxTransparence == null && transitionType == ETransitionType.Fade)
        {
            _maxTransparence = fadeCanvas.alpha;
        }
        if (_enumInstance != null)
            objRef.StopCoroutine(_enumInstance);

        if(transitionType == ETransitionType.Fade)
        {
            _enumInstance = toActivated ? FadeInCoroutine(endAction) : FadeOutCoroutine(endAction);
        }
        else
        {
            _enumInstance = toActivated ? MoveInCoroutine(endAction) : MoveOutCoroutine(endAction);
        }
        objRef.StartCoroutine(_enumInstance);
    }

    private IEnumerator DisableCoroutine(Action action, bool isActivate)
    {
        float timer = isActivate ? delayOnActivation : delayOnDeactivation;
        yield return new WaitForSecondsRealtime(timer);
        target.SetActive(isActivate);
        if (action != null)
        {
            action.Invoke();
        }
    }

    private IEnumerator MoveInCoroutine(Action action)
    {
        float interpolationValue = 0;
        rectTransform.anchoredPosition = _basePosition.Value + offsetDistance;

        yield return new WaitForSecondsRealtime(delayOnActivation);
        while (interpolationValue < 1)
        {
            interpolationValue += Time.unscaledDeltaTime / duration;
            float xLerp = LerpWithoutClamp(_basePosition.Value.x + offsetDistance.x, _basePosition.Value.x, xCurve.Evaluate(interpolationValue));
            float yLerp = LerpWithoutClamp(_basePosition.Value.y + offsetDistance.y, _basePosition.Value.y, yCurve.Evaluate(interpolationValue));
            rectTransform.anchoredPosition = new Vector3(xLerp, yLerp, 0);
            yield return null;
        }

        if (action != null)
        {
            action?.Invoke();
        }

        _enumInstance = null;
    }

    private IEnumerator MoveOutCoroutine(Action action)
    {
        float interpolationValue = 1;
        rectTransform.anchoredPosition = _basePosition.Value;

        yield return new WaitForSecondsRealtime(delayOnDeactivation);

        while(interpolationValue > 0)
        {
            interpolationValue -= Time.unscaledDeltaTime / duration;
            float xLerp = LerpWithoutClamp(_basePosition.Value.x + offsetDistance.x, _basePosition.Value.x, xCurve.Evaluate(interpolationValue));
            float yLerp = LerpWithoutClamp(_basePosition.Value.y + offsetDistance.y, _basePosition.Value.y, yCurve.Evaluate(interpolationValue));
            rectTransform.anchoredPosition = new Vector3(xLerp, yLerp, 0);
            yield return null;
        }
        if (action != null)
        {
            action?.Invoke();
        }

        _enumInstance = null;
    }

    private IEnumerator FadeInCoroutine(Action action)
    {
        fadeCanvas.gameObject.SetActive(true);
        float interpolationValue = 0;
        fadeCanvas.blocksRaycasts = true;
        fadeCanvas.alpha = 0;
        yield return new WaitForSecondsRealtime(delayOnActivation);
        while (interpolationValue < 1)
        {
            interpolationValue += Time.unscaledDeltaTime / duration;
            float fadeLerp = LerpWithoutClamp(0, _maxTransparence.Value, fadeCurve.Evaluate(interpolationValue));
            fadeCanvas.alpha = fadeLerp;
            yield return new WaitForEndOfFrame();
        }
        fadeCanvas.alpha = _maxTransparence.Value;

        if (action != null)
        {
            action?.Invoke();
        }

        _enumInstance = null;
    }

    private IEnumerator FadeOutCoroutine(Action action)
    {
        fadeCanvas.blocksRaycasts = false;
        float interpolationValue = 1;
        fadeCanvas.alpha = _maxTransparence.Value;

        yield return new WaitForSecondsRealtime(delayOnDeactivation);

        while (interpolationValue > 0)
        {
            interpolationValue -= Time.unscaledDeltaTime / duration;
            float fadeLerp = LerpWithoutClamp(0, _maxTransparence.Value, fadeCurve.Evaluate(interpolationValue));
            fadeCanvas.alpha = fadeLerp;
            yield return new WaitForEndOfFrame();
        }
        fadeCanvas.alpha = 0;
        if (action != null)
        {
            action?.Invoke();
        }

        fadeCanvas.gameObject.SetActive(false);
        _enumInstance = null;
    }

    public static float LerpWithoutClamp(float  A, float B, float t)
    {
        return A + (B - A) * t;
    }
}
