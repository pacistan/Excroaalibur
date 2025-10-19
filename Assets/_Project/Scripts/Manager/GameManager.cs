using UnityEngine;

/* Responsable de la gestion globale du Jeu, de l'activation de potentiel Manager etc...*/
public class GameManager: GSingleton<GameManager>
{
    public bool isGamePaused = false;
    
    public void PauseGame(bool resume = false)
    {
        isGamePaused = !resume;
        // TODO : Potentially add more logic here (e.g., notify other systems)
    }
    
    // TODO : Manage the Scene transitions, loading screens, etc.
}
