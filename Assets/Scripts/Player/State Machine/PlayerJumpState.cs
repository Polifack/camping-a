using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJumpState : PlayerBaseState, IRootState
{

    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public void HandleGravity()
    {

        float previousYVelocity = _ctx._currentMovement.y;
        _ctx._currentMovement.y = _ctx._currentMovement.y + (_ctx._gravity * Time.deltaTime);
        _ctx._appliedMovement.y = (previousYVelocity + _ctx._currentMovement.y) * .5f;

    }

    public override void EnterState()
    {
        _ctx._animator.SetBool("isJumping", true);
        InitializeSubState();
        HandleJump();

        // Disable the raycast for a short amount of time to prevent the player from grabbing a ledge while the jump has just started
        _ctx.StartCoroutine(WaitToReEnableLedgeGrab());
    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchState();
    }

    public override void ExitState()
    {
        _ctx._animator.SetBool("isFalling", false);
        _ctx._animator.SetBool("isJumping", false);

        if (_ctx._isJumpPressed)
        {
            _ctx._requiredNewJumpPress = true;
        }

    }

    public override void InitializeSubState()
    {
        
    }

    public override void CheckSwitchState()
    {
        if (_ctx._isGrounded){
            SwitchState(_factory.Grounded());
        }
        else if (_ctx._currentMovement.y <= 0.0f || !_ctx._isJumpPressed)
            SwitchState(_factory.Fall());
    }

    void HandleJump() {
        _ctx._animator.SetBool("isJumping", true);
        _ctx.StartCoroutine(PlayJumpAnimation());

    }

    // This is a hack to make the jump animation play nicely
    IEnumerator PlayJumpAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        _ctx._isJumping = true;
        _ctx._currentMovement.y = _ctx._initialJumpVelocity;
        _ctx._appliedMovement.y = _ctx._initialJumpVelocity;
    }

    // Coroutine to wait for a certain amount of time before re enabling the raycast
    public IEnumerator WaitToReEnableLedgeGrab()
    {
        _ctx._canLedgeGrab = false;
        yield return new WaitForSeconds(0.5f);
        _ctx._canLedgeGrab = true;
    }

    public override string ToString()
    {
        return "Jump State";
    }
}