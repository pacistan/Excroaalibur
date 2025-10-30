public class GHudManager : GSingleton<GHudManager>
{
    public GTargetHud TargetHud { get; private set; }
    public GMainHudManager mainHudManager { get; private set; }
    public GPauseMenu pauseMenu { get; private set; }

    public GStartMenu startMenu { get; private set; } 

    protected override void Awake()
    {
        base.Awake();
        startMenu = GetComponentInChildren<GStartMenu>(true);
        TargetHud = GetComponentInChildren<GTargetHud>(true);
        mainHudManager = GetComponentInChildren<GMainHudManager>(true);
        pauseMenu = GetComponentInChildren<GPauseMenu>(true);
    }
}
