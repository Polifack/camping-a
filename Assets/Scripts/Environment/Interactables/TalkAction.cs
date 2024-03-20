using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TalkAction : Action
{
    public override void OnStateEnter()
    {
        GameManager.instance.toggleTalkUI();
        GameManager.instance.printDialog("Tony", "carmela sexo");
    }

    public override void SetInputs(PlayerCharacterInputs inputs)
    {
        if (inputs.InteractDown){
            player.TransitionToState(CharacterState.Default);
        }
    }

    public override void BeforeCharacterUpdate()
    {
    }

    public override void UpdateRotation()
    {
    }

    public override void UpdateVelocity()
    {
    }

    public override void AfterCharacterUpdate()
    {
    }

    public override void OnStateExit()
    {
        GameManager.instance.toggleTalkUI();
    }
}
