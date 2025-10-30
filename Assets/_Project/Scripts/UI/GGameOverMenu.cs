using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GGameOverMenu : MonoBehaviour
{
    [SerializeField]
    Button _replayButton, _mainMenuButton;

    [SerializeField]
    TextMeshProUGUI _waveNumberValueTxt;

    private void Start()
    {
        _replayButton.onClick.AddListener(() => GGameManager.Instance.ChangeState(EMacroStates.Play));
        _mainMenuButton.onClick.AddListener(() => GGameManager.Instance.ChangeState(EMacroStates.Start));
    }

    public void SetWaveNumberValue(int waveNumberValue)
    {
        _waveNumberValueTxt.text = waveNumberValue.ToString();
    }
}
