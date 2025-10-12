

public class GAction
{
    
    public virtual void Start_Action() {}

    public virtual void Update_Action(float delta) {}
    
    public virtual void End_Action() {}
    
    public virtual bool IsValid() { return false; } 
}