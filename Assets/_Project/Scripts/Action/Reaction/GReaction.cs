using System;

public class GReaction : GAction
{
    public GPawn instigatorPawn;
    public GCell instigatorCell;
    
    public override void PreProcess(GActionContext context = null)
    {
        base.PreProcess();
    }

    public override void Start_Action()
    {
        base.Start_Action();
    }

    public override void Update_Action(float delta)
    {
        base.Update_Action(delta);
    }

    public override void End_Action()
    {
        base.End_Action();
    }
}