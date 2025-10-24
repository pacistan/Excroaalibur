using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using STOP_MODE = FMOD.Studio.STOP_MODE;

/* Responsable de la gestion globale du Jeu, de l'activation de potentiel Manager etc...*/
public class GameManager: GSingleton<GameManager>
{
    public bool isGamePaused = false;

    InputAction _menuInput;
    
    [SerializeField]
    PauseMenu _menu;
    
    [SerializeField]
    EventReference musicEvent;
    EventInstance musicInstance;
    EventInstance pauseSnapshot;
    
    public void PauseGame(bool resume = false)
    {
        isGamePaused = !resume;
        if (resume)
            pauseSnapshot.stop(STOP_MODE.ALLOWFADEOUT);
        else
            pauseSnapshot.start();
        // TODO : Potentially add more logic here (e.g., notify other systems)
    }

    void StartMusic()
    {
        if (musicEvent.IsNull) return;
        pauseSnapshot = RuntimeManager.CreateInstance("snapshot:/Pause");
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
    }

    void OpenMenu()
    {
        if (!_menu) return;
        PauseGame();
        
        _menu.Open();
    }

    void CloseMenu()
    {
        if (!_menu) return;
        PauseGame(true);
        _menu.Close();
    }

    void Start()
    {
        _menuInput = InputSystem.actions.FindAction("Menu");
        StartMusic();
    }

    void Update()
    {
        if (_menuInput.WasPressedThisFrame())
        {
            if (_menu.IsOpen)
                CloseMenu();
            else
                OpenMenu();
        }
    }

    // TODO : Manage the Scene transitions, loading screens, etc.
}
