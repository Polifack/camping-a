using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    public PlayerStateMachine playerStateMachine;

    public void ReturnToDefaultState()
    {
        playerStateMachine.TransitionToState(CharacterState.Default);
    }
    
}
