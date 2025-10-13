using UnityEngine;
using UnityEngine.Serialization;

public class GReceptacle : MonoBehaviour
{
    [FormerlySerializedAs("_currentCell")]
    [SerializeField]
    public GCell cell;

}
