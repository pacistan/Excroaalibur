using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GDebugUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI actNumText;
    [SerializeField]
    TextMeshProUGUI turnControllerText;

    void Awake()
    {
        GTurnBaseManager.Instance.actionPlayed += OnAct;
        GTurnBaseManager.Instance.startControllerTurn += SetTurnController;
    }

    public void OnAct(GAction action, GController controller)
    {
        SetTurnController(controller);
    }

    public IEnumerator SetActNum(GController controller)
    {
        yield return new WaitForSecondsRealtime(.1f);
        actNumText.text = controller.remainingActionToken.ToString();
    }

    public void SetTurnController(GController controller)
    {
        turnControllerText.text = controller.ToString();
        StartCoroutine(SetActNum(controller));
    }
}
