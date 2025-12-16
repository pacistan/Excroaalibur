using UnityEngine;

[CreateAssetMenu(fileName = "Curve_", menuName = "Actions/Previsualisation Curve")]
public class GActionPrevisualisationCurveData : ScriptableObject
{
    [field: SerializeField] public int previsuCurveResolution { get; private set; } = 100;
    [field: SerializeField] public float previsuCurveMaxHeight { get; private set; } = 0.5f;
    [field : SerializeField] public AnimationCurve previsuHeightCurve { get; private set; }= AnimationCurve.EaseInOut(0, 0, 1, 1);
    [field : SerializeField] public float startHeightOffset { get; private set; } = 0;
    [field :  SerializeField] public float endHeightOffset { get; private set; } = 0;
    [field: SerializeField] public Material previusCurveMaterial { get; private set; }
    [field: SerializeField] public float previusCurveWidth { get; private set; } = 1;
    [field: SerializeField] public bool isPositionRelativeToTargets { get; private set; } = true;
}
