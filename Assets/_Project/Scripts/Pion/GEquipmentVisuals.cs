using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(GEquipment))]
public class GEquipmentVisuals : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Components")]
    GEquipment _equipment;
}
