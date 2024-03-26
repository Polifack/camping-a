using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TalkAction : Action
{
    public string talkerName;
    public string talkContent;

    public override void OnStateEnter()
    {
        GameManager.instance.enableTalkUI();
        GameManager.instance.printDialog(talkerName, talkContent);
    }

    public override void SetInputs(PlayerCharacterInputs inputs)
    {
        if (inputs.InteractDown)
        {
            Debug.Log("InteractDown");
            if (GameManager.instance.isTalkingDone())
            {
                // player.TransitionToState(CharacterState.Default);
                // problem: just after the player exits the talk state it interacts again
                // solution: add a delay to the interact button
                GameManager.instance.disableTalkUI();
            }
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
        GameManager.instance.disableTalkUI();
    }
}
