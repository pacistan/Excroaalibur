using System;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.Serialization;

[DefaultExecutionOrder(100)]
public class GSaveManager : GSingleton<GSaveManager>
{
    [SerializeField] GGameStateSaveHandler _gameStateSaveHandler;
    [SerializeField] GMapStatesSaveHandler _mapStatesSaveHandler;

    public int GetMapIndex(GSOMapData mapData) => Array.IndexOf(_mapStatesSaveHandler._mapData, mapData);

    public GSOMapData  GetMapData(int index) => _mapStatesSaveHandler._mapData[index];
    
    private void OnStateChange(EMacroStates currentState, EMacroStates previousState)
    {
        if (currentState == EMacroStates.Play)
        {
            GTurnBaseManager.Instance.OnPostPlayerTurn += OnPlayerTurnEnd;
        }
        else if(previousState == EMacroStates.Play)
        {
            GTurnBaseManager.Instance.OnPostPlayerTurn -= OnPlayerTurnEnd;
        }


        if (currentState == EMacroStates.Start)
        {
            var data = _gameStateSaveHandler.DeserializeFromJson();
            GGameManager.Instance.SetGameStateData(data);
        }

        if (currentState == EMacroStates.End)
        {
            _gameStateSaveHandler.DeleteSaveFile();
            GGameManager.Instance.loadableMapData.NumberOfWavesOnThisMap = Mathf.Max(GGameManager.Instance.loadableMapData.NumberOfWavesOnThisMap, GTurnBaseManager.Instance.GetScore());
            //_mapStatesSaveHandler.SerializeToJson();
        }
    }

    private void OnPlayerTurnEnd(int i)
    {
        _gameStateSaveHandler.SerializeToJson();
    }


    void OnEnable()
    {
        GGameManager.Instance.OnChangeMacroStateEvent += OnStateChange;
    }

    void OnDisable()
    {
        GGameManager.Instance.OnChangeMacroStateEvent -= OnStateChange;
    }

    public bool IsGameStateSaveFileCreated()
    {
        return _gameStateSaveHandler.IsSaveFileCreated();
    }
}





