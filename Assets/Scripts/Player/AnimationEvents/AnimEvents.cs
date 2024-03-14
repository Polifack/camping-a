using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    public PlayerStateMachine playerStateMachine;

    void Start()
    {
        playerStateMachine = GetComponentInParent<PlayerStateMachine>();
    }

    public void ReturnToDefaultState()
    {
        playerStateMachine.TransitionToState(CharacterState.Default);
    }
    
}