using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdleState : PlayerBaseState
{

    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {

    }

    public override void EnterState()
    {
        _ctx._animator.SetBool("isRunning", false);
        _ctx._appliedMovement.x = 0;
        _ctx._appliedMovement.z = 0;
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
        if (_ctx._isMovementPressed)
            SwitchState(_factory.Run());
    }

    public override string ToString()
    {
        return "Idle State";
    }
}