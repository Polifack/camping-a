using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    public PlayerStateMachine playerStateMachine;
    public Animator anim;

    void Start()
    {
        playerStateMachine = GetComponentInParent<PlayerStateMachine>();
        anim = GetComponent<Animator>();
    }

    public void ReturnToDefaultState()
    {
        playerStateMachine.TransitionToState(CharacterState.Default);
    }
    
    // ------------------------------ Disable / Enable attacks
    public void Event_DisableAttack()
    {
        playerStateMachine.canAttack = false;
        anim.SetBool("finishedChargingAttack", false);
    }

    public void Event_EnableAttack()
    {
        playerStateMachine.canAttack = true;
    }

    // ------------------------------ Combo attack
    public void Event_NextCombo()
    {
        playerStateMachine.canAttack = true;
        anim.SetBool("finishedChargingAttack", true);

        //Check if the currentCombo is greater than the comboList length
        if (playerStateMachine.currentCombo >= playerStateMachine.comboList.Length - 1)
        {
            playerStateMachine.currentCombo = 0;
        }
        else
        {
            playerStateMachine.currentCombo++;
        }

    }

    public void FinishedChargingAttack()
    {
        anim.SetBool("finishedChargingAttack", true);
    }

    public void Event_ResetCombo()
    {
        playerStateMachine.canAttack = true;

        playerStateMachine.currentCombo = 0;

        anim.SetBool("finishedChargingAttack", false);
    }
}