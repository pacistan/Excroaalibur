using UnityEngine;
using UnityEngine.UI;

public class GUpgradeCharacterSelectMenu : MonoBehaviour
{
    [SerializeField] Button _btnReturn;

    void Start()
    {
        _btnReturn.onClick.AddListener(()=> GGameManager.Instance.ChangeState(EMacroStates.Upgrade_Select_Card));
    }
}
