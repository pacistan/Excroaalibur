using TMPro;
using UnityEngine;

public class GMainHudManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _crownDamageTxt;
    
    public void SetCrownDamageText(int damage) => _crownDamageTxt.text = $"Sword Damage : {damage.ToString()}";
}
