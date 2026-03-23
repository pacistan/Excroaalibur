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

    public GSOMapData  GetMapData(int index) => _mapStatesSaveHandler._mapData.Length >= index ? null : _mapStatesSaveHandler._mapData[index];

    
    private void OnStateChange(EMacroStates currentState, EMacroStates previousState)
    {
        if (currentState == EMacroStates.Play)
        {
            GTurnBaseManager.Instance.OnPrePlayerTurn += OnPlayerTurnStart;
        }
        else if(previousState == EMacroStates.Play)
        {
            GTurnBaseManager.Instance.OnPrePlayerTurn -= OnPlayerTurnStart;
        }


        if (currentState == EMacroStates.Start)
        {
            var mapData = _mapStatesSaveHandler.DeserializeFromJson();
            var data = _gameStateSaveHandler.DeserializeFromJson();
            GGameManager.Instance.SetGameStateData(data);
            _mapStatesSaveHandler.UpdateGameStateFromData(mapData);
        }

        if (currentState == EMacroStates.End)
        {
            _gameStateSaveHandler.DeleteSaveFile();
            GGameManager.Instance.loadableMapData.NumberOfWavesOnThisMap = Mathf.Max(GGameManager.Instance.loadableMapData.NumberOfWavesOnThisMap, GTurnBaseManager.Instance.GetScore());
            _mapStatesSaveHandler.SerializeToJson();
        }
    }

    private void OnPlayerTurnStart(int i)
    {
        _mapStatesSaveHandler.SerializeToJson();
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
    
    public bool HasValidSaveFile()
    {
        bool isValid = _gameStateSaveHandler.IsSaveFileCreated();
        if (!isValid) return false;
        
        isValid = GGameManager.Instance.loadableMapData != null;
        
        return isValid;
    }
}





