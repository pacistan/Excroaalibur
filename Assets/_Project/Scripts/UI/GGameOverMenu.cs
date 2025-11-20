using Dan.Main;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GGameOverMenu : MonoBehaviour
{
    [SerializeField]
    Button _replayButton, _mainMenuButton;

    [SerializeField]
    TextMeshProUGUI _waveNumberValueTxt;

    [SerializeField, BoxGroup("Leaderboard")]
    int _maxNumberOfLeaderboardProfils;
    
    [SerializeField, BoxGroup("Leaderboard")]
    GLeaderboardProfilEntry _profilPrefab;

    [SerializeField, BoxGroup("Leaderboard")]
    Transform _profilFolder;

    [SerializeField, BoxGroup("Leaderboard")]
    TMP_InputField _newHighscoreInputField;
    
    [SerializeField, BoxGroup("Leaderboard")]
    Button _newHighscoreButton;

    [SerializeField, BoxGroup("Leaderboard")]
    int _characterLimitForProfilName = 10;
    
    GLeaderboardProfilEntry[] _profils;
    
    public void OnPanelOpen()
    {
        if (_profils == null)
        {
            _profils = new GLeaderboardProfilEntry[_maxNumberOfLeaderboardProfils];
            for (int i = 0; i < _maxNumberOfLeaderboardProfils; i++)
            {
                _profils[i] = Instantiate(_profilPrefab,  _profilFolder);
            }
        }
        LoadEntries();
        _waveNumberValueTxt.text = $"Number of waves completed : {GTurnBaseManager.Instance.GetScore()}";
    }
    
    public void LoadEntries()
    {
        _profils.ForEach(a=>a.gameObject.SetActive(false));
        Leaderboards.Croawn.GetEntries((entries) =>
        {
            var length = Mathf.Min(_profils.Length, entries.Length);
            for (int i = 0; i < length; i++)
            {
                _profils[i].gameObject.SetActive(true);
                _profils[i].rankTxt.text = entries[i].Rank.ToString();

                _profils[i].usernameTxt.text = entries[i].Username.Length > _characterLimitForProfilName
                    ? entries[i].Username.Substring(0, _characterLimitForProfilName)
                    : entries[i].Username;

                _profils[i].waveNbrTxt.text = entries[i].Score.ToString();
            }
        });
    }

    public void UploadEntry()
    {
        int Score = GTurnBaseManager.Instance.GetScore();
        Leaderboards.Croawn.UploadNewEntry(_newHighscoreInputField.text, Score , isSuccessful =>
        {
            if (isSuccessful)
                LoadEntries();
        });
    }

    private void Start()
    {
        _replayButton.onClick.AddListener(() => GGameManager.Instance.ChangeState(EMacroStates.LoadingScreen));
        _mainMenuButton.onClick.AddListener(() => GGameManager.Instance.ChangeState(EMacroStates.Start));
        _newHighscoreButton.onClick.AddListener(UploadEntry);
        _newHighscoreInputField.characterLimit = _characterLimitForProfilName;
    }

}