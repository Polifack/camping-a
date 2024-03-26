using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableAction : Action
{
    public float sittingTime = 2f;
    public Transform[] sittingPositions;
    private int currentPos = 0;
 
    public override void OnStateEnter()
    {
        // take a random sitting position
        currentPos = Random.Range(0, sittingPositions.Length);

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
        if (player.transform.rotation != sittingPositions[currentPos].transform.rotation)
            {
                player.Motor.SetRotation(sittingPositions[currentPos].transform.rotation);
            }
    }

    public override void UpdateVelocity()
    {
        if (player.transform.position != sittingPositions[currentPos].position)
        {
            player.Motor.SetPosition(sittingPositions[currentPos].transform.position);
        }
        
    }

    public override void AfterCharacterUpdate()
    {
    }

    public override void OnStateExit()
    {
        Debug.Log("Exiting");
        
        player._animator.CrossFade("Locomotion", 0.08f);
    }
}
