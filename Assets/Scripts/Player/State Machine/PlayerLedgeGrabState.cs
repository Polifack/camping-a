using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLedgeGrabState : PlayerBaseState, IRootState
{

    public PlayerLedgeGrabState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public void HandleGravity()
    {
        float previousYVelocity = _ctx._currentMovement.y;
        _ctx._currentMovement.y = 0f;
        _ctx._appliedMovement.y = 0f;
    }

    public override void EnterState()
    {
        InitializeSubState();
        _ctx._animator.SetBool("isLedgeGrabbing", true);
        _ctx._canMove = false;

        //Rotate the player to face the ledge
        _ctx.transform.rotation = Quaternion.LookRotation(-_ctx._ledgeGrabDirection, Vector3.up);
        //Move the player to the ledge grab position, which is the ledge grab raycast hit point plus an offset to prevent the player from clipping into the ledge
        _ctx.transform.position = _ctx.transform.position + (_ctx._ledgeGrabDirection * _ctx._ledgeGrabOffset);

    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchState();
    }

    public override void ExitState()
    {
        _ctx._animator.SetBool("isLedgeGrabbing", false);
        _ctx._canMove = true;
        //Disable the raycast for a certain amount of time in order to prevent the player from grabbing the same ledge again
        _ctx.StartCoroutine(WaitToReEnableLedgeGrab());
    }

    public override void InitializeSubState()
    {

    }

    public override void CheckSwitchState()
    {
        if (_ctx._isJumpPressed && _ctx._currentMovementInput.y > 0f){
            //Debug.Log("Jumping");
            SwitchState(_factory.Jump());
        }

        else if (_ctx._isJumpPressed && _ctx._currentMovementInput.y < 0f){
            //Debug.Log("Dropping off");
            SwitchState(_factory.Fall());
        }
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
        return "LedgeGrab State";
    }


}