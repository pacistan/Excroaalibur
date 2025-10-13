using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class GCrown : MonoBehaviour
{
    [SerializeField, ReadOnly]
    public GPawn owner;

    [SerializeField, ReadOnly]
    public GCell cell;
}