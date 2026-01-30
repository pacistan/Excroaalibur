using UnityEngine;

public class CheatManager : MonoBehaviour
{
#if UNITY_EDITOR
    private const float Step = 0.25f;
    private const float MinScale = 0f;
    private const float MaxScale = 5f;

    void Update()
    {
        if (!Application.isPlaying)
            return;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            SetTimeScale(Time.timeScale + Step);

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
            SetTimeScale(Time.timeScale - Step);

        if (Input.GetKeyDown(KeyCode.Keypad0))
            SetTimeScale(1f);
    }

    private void SetTimeScale(float value)
    {
        Time.timeScale = Mathf.Clamp(value, MinScale, MaxScale);
        Debug.Log($"[CheatManager] TimeScale = {Time.timeScale}");
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }
#endif
}
