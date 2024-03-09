using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFallState : PlayerBaseState, IRootState
{

    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public void HandleGravity()
    {
        float previousYVelocity = _ctx._currentMovement.y;
        _ctx._currentMovement.y = _ctx._currentMovement.y + (_ctx._gravity * _ctx._fallMultiplier * Time.deltaTime);
        _ctx._appliedMovement.y = Mathf.Max((previousYVelocity + _ctx._currentMovement.y) * .5f, -20f);
    }

    public override void EnterState()
    {
        InitializeSubState();
        _ctx._animator.SetBool("isFalling", true);
    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchState();
    }

    public override void ExitState()
    {
        _ctx._animator.SetBool("isFalling", false);
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
        //Perform two raycasts from the player's head and torso forward to check for a ledge
        if (_ctx._isGrounded){
            SwitchState(_factory.Grounded());
        }

        if (_ctx._canLedgeGrab){
            CheckLedgeGrabBoxCast();
        }

    }

    public override string ToString()
    {
        return "Fall State";
    }

    // Create a check for the ledge grab state using box colliders
    private void CheckLedgeGrabBoxCast()
    {
        Vector3 boxSize = new Vector3(_ctx._ledgeGrabSize.x, 0.05f, _ctx._ledgeGrabSize.y);
        RaycastHit headHit;
        RaycastHit torsoHit;

        // Player layer is 3, thus ignore it when casting
        // int layerMask = 1 << 3;
        // layerMask = ~layerMask;
        
        bool head = Physics.BoxCast(_ctx._headRaycast.position, boxSize, _ctx.transform.forward, out headHit, _ctx.transform.rotation, _ctx._ledgeGrabDistance, Physics.AllLayers);
        bool torso = Physics.BoxCast(_ctx._torsoRaycast.position, boxSize, _ctx.transform.forward, out torsoHit, _ctx.transform.rotation, _ctx._ledgeGrabDistance, Physics.AllLayers);

        if (!head)
        {
            if (torso)
            {
                // Get the ledge grab position and direction from the torso hit
                // and apply offset to the ledge grab position
                _ctx._ledgeGrabPosition = torsoHit.point;
                _ctx._ledgeGrabDirection = torsoHit.normal;
                _ctx._ledgeGrabPosition += _ctx._ledgeGrabDirection * _ctx._ledgeGrabOffset;

                // Switch to the ledge grab state
                SwitchState(_factory.LedgeGrab());
            }
        }
        
    }
    
}