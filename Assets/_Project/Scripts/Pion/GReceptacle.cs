using UnityEngine;
using UnityEngine.Serialization;

public class GReceptacle : GGridObject
{
    [FormerlySerializedAs("_currentCell")]
    [SerializeField]
    public GCell cell;

}
