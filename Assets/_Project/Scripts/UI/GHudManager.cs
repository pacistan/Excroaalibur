public class GHudManager : GSingleton<GHudManager>
{
    public GPlayerHudManager playerHudManager { get; private set; }
    public GMainHudManager mainHudManager { get; private set; }
    public GPauseMenu pauseMenu { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        playerHudManager = GetComponentInChildren<GPlayerHudManager>();
        mainHudManager = GetComponentInChildren<GMainHudManager>();
        pauseMenu = GetComponentInChildren<GPauseMenu>();
    }
}
