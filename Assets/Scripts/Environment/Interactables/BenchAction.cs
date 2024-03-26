using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchAction : Action
{
    public float sittingTime = 2f;
 
    public override void OnStateEnter()
    {
        player.Motor.SetMovementCollisionsSolvingActivation(false);
        player.Motor.SetGroundSolvingActivation(false);

        player._animator.CrossFade("sitting", 0.2f);
    }

    public override void SetInputs(PlayerCharacterInputs inputs)
    {
    }

    public override void BeforeCharacterUpdate()
    {
    }

    public override void UpdateRotation()
    {
        if (player.transform.rotation != transform.rotation)
            {
                player.Motor.SetRotation(transform.rotation);
            }
    }

    public override void UpdateVelocity()
    {
        if (player.transform.position != transform.position)
        {
            player.Motor.SetPosition(transform.position);
        }
        
    }

    public override void AfterCharacterUpdate()
    {
    }

    public override void OnStateExit()
    {
        Debug.Log("Exiting");
        
        player._animator.CrossFade("Idle", 0.2f);
        player.TransitionToState(CharacterState.Default);
    }
}
