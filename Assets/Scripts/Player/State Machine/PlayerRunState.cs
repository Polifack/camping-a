using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunState : PlayerBaseState
{

    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {

    }

    public override void EnterState()
    {
        _ctx._animator.SetBool("isRunning", true);
    }

    public override void UpdateState()
    {
        _ctx._appliedMovement.x = _ctx._currentMovement.x;
        _ctx._appliedMovement.z = _ctx._currentMovement.z;
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