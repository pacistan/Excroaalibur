using System;
using TMPro;
using UnityEngine;

public class GDebugUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI actNumText;
    [SerializeField]
    TextMeshProUGUI turnControllerText;

    void Start()
    {
        GTurnBaseManager.Instance.actionPlayed += SetActNum;
        GTurnBaseManager.Instance.startControllerTurn += SetTurnController;
    }

    public void SetActNum(GAction action, GController controller)
    {
        actNumText.text = controller._remainingActionToken.ToString();
    }

    public void SetTurnController(GController controller)
    {
        turnControllerText.text = controller.ToString();
    }
}
