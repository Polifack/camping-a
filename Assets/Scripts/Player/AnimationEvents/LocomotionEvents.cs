using Unity.VisualScripting;
using UnityEngine;

public class LocomotionEvents : StateMachineBehaviour
{
    public PlayerStateMachine playerStateMachine;
    bool _initialized = false;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_initialized)
        {
            playerStateMachine = animator.GetComponentInParent<PlayerStateMachine>();
            if (playerStateMachine == null)
                throw new System.InvalidOperationException(
                    $"State machine behaviour needs sibling/parent component of type {typeof(PlayerStateMachine)}");
            _initialized = true;
        }
        playerStateMachine.TransitionToState(CharacterState.Default);
    }
}