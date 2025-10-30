using Dan.Main;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GGameOverMenu : MonoBehaviour
{
    [SerializeField]
    Button _replayButton, _mainMenuButton;

    [SerializeField]
    TextMeshProUGUI _waveNumberValueTxt;
    
    [SerializeField] 
    TMP_Text[] _entryTextObjects;
    [SerializeField] 
    TMP_InputField _usernameInputField;

    private void Start()
    {
        _replayButton.onClick.AddListener(() => GGameManager.Instance.ChangeState(EMacroStates.Play));
        _mainMenuButton.onClick.AddListener(() => GGameManager.Instance.ChangeState(EMacroStates.Start));
    }

    public void SetWaveNumberValue(int waveNumberValue)
    {
        _waveNumberValueTxt.text = waveNumberValue.ToString();
    }
    
    public void LoadEntries()
    {
        Leaderboards.Croawn.GetEntries((entries) =>
        {
            foreach (var t in _entryTextObjects)
                t.text = "";

            var length = Mathf.Min(_entryTextObjects.Length, entries.Length);
            for (int i = 0; i < length; i++)
                _entryTextObjects[i].text += $"{entries[i].Rank}. {entries[i].Username} - {entries[i].Score}";
        });
    }

    public void UploadEntry()
    {
        int Score = GWaveManager.Instance.GetScore();
        Leaderboards.Croawn.UploadNewEntry(_usernameInputField.text, Score , isSuccessful =>
        {
            if (isSuccessful)
                LoadEntries();
        });
    }
}
