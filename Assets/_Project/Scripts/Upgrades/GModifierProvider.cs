using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class GModifierProvider : IDisposable
{
    protected GAttributesController Controller;
    protected List<GAttributeModifier> Modifiers;
    protected bool _isPlayer;

    public virtual void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        Controller = attributesController;
        Modifiers = modifiers;
        _isPlayer = attributesController.GetComponent<GPawn>().data.isPlayer;
    }
    
    public virtual void Dispose(){}

    protected void AddModifiers()
    {
        Modifiers.ForEach(mod =>
        {
            GAttributeModifier modifier = new GAttributeModifier(mod);
            modifier.Source = this;
            Controller.AddModifier(modifier);
        });
    }

    protected void RemoveModifiers()
    {
        Modifiers.ForEach(mod =>
        {
            Controller.RemoveAllModifiersFromSource(this);
        });
    }
}

[Serializable]
public class GModifierProvider_OnKill : GModifierProvider
{
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        GPawn.OnKillEvent += OnKillCallback;
        if (_isPlayer)
            GTurnBaseManager.Instance.OnPostPlayerTurn += OnTurnEndCallback;
        else
            GTurnBaseManager.Instance.OnPrePlayerTurn += OnTurnEndCallback;
    }

    void OnKillCallback(GPawn killer, GPawn victim) {
        if (Controller.gameObject == killer.gameObject) AddModifiers();
    }

    void OnTurnEndCallback(int turn) => RemoveModifiers();


    public override void Dispose()
    {
        base.Dispose();
        GPawn.OnKillEvent -= OnKillCallback;
        if (_isPlayer)
            GTurnBaseManager.Instance.OnPostPlayerTurn -= OnTurnEndCallback;
        else
            GTurnBaseManager.Instance.OnPrePlayerTurn -= OnTurnEndCallback;
    }
}

[Serializable]
public class GModifierProvider_OnFirstMove : GModifierProvider
{
    [SerializeField]
    int numberOfActionsBeforeDisablingModifiers;
    int numberOfActionPlayed;
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        GTurnBaseManager.Instance.OnPrePlayerTurn += OnPlayerTurnStart;
        GPlayerController.OnPlayerActioOverEvent += OnActionOver;
    }

    void OnPlayerTurnStart(int fo)
    {
        AddModifiers();
        numberOfActionPlayed = 0;
    }

    void OnActionOver(GPawn p)
    {
        numberOfActionPlayed++;
        if (numberOfActionPlayed >= numberOfActionsBeforeDisablingModifiers)
        {
            RemoveModifiers();
        }
    }


    public override void Dispose()
    {
        base.Dispose();
        GTurnBaseManager.Instance.OnPrePlayerTurn -= OnPlayerTurnStart;
        GPlayerController.OnPlayerActioOverEvent -= OnActionOver;
    }
}

[Serializable]
public class GModifierProvider_OnPassReceived : GModifierProvider
{
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        GCrown.OnPass += OnPassReceivedCallback;
        if (_isPlayer)
            GTurnBaseManager.Instance.OnPostPlayerTurn -= OnTurnEndCallback;
        else
            GTurnBaseManager.Instance.OnPrePlayerTurn -= OnTurnEndCallback;
    }

    public void OnPassReceivedCallback(GPawn thrower, GPawn receiver, int swordDamageAmount)
    {
        if (receiver.gameObject == Controller.gameObject)
        {
            AddModifiers();
        }
    }

    public void OnTurnEndCallback(int fo) => RemoveModifiers();


    public override void Dispose()
    {
        base.Dispose();
        GCrown.OnPass -= OnPassReceivedCallback;
        if (_isPlayer)
            GTurnBaseManager.Instance.OnPostPlayerTurn -= OnTurnEndCallback;
        else
            GTurnBaseManager.Instance.OnPrePlayerTurn -= OnTurnEndCallback;
    }
}

[Serializable]
public class GModifierProvider_OnPassThrown : GModifierProvider
{
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        GCrown.OnPass += OnPassReceivedCallback;
        GTurnBaseManager.Instance.OnPostPlayerTurn -= OnTurnEndCallback;
    }

    public void OnPassReceivedCallback(GPawn thrower, GPawn receiver, int swordDamageAmount)
    {
        if (thrower.gameObject == Controller.gameObject)
        {
            AddModifiers();
        }
    }

    public void OnTurnEndCallback(int fo) => RemoveModifiers();


    public override void Dispose()
    {
        base.Dispose();
        GCrown.OnPass -= OnPassReceivedCallback;
        GTurnBaseManager.Instance.OnPostPlayerTurn -= OnTurnEndCallback;
    }
}

[Serializable]
public class GModifierProvider_OnSwordDamageReached : GModifierProvider
{
    [SerializeField]
    int numberOfDamageToEnableModifiers;

    bool isActive;
    
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        GCrown.OnPass += OnPassReceivedCallback;
        GCrown.OnSwordReset += OnSwordDamageReset;
    }

    public void OnPassReceivedCallback(GPawn thrower, GPawn receiver, int swordDamageAmount)
    {
        if (swordDamageAmount >= numberOfDamageToEnableModifiers)
        {
            isActive = true;
            AddModifiers();
        }
    }

    public void OnSwordDamageReset()
    {
        if (isActive)
        {
            isActive = false;
            RemoveModifiers();
        }
    }


    public override void Dispose()
    {
        base.Dispose();
        GCrown.OnPass -= OnPassReceivedCallback;
        GCrown.OnSwordReset -= OnSwordDamageReset;
    }
}

[Serializable]
public class GModifierProvider_OnFinishTurnNextTo : GModifierProvider
{
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        if (_isPlayer)
        {
            GTurnBaseManager.Instance.OnPrePlayerTurn += OnPassReceivedCallback;
            GTurnBaseManager.Instance.OnPostPlayerTurn += OnSwordDamageReset;
        }
        else
        {
            GTurnBaseManager.Instance.OnPostPlayerTurn += OnPassReceivedCallback;
            GTurnBaseManager.Instance.OnPrePlayerTurn += OnSwordDamageReset;
        }
    }

    public void OnPassReceivedCallback(int fo) => AddModifiers();

    public void OnSwordDamageReset(int fo) => RemoveModifiers();


    public override void Dispose()
    {
        base.Dispose();
        if (_isPlayer)
        {
            GTurnBaseManager.Instance.OnPrePlayerTurn -= OnPassReceivedCallback;
            GTurnBaseManager.Instance.OnPostPlayerTurn -= OnSwordDamageReset;
        }
        else
        {
            GTurnBaseManager.Instance.OnPostPlayerTurn -= OnPassReceivedCallback;
            GTurnBaseManager.Instance.OnPrePlayerTurn -= OnSwordDamageReset;
        }
    }
}

[Serializable]
public class GModifierProvider_OnPush : GModifierProvider
{
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        GPushAction.OnPushEvent += OnPushCallback;
        if (_isPlayer)
            GTurnBaseManager.Instance.OnPostPlayerTurn += OnTurnEndCallback;
        else
            GTurnBaseManager.Instance.OnPrePlayerTurn += OnTurnEndCallback;
    }

    void OnPushCallback(GPawn pusher, GPawn pushed) {
        if (Controller.gameObject == pusher.gameObject) AddModifiers();
    }

    void OnTurnEndCallback(int turn) => RemoveModifiers();


    public override void Dispose()
    {
        base.Dispose();
        GPushAction.OnPushEvent -= OnPushCallback;
        if (_isPlayer)
            GTurnBaseManager.Instance.OnPostPlayerTurn -= OnTurnEndCallback;
        else
            GTurnBaseManager.Instance.OnPrePlayerTurn -= OnTurnEndCallback;
    }
}

[Serializable]
public class GModifierProvider_OnThrow : GModifierProvider
{
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        GThrowAction.OnThrowEvent += OnThrowCallback;
        if (_isPlayer)
            GTurnBaseManager.Instance.OnPostPlayerTurn += OnTurnEndCallback;
        else
            GTurnBaseManager.Instance.OnPrePlayerTurn += OnTurnEndCallback;
    }

    void OnThrowCallback(GPawn pusher) {
        if (Controller.gameObject == pusher.gameObject) AddModifiers();
    }

    void OnTurnEndCallback(int turn) => RemoveModifiers();


    public override void Dispose()
    {
        base.Dispose();
        GThrowAction.OnThrowEvent -= OnThrowCallback;
        if (_isPlayer)
            GTurnBaseManager.Instance.OnPostPlayerTurn -= OnTurnEndCallback;
        else
            GTurnBaseManager.Instance.OnPrePlayerTurn -= OnTurnEndCallback;
    }
}

[Serializable]
public class GModifierProvider_OnNotMoveLastTurn : GModifierProvider
{
    [SerializeField]
    bool _condiderReactionAsMovement;
    
    bool hasMoved;
    bool isActive;
    
    public override void Init(GAttributesController attributesController, List<GAttributeModifier> modifiers)
    {
        base.Init(attributesController, modifiers);
        GTurnBaseManager.Instance.OnPrePlayerTurn += OnPlayersStartTurn;
        GTurnBaseManager.Instance.OnPostPlayerTurn += OnPlayersEndTurn;
        GMoveAction.OnMoveEvent += OnMoveCallback;
    }

    void OnPlayersStartTurn(int turn)
    {
        if (!hasMoved)
        {
            AddModifiers();
        }
        hasMoved = false;
    }

    void OnPlayersEndTurn(int turn)
    {
        if (isActive)
        {
            RemoveModifiers();
        }
    }
    
    void OnMoveCallback(GPawn pawn, bool isReaction)
    {
        if(pawn.gameObject == Controller.gameObject && (_condiderReactionAsMovement || (!_condiderReactionAsMovement && !isReaction)))
            hasMoved = true;
    }


    public override void Dispose()
    {
        base.Dispose();
        GTurnBaseManager.Instance.OnPrePlayerTurn -= OnPlayersStartTurn;
        GTurnBaseManager.Instance.OnPostPlayerTurn -= OnPlayersEndTurn;
        GMoveAction.OnMoveEvent -= OnMoveCallback;
    }
}
