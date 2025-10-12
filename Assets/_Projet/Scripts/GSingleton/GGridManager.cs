using Sirenix.OdinInspector;
using UnityEngine;

public class GGridManager : GSingleton<GGridManager>
{
    [SerializeField] GGridData _gridData;
}

[CreateAssetMenu(fileName = "GridData", menuName = "Grid Data", order = 1)]
public class GGridData : SerializedScriptableObject
{
    [field : SerializeField] public int RowNum { get; private set; }
    [field : SerializeField] public int ColumnNum { get; private set; }
}