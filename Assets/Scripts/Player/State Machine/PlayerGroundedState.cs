using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGroundedState : PlayerBaseState, IRootState
{

    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public void HandleGravity()
    {
        _ctx._currentMovement.y = -0.1f;
        _ctx._appliedMovement.y = -0.1f;
    }

    public override void EnterState()
    {
        InitializeSubState();
        HandleGravity();
    }

    public override void UpdateState()
    {
        CheckSwitchState();

        if (_ctx._isAttackPressed)
            //SetSubState(_factory.Attack1());
            _ctx._animator.CrossFade("2HAttack1", 0.1f);
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {
        if (_ctx._isMovementPressed)
            SetSubState(_factory.Run());
        else
            SetSubState(_factory.Idle());
    }

    public override void CheckSwitchState()
    {
        if (_ctx._isJumpPressed && !_ctx._requiredNewJumpPress){
            SwitchState(_factory.Jump());
        }
        else if (!_ctx._isGrounded){
            SwitchState(_factory.Fall());
        }
    }

    public override string ToString()
    {
        return "Grounded State";
    }

}