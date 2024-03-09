using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack1State : PlayerBaseState
{

    public PlayerAttack1State(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {

    }

    public override void EnterState()
    {
        _ctx._animator.CrossFade("2HAttack1", 0.1f);
    }

    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {
        
    }

    public override void CheckSwitchState()
    {
        if (!_ctx._isMovementPressed)
            SwitchState(_factory.Idle());
    }

    public override string ToString()
    {
        return "Run State";
    }
}