public class GHudManager : GSingleton<GHudManager>
{
    public GTargetHud TargetHud { get; private set; }
    public GPlayMenu playMenu { get; private set; }
    public GPauseMenu pauseMenu { get; private set; }
    public GStartMenu startMenu { get; private set; } 
    public GGameOverMenu gameOverMenu { get; private set; }
    public GLoadingScreenMenu loadingScreenMenu { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        startMenu = GetComponentInChildren<GStartMenu>(true);
        TargetHud = GetComponentInChildren<GTargetHud>(true);
        playMenu = GetComponentInChildren<GPlayMenu>(true);
        pauseMenu = GetComponentInChildren<GPauseMenu>(true);
        gameOverMenu = GetComponentInChildren<GGameOverMenu>(true);
        loadingScreenMenu = GetComponentInChildren<GLoadingScreenMenu>(true);
    }
}
