using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeAction : Action
{
    public AudioClip shitSound;
    public ParticleSystem shitParticles;


    public override void OnStateEnter()
    {
        player.Motor.SetMovementCollisionsSolvingActivation(false);
        player.Motor.SetGroundSolvingActivation(false);

        player._animator.CrossFade("sitting", 0.2f);
        GameManager.instance.audioSource.PlayOneShot(shitSound);
        shitParticles.Play();
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
        
        player._animator.CrossFade("Locomotion", 0.08f);
        GameManager.instance.audioSource.Stop();
        shitParticles.Stop();
    }
}
