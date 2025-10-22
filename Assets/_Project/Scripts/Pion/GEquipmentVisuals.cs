using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(GEquipment))]
public class GEquipmentVisuals : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Components")]
    TextMeshProUGUI _debugText;

    [SerializeField, FoldoutGroup("Components")]
    GEquipment _equipment;
    
    public void OnUpdateDebugTextContent(int damage)
    {
        _debugText.text = $"{damage}";
    }
}
