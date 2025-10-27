using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GMainHudManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _crownDamageTxt;
    
    [field : SerializeField]
    public Button endTurnButton { get; private set; }
    
    public void SetCrownDamageText(int damage) => _crownDamageTxt.text = $"Sword Damage : {damage.ToString()}";
}
